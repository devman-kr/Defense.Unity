using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Defense.Editor.P04
{
    /// <summary>
    /// P04 비교 씬(<see cref="P04SampleSceneBuilder"/> 산출)에 인간형 루트 둘을 더한다 — 꺼진 채 저장해 S1 캡처 · 녹화(루트 셋만 켜고 끈다)에 걸리지 않는다.
    /// `P04Humanoids` — 매끈 판 루트(`P04Smooth`)와 같은 자리에 해골 궁수 4(슬라임 고리 바깥) · 좀비 4(두 고리 사이) · 근접 카메라 둘.
    /// `P04Lineup5` — 매끈 판 다섯(슬라임 · 박쥐 · 등불 탑 · 해골 궁수 · 좀비) 한 줄.
    /// 씬을 통째 다시 만들 때는 <see cref="P04SampleSceneBuilder.Build"/> 뒤에 <see cref="Add"/>.
    /// </summary>
    public static class P04HumanoidSceneBuilder
    {
        public const string HUMANOIDS_ROOT_NAME = "P04Humanoids";
        public const string LINEUP5_ROOT_NAME = "P04Lineup5";
        public const string ARCHERS_NAME = "Archers";
        public const string ZOMBIES_NAME = "Zombies";
        public const string ARCHER_FACE_CAMERA_NAME = "ArcherFaceCamera";
        public const string ZOMBIE_SIDE_CAMERA_NAME = "ZombieSideCamera";
        public const string LINEUP5_CAMERA_NAME = "Lineup5Camera";
        public const string LINEUP5_ARCHER_NAME = "Archer";
        public const string LINEUP5_ZOMBIE_NAME = "Zombie";

        // 배치 — 슬라임은 반지름 3.4 · 4.6 에 22.5° + 45° 간격(P03 구도), 섬 반지름 6.5
        private const int ARCHER_COUNT = 4;
        private const float ARCHER_RADIUS_METERS = 5.6f;
        private const float ARCHER_START_DEGREES = 45f;
        private const int ZOMBIE_COUNT = 4;
        private const float ZOMBIE_RADIUS_METERS = 4.0f;
        private const float ZOMBIE_START_DEGREES = 0f;

        // 근접 — 첫 궁수 얼굴 · 활을 오른쪽 앞에서, 첫 좀비 무릎을 옆에서
        private const float ARCHER_FACE_DISTANCE_METERS = 1.7f;
        private const float ARCHER_FACE_SIDE_METERS = 0.7f;
        private const float ARCHER_FACE_HEIGHT_METERS = 1.5f;
        private const float ARCHER_FACE_TARGET_HEIGHT_METERS = 1.05f;
        private const float ZOMBIE_SIDE_DISTANCE_METERS = 3.2f;
        private const float ZOMBIE_SIDE_FRONT_METERS = 0.6f;
        private const float ZOMBIE_SIDE_HEIGHT_METERS = 1.0f;
        private const float ZOMBIE_SIDE_TARGET_HEIGHT_METERS = 0.8f;
        private const float CLOSE_CAMERA_FIELD_OF_VIEW = 35f;

        // 라인업 다섯 — 다른 루트의 카메라에 걸리지 않게 멀리(S1 라인업은 x 40)
        private static readonly Vector3 LINEUP5_ORIGIN = new Vector3(80f, 0f, 0f);
        private static readonly float[] LINEUP5_X_METERS = { -3.6f, -2.2f, -0.6f, 1.2f, 2.8f };
        private const float LINEUP5_BAT_HEIGHT_METERS = 1.2f;
        private static readonly Vector3 LINEUP5_GROUND_SCALE = new Vector3(10f, 0.25f, 4f);
        private static readonly Vector3 LINEUP5_CAMERA_OFFSET = new Vector3(0f, 3.4f, -11.5f);
        private static readonly Vector3 LINEUP5_TARGET_OFFSET = new Vector3(0f, 1.0f, 0f);
        private const float LINEUP5_CAMERA_FIELD_OF_VIEW = 34f;

        /// <summary>씬을 열어 인간형 루트 둘을 (있으면 지우고) 다시 만들어 꺼진 채 저장하고, 루트마다 오브젝트 수를 한 줄로 돌려준다.</summary>
        public static string Add()
        {
            Scene scene = EditorSceneManager.OpenScene(P04SampleSceneBuilder.SCENE_PATH, OpenSceneMode.Single);
            foreach (GameObject candidate in scene.GetRootGameObjects())
            {
                if (candidate.name == HUMANOIDS_ROOT_NAME || candidate.name == LINEUP5_ROOT_NAME)
                {
                    Object.DestroyImmediate(candidate);
                }
            }
            Material lit = P04ModelImport.LoadMaterial("Lit");

            GameObject humanoids = new GameObject(HUMANOIDS_ROOT_NAME);
            Transform archers = CreateRing(humanoids.transform, ARCHERS_NAME, P04ModelImport.ARCHER_MODEL_PATH, "Archer", ARCHER_COUNT, ARCHER_RADIUS_METERS, ARCHER_START_DEGREES);
            Transform zombies = CreateRing(humanoids.transform, ZOMBIES_NAME, P04ModelImport.ZOMBIE_MODEL_PATH, "Zombie", ZOMBIE_COUNT, ZOMBIE_RADIUS_METERS, ZOMBIE_START_DEGREES);
            CreateCloseCamera(humanoids.transform, ARCHER_FACE_CAMERA_NAME, archers.GetChild(0),
                ARCHER_FACE_DISTANCE_METERS, ARCHER_FACE_SIDE_METERS, ARCHER_FACE_HEIGHT_METERS, ARCHER_FACE_TARGET_HEIGHT_METERS);
            CreateCloseCamera(humanoids.transform, ZOMBIE_SIDE_CAMERA_NAME, zombies.GetChild(0),
                ZOMBIE_SIDE_FRONT_METERS, ZOMBIE_SIDE_DISTANCE_METERS, ZOMBIE_SIDE_HEIGHT_METERS, ZOMBIE_SIDE_TARGET_HEIGHT_METERS);
            GameObject lineup = BuildLineup5();
            foreach (GameObject root in new[] { humanoids, lineup })
            {
                P04SampleSceneBuilder.ApplyMaterial(root, lit);
                root.SetActive(false);
            }

            bool isSaved = EditorSceneManager.SaveScene(scene, P04SampleSceneBuilder.SCENE_PATH);
            Debug.Assert(isSaved, "씬 저장 실패: " + P04SampleSceneBuilder.SCENE_PATH);
            return P04SampleSceneBuilder.SCENE_PATH + " saved=" + isSaved
                + " humanoids=" + humanoids.GetComponentsInChildren<Transform>(true).Length
                + " lineup5=" + lineup.GetComponentsInChildren<Transform>(true).Length;
        }

        /// <summary>원점(탑)을 둘러싼 고리에 모델을 count 개 — 모두 탑을 본다(모델 앞은 +Z).</summary>
        private static Transform CreateRing(Transform parent, string groupName, string modelPath, string namePrefix, int count, float radius, float startDegrees)
        {
            Transform group = new GameObject(groupName).transform;
            group.SetParent(parent, false);
            for (int i = 0; i < count; ++i)
            {
                float angle = (startDegrees + i * 360f / count) * Mathf.Deg2Rad;
                Vector3 position = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * radius;
                Quaternion facing = Quaternion.LookRotation(-position.normalized, Vector3.up);
                P04SampleSceneBuilder.InstantiateModel(modelPath, namePrefix + (i + 1).ToString("00"), group, position, facing);
            }
            return group;
        }

        /// <summary>대상 앞(front) · 오른쪽(side, 대상 기준 +X) · 높이에서 대상의 targetHeight 를 보는 카메라.</summary>
        private static void CreateCloseCamera(Transform parent, string name, Transform subject, float front, float side, float height, float targetHeight)
        {
            // 월드 좌표로 정한다 — 대상은 묶음(Archers · Zombies) 아래, 카메라는 그 위 루트 아래라 local 값을 섞으면 묶음이 움직였을 때 어긋난다
            Vector3 target = subject.position + Vector3.up * targetHeight;
            Vector3 position = subject.position + subject.rotation * new Vector3(side, 0f, front) + Vector3.up * height;
            GameObject cameraObject = new GameObject(name);
            cameraObject.transform.SetParent(parent, false);
            cameraObject.transform.SetPositionAndRotation(position, Quaternion.LookRotation(target - position, Vector3.up));
            P04SampleSceneBuilder.AddCamera(cameraObject, CLOSE_CAMERA_FIELD_OF_VIEW);
        }

        private static GameObject BuildLineup5()
        {
            GameObject root = new GameObject(LINEUP5_ROOT_NAME);
            root.transform.position = LINEUP5_ORIGIN;
            P04SampleSceneBuilder.CreateIsland(root.transform, Vector3.zero, LINEUP5_GROUND_SCALE);
            string[] paths =
            {
                P04ModelImport.SLIME_MODEL_PATH, P04ModelImport.BAT_MODEL_PATH, P04ModelImport.TOWER_MODEL_PATH,
                P04ModelImport.ARCHER_MODEL_PATH, P04ModelImport.ZOMBIE_MODEL_PATH,
            };
            string[] names = { "Slime", "Bat", "Tower", LINEUP5_ARCHER_NAME, LINEUP5_ZOMBIE_NAME };
            // 모델 앞(눈)은 +Z — 카메라(-Z 쪽)를 보게 돌린다
            Quaternion facing = Quaternion.LookRotation(Vector3.back, Vector3.up);
            for (int i = 0; i < paths.Length; ++i)
            {
                float height = paths[i] == P04ModelImport.BAT_MODEL_PATH ? LINEUP5_BAT_HEIGHT_METERS : 0f;
                P04SampleSceneBuilder.InstantiateModel(paths[i], names[i], root.transform, new Vector3(LINEUP5_X_METERS[i], height, 0f), facing);
            }
            P04SampleSceneBuilder.CreateLights(root.transform, false);

            GameObject cameraObject = new GameObject(LINEUP5_CAMERA_NAME);
            cameraObject.transform.SetParent(root.transform, false);
            cameraObject.transform.localPosition = LINEUP5_CAMERA_OFFSET;
            cameraObject.transform.localRotation = Quaternion.LookRotation(LINEUP5_TARGET_OFFSET - LINEUP5_CAMERA_OFFSET, Vector3.up);
            P04SampleSceneBuilder.AddCamera(cameraObject, LINEUP5_CAMERA_FIELD_OF_VIEW);
            return root;
        }
    }
}
