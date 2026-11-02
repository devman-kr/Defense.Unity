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
    /// P04 비교 씬을 찍는다 — 셰이딩 셋(Lit · Unlit · 셀) × 판 둘(매끈 · 로우폴리) 세로 6장 + 라인업 1장 + 슬라임 얼굴 근접 1장.
    /// 그리고 슬라임 Hop 프레임 13 의 꼭대기 높이를 두 판에서 잰다(스킨 가중치가 서브디비전 뒤에도 남았는지).
    /// </summary>
    public static class P04Capture
    {
        private const int PORTRAIT_WIDTH = 1080;
        private const int PORTRAIT_HEIGHT = 1920;
        private const int LINEUP_WIDTH = 1920;
        private const int LINEUP_HEIGHT = 1080;
        private const int FACE_SIZE = 1080;
        private const string HOP_CLIP = "Hop";
        private const int HOP_TOP_FRAME = 13;   // Blender 프레임 번호(1 부터) — Hop 의 꼭대기
        private const int PALETTE_SIZE = 4;

        private static readonly string[] SHADINGS = { "Lit", "Unlit", "Cel" };

        /// <summary>씬을 열어 8장을 outDir 에 쓰고 씬을 디스크에서 다시 연다(바꾼 머티리얼 · 켜고 끈 루트를 버린다). 장마다 한 줄씩 돌려준다.</summary>
        public static string CaptureAll(string outDir)
        {
            Scene scene = EditorSceneManager.OpenScene(P04SampleSceneBuilder.SCENE_PATH, OpenSceneMode.Single);
            Transform smooth = P03FrameRecorder.GetRoot(scene, P04SampleSceneBuilder.SMOOTH_ROOT_NAME);
            Transform low = P03FrameRecorder.GetRoot(scene, P04SampleSceneBuilder.LOW_ROOT_NAME);
            Transform lineup = P03FrameRecorder.GetRoot(scene, P04SampleSceneBuilder.LINEUP_ROOT_NAME);
            Transform[] roots = { smooth, low, lineup };
            Directory.CreateDirectory(outDir);
            StringBuilder report = new StringBuilder();

            foreach (string shading in SHADINGS)
            {
                Material material = P04ModelImport.LoadMaterial(shading);
                Capture(roots, smooth, P03SampleSceneBuilder.CAMERA_NAME, material, Path.Combine(outDir, "P04_Smooth_" + shading + ".png"), PORTRAIT_WIDTH, PORTRAIT_HEIGHT, report);
                Capture(roots, low, P03SampleSceneBuilder.CAMERA_NAME, material, Path.Combine(outDir, "P04_Low_" + shading + ".png"), PORTRAIT_WIDTH, PORTRAIT_HEIGHT, report);
            }
            Material lit = P04ModelImport.LoadMaterial("Lit");
            Capture(roots, lineup, P04SampleSceneBuilder.LINEUP_CAMERA_NAME, lit, Path.Combine(outDir, "P04_Lineup_Lit.png"), LINEUP_WIDTH, LINEUP_HEIGHT, report);
            Capture(roots, smooth, P04SampleSceneBuilder.FACE_CAMERA_NAME, lit, Path.Combine(outDir, "P04_Close_SlimeFace_Smooth_Lit.png"), FACE_SIZE, FACE_SIZE, report);

            EditorSceneManager.OpenScene(P04SampleSceneBuilder.SCENE_PATH, OpenSceneMode.Single);
            return report.ToString();
        }

        /// <summary>
        /// 모델마다 Hop 프레임 1 · 13 의 스킨 메시 최고 · 최저 높이와 UV 가 팔레트 셀 중심에서 가장 멀리 벗어난 거리(텍스처 공간)를 한 줄씩 돌려준다.
        /// 모델을 지금 씬에 잠시 복제했다가 지운다 — 씬은 저장하지 않는다.
        /// </summary>
        public static string MeasureHop()
        {
            StringBuilder report = new StringBuilder();
            foreach (string path in new[] { P04ModelImport.SLIME_MODEL_PATH, P03SampleSceneBuilder.SLIME_MODEL_PATH })
            {
                GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Debug.Assert(model != null, "모델이 없다: " + path);
                AnimationClip hop = LoadClip(path, HOP_CLIP);
                GameObject instance = Object.Instantiate(model);
                SkinnedMeshRenderer skinned = instance.GetComponentInChildren<SkinnedMeshRenderer>();
                Mesh baked = new Mesh();
                report.Append(path);
                foreach (int frame in new[] { 1, HOP_TOP_FRAME })
                {
                    hop.SampleAnimation(instance, (frame - 1) / hop.frameRate);
                    skinned.BakeMesh(baked, true);
                    // BakeMesh(useScale) 의 정점은 렌더러 기준 공간에 크기까지 들어 있다 — 위치 · 회전만 더한다
                    Matrix4x4 toWorld = Matrix4x4.TRS(skinned.transform.position, skinned.transform.rotation, Vector3.one);
                    float top = float.MinValue;
                    float bottom = float.MaxValue;
                    foreach (Vector3 vertex in baked.vertices)
                    {
                        float y = toWorld.MultiplyPoint3x4(vertex).y;
                        top = Mathf.Max(top, y);
                        bottom = Mathf.Min(bottom, y);
                    }
                    report.Append(" f").Append(frame).Append(" max_y=").Append(top.ToString("0.000")).Append(" min_y=").Append(bottom.ToString("0.000"));
                }
                report.Append(" uv_off_center=").Append(GetUvOffCenter(skinned.sharedMesh).ToString("0.000000")).Append('\n');
                Object.DestroyImmediate(baked);
                Object.DestroyImmediate(instance);
            }
            return report.ToString();
        }

        private static void Capture(Transform[] roots, Transform target, string cameraName, Material material, string path, int width, int height, StringBuilder report)
        {
            foreach (Transform root in roots)
            {
                root.gameObject.SetActive(root == target);
            }
            P04SampleSceneBuilder.ApplyMaterial(target.gameObject, material);
            Camera camera = target.Find(cameraName).GetComponent<Camera>();
            P03FrameRecorder.RenderToPng(camera, path, width, height);
            report.Append(path).Append(' ').Append(width).Append('x').Append(height).Append(' ').Append(material.name).Append('\n');
        }

        private static AnimationClip LoadClip(string modelPath, string clipName)
        {
            foreach (Object asset in AssetDatabase.LoadAllAssetRepresentationsAtPath(modelPath))
            {
                AnimationClip clip = asset as AnimationClip;
                if (clip != null && clip.name == clipName)
                {
                    return clip;
                }
            }
            Debug.Assert(false, "클립이 없다: " + modelPath + " " + clipName);
            return null;
        }

        private static float GetUvOffCenter(Mesh mesh)
        {
            float worst = 0f;
            foreach (Vector2 uv in mesh.uv)
            {
                float centerU = (Mathf.Floor(uv.x * PALETTE_SIZE) + 0.5f) / PALETTE_SIZE;
                float centerV = (Mathf.Floor(uv.y * PALETTE_SIZE) + 0.5f) / PALETTE_SIZE;
                worst = Mathf.Max(worst, Mathf.Abs(uv.x - centerU), Mathf.Abs(uv.y - centerV));
            }
            return worst;
        }
    }
}
