using Defense.Editor.P03;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Defense.Editor.P04
{
    /// <summary>
    /// P04 비교 씬을 처음부터 다시 만든다. 루트 셋 중 하나만 켜서 찍는다 —
    /// 매끈 판(`P04Smooth` — P03 샘플 씬과 같은 구도: 섬 · 탑 · 슬라임 8 · 박쥐 4 · 세로 카메라 · 조명 + 슬라임 얼굴 근접 카메라) ·
    /// 로우폴리 판(`P04Low` — 같은 구도에 P03 모델) · 라인업(`P04Lineup` — 로우폴리 3 왼쪽, 매끈 3 오른쪽).
    /// 머티리얼은 저장 때 모두 `PaletteLit`, 셰이딩은 찍을 때 <see cref="ApplyMaterial"/> 로 바꾼다.
    /// </summary>
    public static class P04SampleSceneBuilder
    {
        public const string SCENE_PATH = "Assets/00_Scenes/P04_Smooth_Sample.unity";
        public const string SMOOTH_ROOT_NAME = "P04Smooth";
        public const string LOW_ROOT_NAME = "P04Low";
        public const string LINEUP_ROOT_NAME = "P04Lineup";
        public const string FACE_CAMERA_NAME = "FaceCamera";
        public const string LINEUP_CAMERA_NAME = "LineupCamera";

        private const string LOW_SLIME_MODEL_PATH = P03SampleSceneBuilder.SLIME_MODEL_PATH;
        private const string LOW_BAT_MODEL_PATH = P03SampleSceneBuilder.BAT_MODEL_PATH;
        private const string LOW_TOWER_MODEL_PATH = P03ModelImport.FOLDER + "/tower_bst.fbx";
        private const string ISLAND_MESH_PATH = P03ModelImport.FOLDER + "/island.asset";

        // 구도 — P03SampleSceneBuilder 와 같은 값
        private const float ISLAND_DIAMETER_METERS = 13f;
        private const float ISLAND_THICKNESS_METERS = 0.5f;
        private const int SLIME_COUNT = 8;
        private const float SLIME_INNER_RADIUS_METERS = 3.4f;
        private const float SLIME_OUTER_RADIUS_METERS = 4.6f;
        private const int BAT_COUNT = 4;
        private const float BAT_RADIUS_METERS = 2.4f;
        private const float BAT_LOW_HEIGHT_METERS = 1.35f;
        private const float BAT_HIGH_HEIGHT_METERS = 1.65f;
        private const float SUN_PITCH_DEGREES = 50f;
        private const float SUN_YAW_DEGREES = -30f;
        private const float SUN_INTENSITY = 1.1f;
        private const float LAMP_HEIGHT_METERS = 2.0f;
        private const float LAMP_RANGE_METERS = 7f;
        private const float LAMP_INTENSITY = 3f;
        private const float CAMERA_FIELD_OF_VIEW = 60f;
        private const float CAMERA_PITCH_DEGREES = 58f;
        private const float CAMERA_DISTANCE_METERS = 19f;
        private const float CAMERA_TARGET_HEIGHT_METERS = 0.5f;

        // 근접 — 첫 슬라임 얼굴을 탑 쪽(앞)에서
        private const float FACE_CAMERA_DISTANCE_METERS = 1.5f;
        private const float FACE_CAMERA_HEIGHT_METERS = 0.95f;
        private const float FACE_TARGET_HEIGHT_METERS = 0.45f;
        private const float FACE_CAMERA_FIELD_OF_VIEW = 35f;

        // 라인업 — 다른 루트의 카메라에 걸리지 않게 멀리
        private static readonly Vector3 LINEUP_ORIGIN = new Vector3(40f, 0f, 0f);
        private static readonly float[] LINEUP_X_METERS = { -4.0f, -2.6f, -1.0f, 1.0f, 2.4f, 4.0f };
        private const float LINEUP_BAT_HEIGHT_METERS = 1.2f;
        private static readonly Vector3 LINEUP_GROUND_SCALE = new Vector3(13f, 0.25f, 5f);
        private static readonly Vector3 LINEUP_CAMERA_OFFSET = new Vector3(0f, 3.4f, -11.5f);
        private static readonly Vector3 LINEUP_TARGET_OFFSET = new Vector3(0f, 1.0f, 0f);
        private const float LINEUP_CAMERA_FIELD_OF_VIEW = 38f;

        // 팔레트 셀 11(등불)과 같은 sRGB 값 — Blender 스크립트 PAL 표
        private static readonly Color LAMP_COLOR = new Color(1.00f, 0.86f, 0.40f);
        private static readonly Color SKY_COLOR = new Color(0.62f, 0.80f, 0.93f);
        private static readonly Color AMBIENT_COLOR = new Color(0.52f, 0.56f, 0.66f);

        /// <summary>씬을 새로 만들어 저장하고, 루트마다 오브젝트 수를 한 줄로 돌려준다.</summary>
        public static string Build()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = AMBIENT_COLOR;
            Material lit = P04ModelImport.LoadMaterial("Lit");

            GameObject smooth = BuildSet(SMOOTH_ROOT_NAME, P04ModelImport.SLIME_MODEL_PATH, P04ModelImport.BAT_MODEL_PATH, P04ModelImport.TOWER_MODEL_PATH);
            CreateFaceCamera(smooth.transform);
            GameObject low = BuildSet(LOW_ROOT_NAME, LOW_SLIME_MODEL_PATH, LOW_BAT_MODEL_PATH, LOW_TOWER_MODEL_PATH);
            GameObject lineup = BuildLineup();
            foreach (GameObject root in new[] { smooth, low, lineup })
            {
                ApplyMaterial(root, lit);
            }
            low.SetActive(false);
            lineup.SetActive(false);

            bool isSaved = EditorSceneManager.SaveScene(scene, SCENE_PATH);
            Debug.Assert(isSaved, "씬 저장 실패: " + SCENE_PATH);
            return SCENE_PATH + " saved=" + isSaved
                + " smooth=" + smooth.GetComponentsInChildren<Transform>(true).Length
                + " low=" + low.GetComponentsInChildren<Transform>(true).Length
                + " lineup=" + lineup.GetComponentsInChildren<Transform>(true).Length;
        }

        /// <summary>루트 아래 모든 렌더러(섬 포함)의 머티리얼을 하나로 바꾼다 — 씬 인스턴스에만, 모델 에셋은 그대로.</summary>
        public static void ApplyMaterial(GameObject root, Material material)
        {
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; ++i)
                {
                    materials[i] = material;
                }
                renderer.sharedMaterials = materials;
            }
        }

        private static GameObject BuildSet(string rootName, string slimePath, string batPath, string towerPath)
        {
            GameObject root = new GameObject(rootName);
            CreateIsland(root.transform, Vector3.zero, new Vector3(ISLAND_DIAMETER_METERS, ISLAND_THICKNESS_METERS * 0.5f, ISLAND_DIAMETER_METERS));
            InstantiateModel(towerPath, "Tower", root.transform, Vector3.zero, Quaternion.identity);
            CreateSlimes(root.transform, slimePath);
            CreateBats(root.transform, batPath);
            CreateLights(root.transform, true);
            CreateCamera(root.transform);
            return root;
        }

        private static GameObject BuildLineup()
        {
            GameObject root = new GameObject(LINEUP_ROOT_NAME);
            root.transform.position = LINEUP_ORIGIN;
            CreateIsland(root.transform, Vector3.zero, LINEUP_GROUND_SCALE);
            string[] paths = { LOW_SLIME_MODEL_PATH, LOW_BAT_MODEL_PATH, LOW_TOWER_MODEL_PATH, P04ModelImport.SLIME_MODEL_PATH, P04ModelImport.BAT_MODEL_PATH, P04ModelImport.TOWER_MODEL_PATH };
            string[] names = { "LowSlime", "LowBat", "LowTower", "SmoothSlime", "SmoothBat", "SmoothTower" };
            // 모델 앞(눈)은 +Z — 카메라(-Z 쪽)를 보게 돌린다
            Quaternion facing = Quaternion.LookRotation(Vector3.back, Vector3.up);
            for (int i = 0; i < paths.Length; ++i)
            {
                float height = paths[i] == LOW_BAT_MODEL_PATH || paths[i] == P04ModelImport.BAT_MODEL_PATH ? LINEUP_BAT_HEIGHT_METERS : 0f;
                InstantiateModel(paths[i], names[i], root.transform, new Vector3(LINEUP_X_METERS[i], height, 0f), facing);
            }
            CreateLights(root.transform, false);

            GameObject cameraObject = new GameObject(LINEUP_CAMERA_NAME);
            cameraObject.transform.SetParent(root.transform, false);
            cameraObject.transform.localPosition = LINEUP_CAMERA_OFFSET;
            cameraObject.transform.localRotation = Quaternion.LookRotation(LINEUP_TARGET_OFFSET - LINEUP_CAMERA_OFFSET, Vector3.up);
            AddCamera(cameraObject, LINEUP_CAMERA_FIELD_OF_VIEW);
            return root;
        }

        internal static void CreateIsland(Transform parent, Vector3 topCenter, Vector3 scale)
        {
            GameObject island = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            island.name = "Island";
            island.transform.SetParent(parent, false);
            // 실린더 프리미티브는 높이 2 · 지름 1 — 윗면이 topCenter 가 되게 내린다
            island.transform.localScale = scale;
            island.transform.localPosition = topCenter - new Vector3(0f, scale.y, 0f);
            Mesh grassCell = AssetDatabase.LoadAssetAtPath<Mesh>(ISLAND_MESH_PATH);
            Debug.Assert(grassCell != null, "섬 메시가 없다(P03 산출): " + ISLAND_MESH_PATH);
            island.GetComponent<MeshFilter>().sharedMesh = grassCell;
        }

        private static void CreateSlimes(Transform parent, string modelPath)
        {
            Transform group = new GameObject(P03SampleSceneBuilder.SLIMES_NAME).transform;
            group.SetParent(parent, false);
            for (int i = 0; i < SLIME_COUNT; ++i)
            {
                InstantiateModel(modelPath, "Slime" + (i + 1).ToString("00"), group, GetSlimePosition(i), GetSlimeFacing(i));
            }
        }

        private static Vector3 GetSlimePosition(int index)
        {
            float angle = (index + 0.5f) * Mathf.PI * 2f / SLIME_COUNT;
            float radius = index % 2 == 0 ? SLIME_INNER_RADIUS_METERS : SLIME_OUTER_RADIUS_METERS;
            return new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius);
        }

        // 모델 앞(눈)은 +Z — 탑(원점)을 보게 돌린다
        private static Quaternion GetSlimeFacing(int index)
        {
            return Quaternion.LookRotation(-GetSlimePosition(index).normalized, Vector3.up);
        }

        private static void CreateBats(Transform parent, string modelPath)
        {
            Transform group = new GameObject(P03SampleSceneBuilder.BATS_NAME).transform;
            group.SetParent(parent, false);
            for (int i = 0; i < BAT_COUNT; ++i)
            {
                float angle = (i + 0.25f) * Mathf.PI * 2f / BAT_COUNT;
                float height = i % 2 == 0 ? BAT_LOW_HEIGHT_METERS : BAT_HIGH_HEIGHT_METERS;
                Vector3 outward = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                Vector3 position = outward * BAT_RADIUS_METERS + Vector3.up * height;
                // 위에서 볼 때 시계 방향 접선을 본다 — 녹화에서는 자리에 머물고 날갯짓만 한다
                Quaternion facing = Quaternion.LookRotation(Vector3.Cross(Vector3.up, outward), Vector3.up);
                InstantiateModel(modelPath, "Bat" + (i + 1).ToString("00"), group, position, facing);
            }
        }

        internal static void CreateLights(Transform parent, bool hasLamp)
        {
            GameObject sun = new GameObject("Sun");
            sun.transform.SetParent(parent, false);
            sun.transform.localRotation = Quaternion.Euler(SUN_PITCH_DEGREES, SUN_YAW_DEGREES, 0f);
            Light sunLight = sun.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.intensity = SUN_INTENSITY;
            sunLight.shadows = LightShadows.Soft;
            if (!hasLamp)
            {
                return;
            }

            GameObject lamp = new GameObject("LampLight");
            lamp.transform.SetParent(parent, false);
            lamp.transform.localPosition = new Vector3(0f, LAMP_HEIGHT_METERS, 0f);
            Light lampLight = lamp.AddComponent<Light>();
            lampLight.type = LightType.Point;
            lampLight.color = LAMP_COLOR;
            lampLight.range = LAMP_RANGE_METERS;
            lampLight.intensity = LAMP_INTENSITY;
        }

        private static void CreateCamera(Transform parent)
        {
            GameObject cameraObject = new GameObject(P03SampleSceneBuilder.CAMERA_NAME);
            cameraObject.transform.SetParent(parent, false);
            Vector3 target = new Vector3(0f, CAMERA_TARGET_HEIGHT_METERS, 0f);
            Quaternion pitch = Quaternion.Euler(CAMERA_PITCH_DEGREES, 0f, 0f);
            cameraObject.transform.localPosition = target - pitch * Vector3.forward * CAMERA_DISTANCE_METERS;
            cameraObject.transform.localRotation = pitch;
            AddCamera(cameraObject, CAMERA_FIELD_OF_VIEW);
        }

        private static void CreateFaceCamera(Transform parent)
        {
            Vector3 slime = GetSlimePosition(0);
            Vector3 forward = GetSlimeFacing(0) * Vector3.forward;
            Vector3 target = slime + Vector3.up * FACE_TARGET_HEIGHT_METERS;
            Vector3 position = slime + forward * FACE_CAMERA_DISTANCE_METERS + Vector3.up * FACE_CAMERA_HEIGHT_METERS;
            GameObject cameraObject = new GameObject(FACE_CAMERA_NAME);
            cameraObject.transform.SetParent(parent, false);
            cameraObject.transform.localPosition = position;
            cameraObject.transform.localRotation = Quaternion.LookRotation(target - position, Vector3.up);
            AddCamera(cameraObject, FACE_CAMERA_FIELD_OF_VIEW);
        }

        internal static void AddCamera(GameObject cameraObject, float fieldOfView)
        {
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = false;
            camera.fieldOfView = fieldOfView;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = SKY_COLOR;
        }

        internal static GameObject InstantiateModel(string path, string name, Transform parent, Vector3 position, Quaternion rotation)
        {
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Debug.Assert(model != null, "모델이 없다: " + path);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(model, parent);
            instance.name = name;
            instance.transform.localPosition = position;
            instance.transform.localRotation = rotation;
            return instance;
        }
    }
}
