using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Defense.Editor.P03;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Defense.Editor.P04
{
    /// <summary>
    /// P04 S3 카툰 렌더링 시험 캡처 — 매끈 판 5종(슬라임 · 박쥐 · 등불 탑 · 해골 궁수 · 좀비)을 한 머티리얼로 칠해
    /// 세로 1 · 라인업 1 · 근접 2(궁수 얼굴 · 슬라임 얼굴)를 품질 단계(Mobile · PC)마다 찍는다.
    /// 품질 단계는 찍는 동안만 바꾸고 원래 값으로 되돌린다. 씬은 디스크에서 다시 열어 바꾼 것을 버린다.
    /// </summary>
    public static class P04ToonCapture
    {
        public const int MOBILE_QUALITY = 0;
        public const int PC_QUALITY = 1;

        private const int PORTRAIT_WIDTH = 1080;
        private const int PORTRAIT_HEIGHT = 1920;
        private const int LINEUP_WIDTH = 1920;
        private const int LINEUP_HEIGHT = 1080;
        private const int CLOSE_SIZE = 1080;

        private const string CAMERA_DATA_TYPE_NAME = "UniversalAdditionalCameraData";
        private const string CAMERA_DATA_ASSEMBLY_QUALIFIED_NAME = "UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime";

        private const string VOLUME_ASSEMBLY_QUALIFIED_NAME = "UnityEngine.Rendering.Volume, Unity.RenderPipelines.Core.Runtime";
        // PaletteToon.shader 의 CHARACTER_LAYER 와 같은 비트(렌더링 레이어 1)
        private const uint CHARACTER_LAYER_MASK = 2u;
        private const float BLOB_LIFT_METERS = 0.01f;
        private const float BLOB_SIZE_RATIO = 1.1f;

        private static readonly int[] BOTH_QUALITIES = { MOBILE_QUALITY, PC_QUALITY };

        /// <summary>캡처 한 벌의 조건 — 비우면 에셋 머티리얼 그대로 · 두 품질 · 기본 렌더러 · 후처리 끔.</summary>
        public sealed class Options
        {
            /// <summary>있으면 씬을 연 뒤 머티리얼 사본을 만들어 적용한다(에셋은 그대로).</summary>
            public System.Action<Material> Configure;
            /// <summary>있으면 씬을 연 뒤 · 찍기 전에 씬 인스턴스를 바꾼다(블롭 · 렌더링 레이어 · 볼륨). 저장하지 않는다.</summary>
            public System.Action<Scene> PrepareScene;
            public int[] Qualities = BOTH_QUALITIES;
            /// <summary>0 이상이면 그 렌더러로 찍는다(시험 렌더러 — 품질의 RP 에셋 목록에 있어야 한다).</summary>
            public int RendererIndex = -1;
            public bool IsPostProcessing;
        }

        /// <summary>
        /// label 이름으로 품질마다 4장을 outDir 에 쓴다 — `{label}_{Mobile|PC}_{Portrait|Lineup5|CloseArcher|CloseSlime}.png`.
        /// </summary>
        public static string CaptureSet(string outDir, string label, Material material, Options options = null)
        {
            Debug.Assert(material != null, "머티리얼이 없다: " + label);
            if (options == null)
            {
                options = new Options();
            }
            int originalQuality = QualitySettings.GetQualityLevel();
            Directory.CreateDirectory(outDir);
            StringBuilder report = new StringBuilder();
            foreach (int quality in options.Qualities)
            {
                QualitySettings.SetQualityLevel(quality, true);
                string prefix = Path.Combine(outDir, label + "_" + GetQualityName(quality) + "_");
                Scene scene = EditorSceneManager.OpenScene(P04SampleSceneBuilder.SCENE_PATH, OpenSceneMode.Single);
                Transform smooth = P03FrameRecorder.GetRoot(scene, P04SampleSceneBuilder.SMOOTH_ROOT_NAME);
                Transform humanoids = P03FrameRecorder.GetRoot(scene, P04HumanoidSceneBuilder.HUMANOIDS_ROOT_NAME);
                Transform lineup5 = P03FrameRecorder.GetRoot(scene, P04HumanoidSceneBuilder.LINEUP5_ROOT_NAME);
                // 사본은 씬을 연 뒤에 만든다 — 연기 전에 만든 사본은 Single 열기가 셰이더를 언로드해 오류 셰이더(자홍색)로 그려진다(이 세션 실측)
                Material used = material;
                if (options.Configure != null)
                {
                    used = new Material(material);
                    used.name = label;
                    options.Configure(used);
                }
                foreach (Transform root in new[] { smooth, humanoids, lineup5 })
                {
                    P04SampleSceneBuilder.ApplyMaterial(root.gameObject, used);
                }
                // 캐릭터 표시는 늘 — PaletteToon 의 캐릭터 전용 키워드가 꺼져 있으면 그림이 바뀌지 않는다
                MarkCharacters(scene);
                // 씬 준비는 머티리얼을 칠한 뒤 — 먼저 하면 블롭 쿼드까지 팔레트 머티리얼로 덮인다(이 세션 실측)
                if (options.PrepareScene != null)
                {
                    options.PrepareScene(scene);
                }
                P04HumanoidCapture.ForceSkinningPerRender(smooth, humanoids, lineup5);
                P04HumanoidCapture.Clips clips = new P04HumanoidCapture.Clips();

                P04HumanoidCapture.ShowOnly(scene, smooth, humanoids);
                P04HumanoidCapture.PoseEnemies(smooth, clips, P04HumanoidCapture.CAPTURE_TIME_SECONDS);
                P04HumanoidCapture.PoseHumanoids(humanoids, clips,
                    clips.Archer[P04HumanoidCapture.SHOOT_CLIP], P04HumanoidCapture.ToTime(P04HumanoidCapture.SHOOT_DRAW_FRAME),
                    clips.Zombie[P04HumanoidCapture.WALK_CLIP], P04HumanoidCapture.CAPTURE_TIME_SECONDS);
                Render(smooth.Find(P03SampleSceneBuilder.CAMERA_NAME), prefix + "Portrait.png", PORTRAIT_WIDTH, PORTRAIT_HEIGHT, options.RendererIndex, options.IsPostProcessing, report);
                Render(humanoids.Find(P04HumanoidSceneBuilder.ARCHER_FACE_CAMERA_NAME), prefix + "CloseArcher.png", CLOSE_SIZE, CLOSE_SIZE, options.RendererIndex, options.IsPostProcessing, report);
                Render(smooth.Find(P04SampleSceneBuilder.FACE_CAMERA_NAME), prefix + "CloseSlime.png", CLOSE_SIZE, CLOSE_SIZE, options.RendererIndex, options.IsPostProcessing, report);

                P04HumanoidCapture.ShowOnly(scene, lineup5);
                clips.Archer[P04HumanoidCapture.IDLE_CLIP].SampleAnimation(lineup5.Find(P04HumanoidSceneBuilder.LINEUP5_ARCHER_NAME).gameObject, 0f);
                clips.Zombie[P04HumanoidCapture.IDLE_CLIP].SampleAnimation(lineup5.Find(P04HumanoidSceneBuilder.LINEUP5_ZOMBIE_NAME).gameObject, 0f);
                Render(lineup5.Find(P04HumanoidSceneBuilder.LINEUP5_CAMERA_NAME), prefix + "Lineup5.png", LINEUP_WIDTH, LINEUP_HEIGHT, options.RendererIndex, options.IsPostProcessing, report);
                if (used != material)
                {
                    Object.DestroyImmediate(used);
                }
            }
            QualitySettings.SetQualityLevel(originalQuality, true);
            EditorSceneManager.OpenScene(P04SampleSceneBuilder.SCENE_PATH, OpenSceneMode.Single);
            return report.ToString();
        }

        /// <summary>
        /// 셰이더의 패스 · 변형 수와 세로 장면(매끈 판 + 인간형)의 렌더러 · 삼각형 · 정점 수를 한 줄로 — 「× 300」 환산은 보고에서 한다.
        /// UnityStats 는 마지막 렌더의 통계(창 없는 에디터에서 값이 나오는지도 이 출력으로 본다).
        /// </summary>
        /// <summary>
        /// 최종 조합 녹화 — 품질 하나에서 매끈 판 루트 + 인간형 루트를 frameCount 장. 슬라임 Idle · Hop · 박쥐 Flap(P03 규칙),
        /// 궁수는 짝수 Shoot · 홀수 Walk, 좀비는 짝수 Walk · 홀수 Attack, 개체마다 위상이 다르다. 스킨 메시는 렌더마다 행렬 재계산.
        /// options 의 Configure · PrepareScene 을 캡처와 같이 따른다. 끝나면 품질 · 씬을 되돌린다.
        /// </summary>
        public static string Record(string outDir, int quality, int frameCount, int fps, int width, int height, Material material, Options options)
        {
            int originalQuality = QualitySettings.GetQualityLevel();
            QualitySettings.SetQualityLevel(quality, true);
            Scene scene = EditorSceneManager.OpenScene(P04SampleSceneBuilder.SCENE_PATH, OpenSceneMode.Single);
            Transform smooth = P03FrameRecorder.GetRoot(scene, P04SampleSceneBuilder.SMOOTH_ROOT_NAME);
            Transform humanoids = P03FrameRecorder.GetRoot(scene, P04HumanoidSceneBuilder.HUMANOIDS_ROOT_NAME);
            Material used = material;
            if (options != null && options.Configure != null)
            {
                used = new Material(material);
                options.Configure(used);
            }
            foreach (Transform root in new[] { smooth, humanoids })
            {
                P04SampleSceneBuilder.ApplyMaterial(root.gameObject, used);
            }
            MarkCharacters(scene);
            if (options != null && options.PrepareScene != null)
            {
                options.PrepareScene(scene);
            }
            P04HumanoidCapture.ShowOnly(scene, smooth, humanoids);
            int skinnedCount = P04HumanoidCapture.ForceSkinningPerRender(smooth, humanoids);
            P04HumanoidCapture.Clips clips = new P04HumanoidCapture.Clips();
            Transform archers = humanoids.Find(P04HumanoidSceneBuilder.ARCHERS_NAME);
            Transform zombies = humanoids.Find(P04HumanoidSceneBuilder.ZOMBIES_NAME);
            Camera camera = smooth.Find(P03SampleSceneBuilder.CAMERA_NAME).GetComponent<Camera>();
            Directory.CreateDirectory(outDir);
            for (int frame = 0; frame < frameCount; ++frame)
            {
                float time = (float)frame / fps;
                P04HumanoidCapture.PoseEnemies(smooth, clips, time);
                SampleAlternating(archers, clips.Archer[P04HumanoidCapture.SHOOT_CLIP], clips.Archer[P04HumanoidCapture.WALK_CLIP], time);
                SampleAlternating(zombies, clips.Zombie[P04HumanoidCapture.WALK_CLIP], clips.Zombie[P04HumanoidCapture.ATTACK_CLIP], time);
                P03FrameRecorder.RenderToPng(camera, Path.Combine(outDir, "frame_" + frame.ToString("000") + ".png"), width, height);
            }
            string summary = outDir + " quality=" + GetQualityName(quality) + " frames=" + frameCount + " fps=" + fps + " size=" + width + "x" + height + " skinned=" + skinnedCount;
            QualitySettings.SetQualityLevel(originalQuality, true);
            EditorSceneManager.OpenScene(P04SampleSceneBuilder.SCENE_PATH, OpenSceneMode.Single);
            return summary;
        }

        // 짝수 개체는 even 클립, 홀수 개체는 odd 클립 — 위상은 개체 순서로 나눈다
        private static void SampleAlternating(Transform group, AnimationClip even, AnimationClip odd, float time)
        {
            for (int i = 0; i < group.childCount; ++i)
            {
                AnimationClip clip = i % 2 == 0 ? even : odd;
                clip.SampleAnimation(group.GetChild(i).gameObject, Mathf.Repeat(time + clip.length * i / group.childCount, clip.length));
            }
        }

        public static string MeasureCost(Material material)
        {
            Shader shader = material.shader;
            StringBuilder report = new StringBuilder();
            report.Append(shader.name).Append(" passes=").Append(shader.GetPassCountInSubshader(0)).Append(" [");
            for (int i = 0; i < shader.GetPassCountInSubshader(0); ++i)
            {
                report.Append(i == 0 ? "" : ",").Append(shader.FindPassTagValue(i, new ShaderTagId("LightMode")).name);
            }
            report.Append("] keywords=").Append(string.Join(",", material.shaderKeywords));
            report.Append(" variants(all/scene)=").Append(GetVariantCount(shader, false)).Append('/').Append(GetVariantCount(shader, true));

            Scene scene = EditorSceneManager.OpenScene(P04SampleSceneBuilder.SCENE_PATH, OpenSceneMode.Single);
            Transform smooth = P03FrameRecorder.GetRoot(scene, P04SampleSceneBuilder.SMOOTH_ROOT_NAME);
            Transform humanoids = P03FrameRecorder.GetRoot(scene, P04HumanoidSceneBuilder.HUMANOIDS_ROOT_NAME);
            P04HumanoidCapture.ShowOnly(scene, smooth, humanoids);
            int renderers = 0;
            int skinned = 0;
            long triangles = 0;
            long vertices = 0;
            foreach (Transform root in new[] { smooth, humanoids })
            {
                P04SampleSceneBuilder.ApplyMaterial(root.gameObject, material);
                foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    Mesh mesh = GetMesh(renderer);
                    if (mesh == null)
                    {
                        continue;
                    }
                    ++renderers;
                    skinned += renderer is SkinnedMeshRenderer ? 1 : 0;
                    vertices += mesh.vertexCount;
                    for (int sub = 0; sub < mesh.subMeshCount; ++sub)
                    {
                        triangles += mesh.GetIndexCount(sub) / 3;
                    }
                }
            }
            Camera camera = smooth.Find(P03SampleSceneBuilder.CAMERA_NAME).GetComponent<Camera>();
            string path = Path.Combine(Application.temporaryCachePath, "P04ToonCost.png");
            P03FrameRecorder.RenderToPng(camera, path, PORTRAIT_WIDTH, PORTRAIT_HEIGHT);
            report.Append(" quality=").Append(GetQualityName(QualitySettings.GetQualityLevel()))
                .Append(" renderers=").Append(renderers).Append(" skinned=").Append(skinned)
                .Append(" tris=").Append(triangles).Append(" verts=").Append(vertices)
                .Append(" stats(draw/batch/setpass/tris/verts)=").Append(UnityStats.drawCalls).Append('/').Append(UnityStats.batches)
                .Append('/').Append(UnityStats.setPassCalls).Append('/').Append(UnityStats.triangles).Append('/').Append(UnityStats.vertices);
            EditorSceneManager.OpenScene(P04SampleSceneBuilder.SCENE_PATH, OpenSceneMode.Single);
            return report.ToString();
        }

        /// <summary>씬의 스킨 메시(캐릭터)에 렌더링 레이어 1 을 켠다 — PaletteToon 의 캐릭터 전용 기법(자기 그림자 끔 · 조명 고정 · 림 · 스페큘러)의 대상. 켠 수를 돌려준다.</summary>
        public static int MarkCharacters(Scene scene)
        {
            int count = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (SkinnedMeshRenderer skinned in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    skinned.renderingLayerMask |= CHARACTER_LAYER_MASK;
                    ++count;
                }
            }
            return count;
        }

        /// <summary>
        /// 캐릭터(스킨 메시를 가진 모델 인스턴스)마다 그림자 드리우기를 끄고 발밑에 블롭 쿼드를 놓는다 — 높이는 그 루트의 바닥(y 0).
        /// 등불 탑 같은 정적 모델은 실시간 그림자를 그대로 둔다. 놓은 수를 돌려준다.
        /// </summary>
        public static int UseBlobShadows(Scene scene)
        {
            Material blob = P04ToonAssets.LoadBlob();
            int count = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                HashSet<GameObject> characters = new HashSet<GameObject>();
                foreach (SkinnedMeshRenderer skinned in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    characters.Add(PrefabUtility.GetOutermostPrefabInstanceRoot(skinned.gameObject));
                }
                foreach (GameObject character in characters)
                {
                    Renderer[] renderers = character.GetComponentsInChildren<Renderer>(true);
                    Bounds bounds = renderers[0].bounds;
                    foreach (Renderer renderer in renderers)
                    {
                        renderer.shadowCastingMode = ShadowCastingMode.Off;
                        bounds.Encapsulate(renderer.bounds);
                    }
                    GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    quad.name = "BlobShadow";
                    Object.DestroyImmediate(quad.GetComponent<Collider>());
                    quad.transform.SetParent(root.transform, true);
                    // 쿼드 앞면(-Z)이 위를 보게 눕힌다 · 바닥에 살짝 띄워 깊이 다툼을 피한다
                    quad.transform.SetPositionAndRotation(new Vector3(bounds.center.x, root.transform.position.y + BLOB_LIFT_METERS, bounds.center.z), Quaternion.Euler(90f, 0f, 0f));
                    float size = Mathf.Max(bounds.size.x, bounds.size.z) * BLOB_SIZE_RATIO;
                    quad.transform.localScale = new Vector3(size, size, 1f);
                    MeshRenderer quadRenderer = quad.GetComponent<MeshRenderer>();
                    quadRenderer.sharedMaterial = blob;
                    quadRenderer.shadowCastingMode = ShadowCastingMode.Off;
                    quadRenderer.receiveShadows = false;
                    ++count;
                }
            }
            return count;
        }

        /// <summary>씬에 전역 볼륨 하나를 두고 profilePath 의 프로필을 건다(후처리를 켠 카메라만 영향). URP 타입은 이름으로 찾는다.</summary>
        public static void AddGlobalVolume(Scene scene, string profilePath)
        {
            Object profile = AssetDatabase.LoadAssetAtPath<ScriptableObject>(profilePath);
            Debug.Assert(profile != null, "볼륨 프로필이 없다: " + profilePath);
            System.Type type = System.Type.GetType(VOLUME_ASSEMBLY_QUALIFIED_NAME);
            Debug.Assert(type != null, "Volume 타입이 없다");
            GameObject volumeObject = new GameObject("ToonVolume");
            SceneManager.MoveGameObjectToScene(volumeObject, scene);
            SerializedObject serialized = new SerializedObject(volumeObject.AddComponent(type));
            serialized.FindProperty("sharedProfile").objectReferenceValue = profile;
            serialized.FindProperty("m_IsGlobal").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static string GetQualityName(int quality)
        {
            return QualitySettings.names[quality];
        }

        private static void Render(Transform cameraTransform, string path, int width, int height, int rendererIndex, bool isPostProcessing, StringBuilder report)
        {
            Camera camera = cameraTransform.GetComponent<Camera>();
            SetCameraRendering(camera, rendererIndex, isPostProcessing);
            P03FrameRecorder.RenderToPng(camera, path, width, height);
            report.Append(path).Append(' ').Append(width).Append('x').Append(height).Append('\n');
        }

        /// <summary>
        /// URP 카메라 추가 데이터의 렌더러 인덱스 · 후처리 켬을 씬 인스턴스에만 바꾼다.
        /// 에디터 어셈블리가 URP 를 참조하지 않아(asmdef 변경은 이 시험 범위 밖) 직렬화 필드로 바꾼다.
        /// </summary>
        private static void SetCameraRendering(Camera camera, int rendererIndex, bool isPostProcessing)
        {
            Component data = camera.GetComponent(CAMERA_DATA_TYPE_NAME);
            if (data == null)
            {
                // 비교 씬 카메라는 추가 데이터 없이 저장돼 있다(URP 기본값 — 렌더러 기본 · 후처리 끔) — 바꿀 때만 씬 인스턴스에 붙인다
                if (rendererIndex < 0 && !isPostProcessing)
                {
                    return;
                }
                System.Type type = System.Type.GetType(CAMERA_DATA_ASSEMBLY_QUALIFIED_NAME);
                Debug.Assert(type != null, "URP 카메라 데이터 타입이 없다");
                data = camera.gameObject.AddComponent(type);
            }
            SerializedObject serialized = new SerializedObject(data);
            serialized.FindProperty("m_RendererIndex").intValue = rendererIndex;
            serialized.FindProperty("m_RenderPostProcessing").boolValue = isPostProcessing;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Mesh GetMesh(Renderer renderer)
        {
            SkinnedMeshRenderer skinnedRenderer = renderer as SkinnedMeshRenderer;
            if (skinnedRenderer != null)
            {
                return skinnedRenderer.sharedMesh;
            }
            MeshFilter filter = renderer.GetComponent<MeshFilter>();
            return filter != null ? filter.sharedMesh : null;
        }

        // ShaderUtil.GetVariantCount 는 Unity 6 에서 internal — 셰이더 인스펙터가 보여 주는 같은 값을 리플렉션으로 읽는다(에디터 시험 도구)
        private static ulong GetVariantCount(Shader shader, bool isUsedBySceneOnly)
        {
            MethodInfo method = typeof(ShaderUtil).GetMethod("GetVariantCount", BindingFlags.NonPublic | BindingFlags.Static);
            Debug.Assert(method != null, "ShaderUtil.GetVariantCount 가 없다");
            return method == null ? 0UL : System.Convert.ToUInt64(method.Invoke(null, new object[] { shader, isUsedBySceneOnly }));
        }
    }
}
