using System.Collections.Generic;
using System.IO;
using System.Text;
using Defense.Editor.P03;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Defense.Editor.P04
{
    /// <summary>
    /// P04 S2 인간형(해골 궁수 · 좀비)을 찍고 잰다 — 비교 씬의 매끈 판 루트에 인간형 루트를 켜서 세로 · 근접 · 라인업 캡처와 48프레임 녹화,
    /// 그리고 가져온 모델의 리그 · 클립 반복 · 관절 접힘 각 · 활이 손을 따라가는지를 클립 샘플링으로 잰다.
    /// </summary>
    public static class P04HumanoidCapture
    {
        private const int PORTRAIT_WIDTH = 1080;
        private const int PORTRAIT_HEIGHT = 1920;
        private const int LINEUP_WIDTH = 1920;
        private const int LINEUP_HEIGHT = 1080;
        private const int CLOSE_SIZE = 1080;
        private const float BLENDER_FPS = 24f;   // Blender 프레임 번호(1 부터) → 클립 시간
        internal const float CAPTURE_TIME_SECONDS = 0.5f;

        internal const string IDLE_CLIP = "Idle";
        private const string HOP_CLIP = "Hop";
        private const string FLAP_CLIP = "Flap";
        internal const string WALK_CLIP = "Walk";
        internal const string SHOOT_CLIP = "Shoot";
        private const string ATTACK_CLIP = "Attack";
        internal const int SHOOT_DRAW_FRAME = 16;
        // 관절 최대 접힘 프레임 — Blender `P04_RESULT.models.*.joints` 에서 옮긴 값. Blender 동작을 고치면 같이 고친다
        private const int ARCHER_MAX_ELBOW_FRAME = 24;   // 오른팔꿈치 · Shoot
        private const int ARCHER_MAX_KNEE_FRAME = 19;    // 왼무릎 · Walk
        private const int ZOMBIE_MAX_ELBOW_FRAME = 10;   // 왼팔꿈치 · Attack
        private const int ZOMBIE_MAX_KNEE_FRAME = 25;    // 왼무릎 · Walk
        private static readonly int[] BOW_FRAMES = { 1, 6, 16, 21, 31 };
        private const string BOW_RENDERER_NAME = "SkeletonArcherBow";
        private const string RIG_SUFFIX = "Rig";

        /// <summary>세로 1 · 근접 3 · 라인업 1 을 outDir 에 쓰고 씬을 디스크에서 다시 연다. 장마다 한 줄씩 돌려준다.</summary>
        public static string CaptureAll(string outDir)
        {
            Scene scene = EditorSceneManager.OpenScene(P04SampleSceneBuilder.SCENE_PATH, OpenSceneMode.Single);
            Transform smooth = P03FrameRecorder.GetRoot(scene, P04SampleSceneBuilder.SMOOTH_ROOT_NAME);
            Transform humanoids = P03FrameRecorder.GetRoot(scene, P04HumanoidSceneBuilder.HUMANOIDS_ROOT_NAME);
            Transform lineup5 = P03FrameRecorder.GetRoot(scene, P04HumanoidSceneBuilder.LINEUP5_ROOT_NAME);
            Clips clips = new Clips();
            Directory.CreateDirectory(outDir);
            StringBuilder report = new StringBuilder();
            // 같은 에디터 프레임에서 자세를 바꿔 두 번째로 렌더하면 스킨 행렬이 그대로라 앞 장과 같은 자세가 찍힌다(이 세션 실측 — 녹화와 같은 처방)
            ForceSkinningPerRender(smooth, humanoids, lineup5);

            ShowOnly(scene, smooth, humanoids);
            PoseEnemies(smooth, clips, CAPTURE_TIME_SECONDS);
            PoseHumanoids(humanoids, clips, clips.Archer[SHOOT_CLIP], ToTime(SHOOT_DRAW_FRAME), clips.Zombie[WALK_CLIP], CAPTURE_TIME_SECONDS);
            Render(smooth.Find(P03SampleSceneBuilder.CAMERA_NAME), Path.Combine(outDir, "P04S2_Portrait_Lit.png"), PORTRAIT_WIDTH, PORTRAIT_HEIGHT, report);
            Render(humanoids.Find(P04HumanoidSceneBuilder.ARCHER_FACE_CAMERA_NAME), Path.Combine(outDir, "P04S2_Close_ArcherFaceBow_Lit.png"), CLOSE_SIZE, CLOSE_SIZE, report);
            PoseHumanoids(humanoids, clips, clips.Archer[SHOOT_CLIP], ToTime(ARCHER_MAX_ELBOW_FRAME), clips.Zombie[WALK_CLIP], ToTime(ZOMBIE_MAX_KNEE_FRAME));
            Render(humanoids.Find(P04HumanoidSceneBuilder.ARCHER_FACE_CAMERA_NAME), Path.Combine(outDir, "P04S2_Close_ArcherShoot24_Lit.png"), CLOSE_SIZE, CLOSE_SIZE, report);
            Render(humanoids.Find(P04HumanoidSceneBuilder.ZOMBIE_SIDE_CAMERA_NAME), Path.Combine(outDir, "P04S2_Close_ZombieWalk25_Lit.png"), CLOSE_SIZE, CLOSE_SIZE, report);

            ShowOnly(scene, lineup5);
            clips.Archer[IDLE_CLIP].SampleAnimation(lineup5.Find(P04HumanoidSceneBuilder.LINEUP5_ARCHER_NAME).gameObject, 0f);
            clips.Zombie[IDLE_CLIP].SampleAnimation(lineup5.Find(P04HumanoidSceneBuilder.LINEUP5_ZOMBIE_NAME).gameObject, 0f);
            Render(lineup5.Find(P04HumanoidSceneBuilder.LINEUP5_CAMERA_NAME), Path.Combine(outDir, "P04S2_Lineup5_Lit.png"), LINEUP_WIDTH, LINEUP_HEIGHT, report);

            EditorSceneManager.OpenScene(P04SampleSceneBuilder.SCENE_PATH, OpenSceneMode.Single);
            return report.ToString();
        }

        /// <summary>
        /// 매끈 판 루트 + 인간형 루트를 frameCount 장 녹화한다 — 슬라임 Idle · Hop · 박쥐 Flap(P03 녹화와 같은 규칙) · 궁수 Shoot · 좀비 Walk, 개체마다 위상이 다르다.
        /// 스킨 메시는 렌더마다 행렬을 다시 계산한다(P03 발견 ④). 끝나면 씬을 디스크에서 다시 연다.
        /// </summary>
        public static string Record(string outDir, int frameCount, int fps, int width, int height)
        {
            Scene scene = EditorSceneManager.OpenScene(P04SampleSceneBuilder.SCENE_PATH, OpenSceneMode.Single);
            Transform smooth = P03FrameRecorder.GetRoot(scene, P04SampleSceneBuilder.SMOOTH_ROOT_NAME);
            Transform humanoids = P03FrameRecorder.GetRoot(scene, P04HumanoidSceneBuilder.HUMANOIDS_ROOT_NAME);
            ShowOnly(scene, smooth, humanoids);
            int skinnedCount = ForceSkinningPerRender(smooth, humanoids);
            Clips clips = new Clips();
            Camera camera = smooth.Find(P03SampleSceneBuilder.CAMERA_NAME).GetComponent<Camera>();
            Directory.CreateDirectory(outDir);
            for (int frame = 0; frame < frameCount; ++frame)
            {
                float time = (float)frame / fps;
                PoseEnemies(smooth, clips, time);
                PoseHumanoids(humanoids, clips, clips.Archer[SHOOT_CLIP], time, clips.Zombie[WALK_CLIP], time);
                P03FrameRecorder.RenderToPng(camera, Path.Combine(outDir, "frame_" + frame.ToString("000") + ".png"), width, height);
            }
            string summary = outDir + " frames=" + frameCount + " fps=" + fps + " size=" + width + "x" + height + " skinned=" + skinnedCount
                + " shoot/walk(archer)/walk(zombie) length=" + clips.Archer[SHOOT_CLIP].length.ToString("0.###") + "/" + clips.Archer[WALK_CLIP].length.ToString("0.###")
                + "/" + clips.Zombie[WALK_CLIP].length.ToString("0.###");
            EditorSceneManager.OpenScene(P04SampleSceneBuilder.SCENE_PATH, OpenSceneMode.Single);
            return summary;
        }

        /// <summary>
        /// 인간형 모델마다 — 뼈대 오브젝트 회전 · 클립(길이 · 반복) · Walk 첫 · 끝 자세 차이 · 루트 이동(루트 모션) · 관절 접힘 각 · 활 중심과 왼손 뼈 사이 거리.
        /// 모델을 지금 씬에 잠시 복제했다가 지운다 — 씬은 저장하지 않는다.
        /// </summary>
        public static string Measure()
        {
            StringBuilder report = new StringBuilder();
            Clips clips = new Clips();
            MeasureModel(P04ModelImport.ARCHER_MODEL_PATH, clips.Archer, report,
                new JointProbe("ElbowR", "UpperArmR", "ForeArmR", "HandR", SHOOT_CLIP, ARCHER_MAX_ELBOW_FRAME),
                new JointProbe("KneeL", "ThighL", "ShinL", null, WALK_CLIP, ARCHER_MAX_KNEE_FRAME));
            MeasureModel(P04ModelImport.ZOMBIE_MODEL_PATH, clips.Zombie, report,
                new JointProbe("ElbowL", "UpperArmL", "ForeArmL", "HandL", ATTACK_CLIP, ZOMBIE_MAX_ELBOW_FRAME),
                new JointProbe("KneeL", "ThighL", "ShinL", null, WALK_CLIP, ZOMBIE_MAX_KNEE_FRAME));
            return report.ToString();
        }

        private static void MeasureModel(string path, Dictionary<string, AnimationClip> clips, StringBuilder report, params JointProbe[] probes)
        {
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Debug.Assert(model != null, "모델이 없다: " + path);
            GameObject instance = Object.Instantiate(model);
            Transform rig = null;
            foreach (Transform child in instance.transform)
            {
                if (child.name.EndsWith(RIG_SUFFIX))
                {
                    rig = child;
                }
            }
            Debug.Assert(rig != null, "뼈대 오브젝트가 없다: " + path);
            report.Append(path).Append(" rig=").Append(rig.name).Append(" rigEuler=").Append(rig.localEulerAngles.ToString("0.0"))
                .Append(" bones=").Append(rig.GetComponentsInChildren<Transform>(true).Length - 1).Append('\n');
            foreach (KeyValuePair<string, AnimationClip> pair in clips)
            {
                report.Append("  clip ").Append(pair.Key).Append(" length=").Append(pair.Value.length.ToString("0.###"))
                    .Append(" frameRate=").Append(pair.Value.frameRate).Append(" loop=").Append(pair.Value.isLooping).Append('\n');
            }

            // Walk 반복 — 첫 프레임과 끝 프레임의 뼈마다 위치 · 회전 차이, 그리고 반 주기에서 모델 루트가 움직였는가(루트 모션)
            Transform[] bones = rig.GetComponentsInChildren<Transform>(true);
            AnimationClip walk = clips[WALK_CLIP];
            Vector3[] firstPositions = new Vector3[bones.Length];
            Quaternion[] firstRotations = new Quaternion[bones.Length];
            walk.SampleAnimation(instance, 0f);
            for (int i = 0; i < bones.Length; ++i)
            {
                firstPositions[i] = bones[i].localPosition;
                firstRotations[i] = bones[i].localRotation;
            }
            walk.SampleAnimation(instance, walk.length);
            float maxPosition = 0f;
            float maxAngle = 0f;
            for (int i = 0; i < bones.Length; ++i)
            {
                maxPosition = Mathf.Max(maxPosition, (bones[i].localPosition - firstPositions[i]).magnitude);
                maxAngle = Mathf.Max(maxAngle, Quaternion.Angle(bones[i].localRotation, firstRotations[i]));
            }
            Transform hips = FindDeep(rig, "Hips");
            walk.SampleAnimation(instance, walk.length * 0.25f);
            Vector3 hipsQuarter = hips.position;
            walk.SampleAnimation(instance, walk.length * 0.5f);
            report.Append("  walk first-last maxPos=").Append(maxPosition.ToString("0.000000")).Append(" maxAngle=").Append(maxAngle.ToString("0.0000"))
                .Append(" modelRootMoved=").Append(instance.transform.position.magnitude.ToString("0.000000"))
                .Append(" hipsY quarter/half=").Append(hipsQuarter.y.ToString("0.000")).Append('/').Append(hips.position.y.ToString("0.000")).Append('\n');

            foreach (JointProbe probe in probes)
            {
                clips[probe.Clip].SampleAnimation(instance, ToTime(probe.Frame));
                Transform upper = FindDeep(rig, probe.Upper);
                Transform lower = FindDeep(rig, probe.Lower);
                Vector3 upperDirection = lower.position - upper.position;
                // 아래 뼈 방향 — 끝 자식(손)이 있으면 그 위치, 없으면 위 뼈 방향을 위 뼈 기준 공간으로 옮겨 아래 뼈 회전에 싣는다(두 뼈의 쉼 자세 축이 같다 — 허벅지 · 정강이 모두 곧게 아래)
                Vector3 lowerDirection = probe.End != null
                    ? FindDeep(rig, probe.End).position - lower.position
                    : lower.rotation * (Quaternion.Inverse(upper.rotation) * upperDirection);
                report.Append("  fold ").Append(probe.Label).Append(' ').Append(probe.Clip).Append(" f").Append(probe.Frame)
                    .Append(" deg=").Append(Vector3.Angle(upperDirection, lowerDirection).ToString("0.0")).Append('\n');
            }

            foreach (SkinnedMeshRenderer skinned in instance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (skinned.name != BOW_RENDERER_NAME)
                {
                    continue;
                }
                Transform hand = FindDeep(rig, "HandL");
                Mesh baked = new Mesh();
                report.Append("  bow centroid-handL");
                foreach (int frame in BOW_FRAMES)
                {
                    clips[SHOOT_CLIP].SampleAnimation(instance, ToTime(frame));
                    skinned.BakeMesh(baked, true);
                    // BakeMesh(useScale) 의 정점은 렌더러 기준 공간에 크기까지 들어 있다 — 위치 · 회전만 더한다(P04Capture.MeasureHop 과 같다)
                    Matrix4x4 toWorld = Matrix4x4.TRS(skinned.transform.position, skinned.transform.rotation, Vector3.one);
                    Vector3 sum = Vector3.zero;
                    Vector3[] vertices = baked.vertices;
                    foreach (Vector3 vertex in vertices)
                    {
                        sum += toWorld.MultiplyPoint3x4(vertex);
                    }
                    report.Append(" f").Append(frame).Append('=').Append(((sum / vertices.Length) - hand.position).magnitude.ToString("0.0000"));
                }
                report.Append('\n');
                Object.DestroyImmediate(baked);
            }
            Object.DestroyImmediate(instance);
        }

        /// <summary>루트들 아래 스킨 메시가 렌더마다 행렬을 다시 계산하게 한다(P03 발견 ④). 켠 수를 돌려준다.</summary>
        internal static int ForceSkinningPerRender(params Transform[] roots)
        {
            int count = 0;
            foreach (Transform root in roots)
            {
                foreach (SkinnedMeshRenderer skinned in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    skinned.forceMatrixRecalculationPerRender = true;
                    ++count;
                }
            }
            return count;
        }

        internal static void ShowOnly(Scene scene, params Transform[] keep)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                root.SetActive(System.Array.IndexOf(keep, root.transform) >= 0);
            }
        }

        /// <summary>슬라임(짝수 Idle · 홀수 Hop) · 박쥐 Flap — P03 녹화와 같은 위상 규칙.</summary>
        internal static void PoseEnemies(Transform smooth, Clips clips, float time)
        {
            Transform slimes = smooth.Find(P03SampleSceneBuilder.SLIMES_NAME);
            Transform bats = smooth.Find(P03SampleSceneBuilder.BATS_NAME);
            for (int i = 0; i < slimes.childCount; ++i)
            {
                AnimationClip clip = clips.Slime[i % 2 == 0 ? IDLE_CLIP : HOP_CLIP];
                clip.SampleAnimation(slimes.GetChild(i).gameObject, Mathf.Repeat(time + clip.length * i / slimes.childCount, clip.length));
            }
            AnimationClip flap = clips.Bat[FLAP_CLIP];
            for (int i = 0; i < bats.childCount; ++i)
            {
                flap.SampleAnimation(bats.GetChild(i).gameObject, Mathf.Repeat(time + flap.length * i / bats.childCount, flap.length));
            }
        }

        /// <summary>궁수 · 좀비 — 첫 개체는 주어진 시간 그대로(근접 카메라가 본다), 나머지는 위상을 나눠 엇갈린다.</summary>
        internal static void PoseHumanoids(Transform humanoids, Clips clips, AnimationClip archerClip, float archerTime, AnimationClip zombieClip, float zombieTime)
        {
            SampleGroup(humanoids.Find(P04HumanoidSceneBuilder.ARCHERS_NAME), archerClip, archerTime);
            SampleGroup(humanoids.Find(P04HumanoidSceneBuilder.ZOMBIES_NAME), zombieClip, zombieTime);
        }

        private static void SampleGroup(Transform group, AnimationClip clip, float time)
        {
            for (int i = 0; i < group.childCount; ++i)
            {
                clip.SampleAnimation(group.GetChild(i).gameObject, Mathf.Repeat(time + clip.length * i / group.childCount, clip.length));
            }
        }

        private static void Render(Transform cameraTransform, string path, int width, int height, StringBuilder report)
        {
            P03FrameRecorder.RenderToPng(cameraTransform.GetComponent<Camera>(), path, width, height);
            report.Append(path).Append(' ').Append(width).Append('x').Append(height).Append('\n');
        }

        internal static float ToTime(int blenderFrame)
        {
            return (blenderFrame - 1) / BLENDER_FPS;
        }

        private static Transform FindDeep(Transform parent, string name)
        {
            foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == name)
                {
                    return child;
                }
            }
            Debug.Assert(false, "뼈가 없다: " + name);
            return null;
        }

        /// <summary>모델 넷의 클립 — 이름(뼈대 접두 · `.NNN` 을 벗긴 것)으로 찾는다.</summary>
        private readonly struct JointProbe
        {
            public readonly string Label;
            public readonly string Upper;
            public readonly string Lower;
            public readonly string End;   // 아래 뼈 꼬리 쪽 자식(손) — 없으면 null(정강이는 FBX 에 끝 뼈가 없다)
            public readonly string Clip;
            public readonly int Frame;

            public JointProbe(string label, string upper, string lower, string end, string clip, int frame)
            {
                Label = label;
                Upper = upper;
                Lower = lower;
                End = end;
                Clip = clip;
                Frame = frame;
            }
        }

        internal sealed class Clips
        {
            public readonly Dictionary<string, AnimationClip> Slime = Load(P04ModelImport.SLIME_MODEL_PATH);
            public readonly Dictionary<string, AnimationClip> Bat = Load(P04ModelImport.BAT_MODEL_PATH);
            public readonly Dictionary<string, AnimationClip> Archer = Load(P04ModelImport.ARCHER_MODEL_PATH);
            public readonly Dictionary<string, AnimationClip> Zombie = Load(P04ModelImport.ZOMBIE_MODEL_PATH);

            private static Dictionary<string, AnimationClip> Load(string modelPath)
            {
                Dictionary<string, AnimationClip> clips = new Dictionary<string, AnimationClip>();
                foreach (Object asset in AssetDatabase.LoadAllAssetRepresentationsAtPath(modelPath))
                {
                    AnimationClip clip = asset as AnimationClip;
                    if (clip != null)
                    {
                        Debug.Assert(!clips.ContainsKey(clip.name), "같은 이름 클립이 둘: " + modelPath + " " + clip.name);
                        clips[clip.name] = clip;
                    }
                }
                return clips;
            }
        }
    }
}
