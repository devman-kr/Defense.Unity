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

        private static readonly int[] BOTH_QUALITIES = { MOBILE_QUALITY, PC_QUALITY };

        /// <summary>
        /// label 이름으로 두 품질 × 4장을 outDir 에 쓴다 — `{label}_{Mobile|PC}_{Portrait|Lineup5|CloseArcher|CloseSlime}.png`.
        /// configure 가 있으면 씬을 연 뒤 material 의 사본을 만들어 그것에 적용하고 그 사본으로 찍는다(에셋은 그대로).
        /// rendererIndex 가 0 이상이면 그 장면의 카메라를 그 렌더러로 찍는다(시험 렌더러 — 품질의 RP 에셋 목록에 있어야 한다).
        /// </summary>
        public static string CaptureSet(string outDir, string label, Material material, System.Action<Material> configure = null, int rendererIndex = -1, bool isPostProcessing = false)
        {
            return CaptureSet(outDir, label, material, configure, BOTH_QUALITIES, rendererIndex, isPostProcessing);
        }

        public static string CaptureSet(string outDir, string label, Material material, System.Action<Material> configure, int[] qualities, int rendererIndex, bool isPostProcessing)
        {
            Debug.Assert(material != null, "머티리얼이 없다: " + label);
            int originalQuality = QualitySettings.GetQualityLevel();
            Directory.CreateDirectory(outDir);
            StringBuilder report = new StringBuilder();
            foreach (int quality in qualities)
            {
                QualitySettings.SetQualityLevel(quality, true);
                string prefix = Path.Combine(outDir, label + "_" + GetQualityName(quality) + "_");
                Scene scene = EditorSceneManager.OpenScene(P04SampleSceneBuilder.SCENE_PATH, OpenSceneMode.Single);
                Transform smooth = P03FrameRecorder.GetRoot(scene, P04SampleSceneBuilder.SMOOTH_ROOT_NAME);
                Transform humanoids = P03FrameRecorder.GetRoot(scene, P04HumanoidSceneBuilder.HUMANOIDS_ROOT_NAME);
                Transform lineup5 = P03FrameRecorder.GetRoot(scene, P04HumanoidSceneBuilder.LINEUP5_ROOT_NAME);
                // 사본은 씬을 연 뒤에 만든다 — 연기 전에 만든 사본은 Single 열기가 셰이더를 언로드해 오류 셰이더(자홍색)로 그려진다(이 세션 실측)
                Material used = material;
                if (configure != null)
                {
                    used = new Material(material);
                    used.name = label;
                    configure(used);
                }
                foreach (Transform root in new[] { smooth, humanoids, lineup5 })
                {
                    P04SampleSceneBuilder.ApplyMaterial(root.gameObject, used);
                }
                P04HumanoidCapture.ForceSkinningPerRender(smooth, humanoids, lineup5);
                P04HumanoidCapture.Clips clips = new P04HumanoidCapture.Clips();

                P04HumanoidCapture.ShowOnly(scene, smooth, humanoids);
                P04HumanoidCapture.PoseEnemies(smooth, clips, P04HumanoidCapture.CAPTURE_TIME_SECONDS);
                P04HumanoidCapture.PoseHumanoids(humanoids, clips,
                    clips.Archer[P04HumanoidCapture.SHOOT_CLIP], P04HumanoidCapture.ToTime(P04HumanoidCapture.SHOOT_DRAW_FRAME),
                    clips.Zombie[P04HumanoidCapture.WALK_CLIP], P04HumanoidCapture.CAPTURE_TIME_SECONDS);
                Render(smooth.Find(P03SampleSceneBuilder.CAMERA_NAME), prefix + "Portrait.png", PORTRAIT_WIDTH, PORTRAIT_HEIGHT, rendererIndex, isPostProcessing, report);
                Render(humanoids.Find(P04HumanoidSceneBuilder.ARCHER_FACE_CAMERA_NAME), prefix + "CloseArcher.png", CLOSE_SIZE, CLOSE_SIZE, rendererIndex, isPostProcessing, report);
                Render(smooth.Find(P04SampleSceneBuilder.FACE_CAMERA_NAME), prefix + "CloseSlime.png", CLOSE_SIZE, CLOSE_SIZE, rendererIndex, isPostProcessing, report);

                P04HumanoidCapture.ShowOnly(scene, lineup5);
                clips.Archer[P04HumanoidCapture.IDLE_CLIP].SampleAnimation(lineup5.Find(P04HumanoidSceneBuilder.LINEUP5_ARCHER_NAME).gameObject, 0f);
                clips.Zombie[P04HumanoidCapture.IDLE_CLIP].SampleAnimation(lineup5.Find(P04HumanoidSceneBuilder.LINEUP5_ZOMBIE_NAME).gameObject, 0f);
                Render(lineup5.Find(P04HumanoidSceneBuilder.LINEUP5_CAMERA_NAME), prefix + "Lineup5.png", LINEUP_WIDTH, LINEUP_HEIGHT, rendererIndex, isPostProcessing, report);
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
