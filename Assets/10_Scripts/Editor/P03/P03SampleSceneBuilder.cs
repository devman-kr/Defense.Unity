using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Defense.Editor.P03
{
    /// <summary>
    /// P03 샘플 씬을 처음부터 다시 만든다 — 잔디 원판 섬 · 중앙 등불 탑 · 탑을 보는 슬라임 8 · 탑 둘레 공중의 박쥐 4 ·
    /// 세로 9:16 탑다운 사선 카메라 · 방향광 + 등불 점광. 머티리얼은 모델과 섬 모두 팔레트 하나(`palette.mat`).
    /// 모델은 bake_space_transform 변형(`_bst`)을 쓴다 — 기본 변형의 탑은 축 보정 X 270° 가 루트에 있어 루트 회전을 정하면 눕는다.
    /// </summary>
    public static class P03SampleSceneBuilder
    {
        public const string SCENE_PATH = "Assets/00_Scenes/P03_CLI_Sample.unity";
        public const string ROOT_NAME = "P03Sample";
        public const string SLIMES_NAME = "Slimes";
        public const string BATS_NAME = "Bats";
        public const string CAMERA_NAME = "PortraitCamera";
        public const string SLIME_MODEL_PATH = P03ModelImport.FOLDER + "/slime_bst.fbx";
        public const string BAT_MODEL_PATH = P03ModelImport.FOLDER + "/bat_bst.fbx";

        private const string TOWER_MODEL_PATH = P03ModelImport.FOLDER + "/tower_bst.fbx";
        private const string ISLAND_MESH_PATH = P03ModelImport.FOLDER + "/island.asset";

        private const int PALETTE_SIZE = 4;
        private const int GRASS_PALETTE_CELL = 12;
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

        // 팔레트 셀 11(등불)과 같은 sRGB 값 — Blender 스크립트 PAL 표
        private static readonly Color LAMP_COLOR = new Color(1.00f, 0.86f, 0.40f);
        private static readonly Color SKY_COLOR = new Color(0.62f, 0.80f, 0.93f);
        private static readonly Color AMBIENT_COLOR = new Color(0.52f, 0.56f, 0.66f);

        /// <summary>씬을 새로 만들어 저장하고, 놓은 오브젝트 수를 한 줄로 돌려준다.</summary>
        public static string Build()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = AMBIENT_COLOR;

            GameObject root = new GameObject(ROOT_NAME);
            CreateIsland(root.transform);
            InstantiateModel(TOWER_MODEL_PATH, "Tower", root.transform, Vector3.zero, Quaternion.identity);
            CreateSlimes(root.transform);
            CreateBats(root.transform);
            CreateLights(root.transform);
            CreateCamera(root.transform);

            bool isSaved = EditorSceneManager.SaveScene(scene, SCENE_PATH);
            Debug.Assert(isSaved, "씬 저장 실패: " + SCENE_PATH);
            return SCENE_PATH + " saved=" + isSaved + " objects=" + root.GetComponentsInChildren<Transform>(true).Length;
        }

        private static void CreateIsland(Transform parent)
        {
            GameObject island = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            island.name = "Island";
            island.transform.SetParent(parent, false);
            // 실린더 프리미티브는 높이 2 · 지름 1 — 윗면이 y = 0 이 되게 내린다
            island.transform.localScale = new Vector3(ISLAND_DIAMETER_METERS, ISLAND_THICKNESS_METERS * 0.5f, ISLAND_DIAMETER_METERS);
            island.transform.localPosition = new Vector3(0f, -ISLAND_THICKNESS_METERS * 0.5f, 0f);
            MeshFilter filter = island.GetComponent<MeshFilter>();
            filter.sharedMesh = SaveGrassCellMesh(filter.sharedMesh);
            Material palette = AssetDatabase.LoadAssetAtPath<Material>(P03ModelImport.PALETTE_MATERIAL_PATH);
            Debug.Assert(palette != null, "팔레트 머티리얼이 없다 — P03ModelImport.Apply 를 먼저: " + P03ModelImport.PALETTE_MATERIAL_PATH);
            island.GetComponent<MeshRenderer>().sharedMaterial = palette;
        }

        // 프리미티브 UV 는 팔레트 전체(0~1)를 덮는다 — 모든 UV 를 잔디 셀 중심으로 모은 사본을 에셋으로 둔다(이미 있으면 GUID 를 지키며 덮어쓴다)
        private static Mesh SaveGrassCellMesh(Mesh primitive)
        {
            Mesh mesh = Object.Instantiate(primitive);
            mesh.name = "island";
            Vector2 cellCenter = new Vector2(
                (GRASS_PALETTE_CELL % PALETTE_SIZE + 0.5f) / PALETTE_SIZE,
                (GRASS_PALETTE_CELL / PALETTE_SIZE + 0.5f) / PALETTE_SIZE);
            Vector2[] uv = new Vector2[mesh.vertexCount];
            for (int i = 0; i < uv.Length; ++i)
            {
                uv[i] = cellCenter;
            }
            mesh.uv = uv;

            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(ISLAND_MESH_PATH);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, ISLAND_MESH_PATH);
                return mesh;
            }
            EditorUtility.CopySerialized(mesh, existing);
            Object.DestroyImmediate(mesh);
            AssetDatabase.SaveAssets();
            return existing;
        }

        private static void CreateSlimes(Transform parent)
        {
            Transform group = new GameObject(SLIMES_NAME).transform;
            group.SetParent(parent, false);
            for (int i = 0; i < SLIME_COUNT; ++i)
            {
                float angle = (i + 0.5f) * Mathf.PI * 2f / SLIME_COUNT;
                float radius = i % 2 == 0 ? SLIME_INNER_RADIUS_METERS : SLIME_OUTER_RADIUS_METERS;
                Vector3 position = new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius);
                // 모델 앞(눈)은 +Z — 탑(원점)을 보게 돌린다
                Quaternion facing = Quaternion.LookRotation(-position.normalized, Vector3.up);
                InstantiateModel(SLIME_MODEL_PATH, "Slime" + (i + 1).ToString("00"), group, position, facing);
            }
        }

        private static void CreateBats(Transform parent)
        {
            Transform group = new GameObject(BATS_NAME).transform;
            group.SetParent(parent, false);
            for (int i = 0; i < BAT_COUNT; ++i)
            {
                float angle = (i + 0.25f) * Mathf.PI * 2f / BAT_COUNT;
                float height = i % 2 == 0 ? BAT_LOW_HEIGHT_METERS : BAT_HIGH_HEIGHT_METERS;
                Vector3 outward = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                Vector3 position = outward * BAT_RADIUS_METERS + Vector3.up * height;
                // 위에서 볼 때 시계 방향 접선을 본다 — 녹화에서는 자리에 머물고 날갯짓만 한다
                Quaternion facing = Quaternion.LookRotation(Vector3.Cross(Vector3.up, outward), Vector3.up);
                InstantiateModel(BAT_MODEL_PATH, "Bat" + (i + 1).ToString("00"), group, position, facing);
            }
        }

        private static void CreateLights(Transform parent)
        {
            GameObject sun = new GameObject("Sun");
            sun.transform.SetParent(parent, false);
            sun.transform.localRotation = Quaternion.Euler(SUN_PITCH_DEGREES, SUN_YAW_DEGREES, 0f);
            Light sunLight = sun.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.intensity = SUN_INTENSITY;
            sunLight.shadows = LightShadows.Soft;

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
            GameObject cameraObject = new GameObject(CAMERA_NAME);
            cameraObject.transform.SetParent(parent, false);
            Vector3 target = new Vector3(0f, CAMERA_TARGET_HEIGHT_METERS, 0f);
            Quaternion pitch = Quaternion.Euler(CAMERA_PITCH_DEGREES, 0f, 0f);
            cameraObject.transform.localPosition = target - pitch * Vector3.forward * CAMERA_DISTANCE_METERS;
            cameraObject.transform.localRotation = pitch;
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = false;
            camera.fieldOfView = CAMERA_FIELD_OF_VIEW;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = SKY_COLOR;
        }

        private static GameObject InstantiateModel(string path, string name, Transform parent, Vector3 position, Quaternion rotation)
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
