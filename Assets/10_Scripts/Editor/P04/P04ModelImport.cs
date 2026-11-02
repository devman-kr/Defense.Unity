using System.Text;
using Defense.Editor.P03;
using UnityEditor;
using UnityEngine;

namespace Defense.Editor.P04
{
    /// <summary>
    /// P04 매끈 판 모델(Blender `make_enemies_smooth.py` 의 FBX · 팔레트)의 가져오기 설정과 팔레트 머티리얼 셋(Lit · Unlit · 셀)을 코드로 맞춘다 —
    /// 팔레트 텍스처 Point 필터 · 노멀은 FBX 의 것 · 뼈대 접두를 벗긴 클립 이름 · 반복 클립 · 셰이딩마다 머티리얼 하나.
    /// </summary>
    public static class P04ModelImport
    {
        public const string FOLDER = "Assets/30_Assets/Models/P04_Smooth";
        public const string MATERIAL_FOLDER = "Assets/30_Assets/Materials";
        public const string LIT_MATERIAL_PATH = MATERIAL_FOLDER + "/PaletteLit.mat";
        public const string UNLIT_MATERIAL_PATH = MATERIAL_FOLDER + "/PaletteUnlit.mat";
        public const string CEL_MATERIAL_PATH = MATERIAL_FOLDER + "/PaletteCel.mat";
        public const string SLIME_MODEL_PATH = FOLDER + "/Slime.fbx";
        public const string BAT_MODEL_PATH = FOLDER + "/Bat.fbx";
        public const string TOWER_MODEL_PATH = FOLDER + "/Tower.fbx";

        private const string PALETTE_TEXTURE_PATH = FOLDER + "/Palette.png";
        private const string LIT_SHADER_PATH = "Packages/com.unity.render-pipelines.universal/Shaders/Lit.shader";
        private const string UNLIT_SHADER_PATH = "Packages/com.unity.render-pipelines.universal/Shaders/Unlit.shader";
        private const string CEL_SHADER_PATH = "Assets/30_Assets/Shaders/PaletteCel.shader";
        private const string FBX_MATERIAL_NAME = "Palette";
        private const float PALETTE_SMOOTHNESS = 0.1f;

        private static readonly string[] MODEL_PATHS = { SLIME_MODEL_PATH, BAT_MODEL_PATH, TOWER_MODEL_PATH };
        private static readonly string[] LOOP_CLIP_NAMES = { "Idle", "Hop", "Flap" };

        /// <summary>팔레트 텍스처 · 머티리얼 셋 · 모델 셋을 설정하고, 모델마다 클립 이름과 메시 수(삼각형 · 정점 · 노멀 · 뼈)를 한 줄씩 돌려준다.</summary>
        public static string Apply()
        {
            StringBuilder report = new StringBuilder();
            ApplyPaletteTexture();
            Texture2D palette = AssetDatabase.LoadAssetAtPath<Texture2D>(PALETTE_TEXTURE_PATH);
            Debug.Assert(palette != null, "Palette.png 가 없다: " + PALETTE_TEXTURE_PATH);
            if (!AssetDatabase.IsValidFolder(MATERIAL_FOLDER))
            {
                AssetDatabase.CreateFolder("Assets/30_Assets", "Materials");
            }
            Material lit = GetOrCreateMaterial(LIT_MATERIAL_PATH, LIT_SHADER_PATH, palette);
            lit.SetFloat("_Smoothness", PALETTE_SMOOTHNESS);
            GetOrCreateMaterial(UNLIT_MATERIAL_PATH, UNLIT_SHADER_PATH, palette);
            GetOrCreateMaterial(CEL_MATERIAL_PATH, CEL_SHADER_PATH, palette);
            AssetDatabase.SaveAssets();

            foreach (string path in MODEL_PATHS)
            {
                ApplyModel(path, lit, report);
            }
            foreach (string path in MODEL_PATHS)
            {
                AppendMeshStats(path, report);
            }
            return report.ToString();
        }

        /// <summary>셰이딩 이름(Lit · Unlit · Cel)의 팔레트 머티리얼.</summary>
        public static Material LoadMaterial(string shading)
        {
            string path = shading == "Unlit" ? UNLIT_MATERIAL_PATH : shading == "Cel" ? CEL_MATERIAL_PATH : LIT_MATERIAL_PATH;
            Debug.Assert(shading == "Lit" || shading == "Unlit" || shading == "Cel", "모르는 셰이딩: " + shading);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Debug.Assert(material != null, "머티리얼이 없다 — Apply 를 먼저: " + path);
            return material;
        }

        private static void ApplyPaletteTexture()
        {
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(PALETTE_TEXTURE_PATH);
            Debug.Assert(importer != null, "Palette.png 가 없다: " + PALETTE_TEXTURE_PATH);
            // 4x4 셀 팔레트 — 보간 · 밉맵 · 블록 압축은 이웃 셀 색을 섞는다
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
        }

        private static Material GetOrCreateMaterial(string path, string shaderPath, Texture2D palette)
        {
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
            Debug.Assert(shader != null, "셰이더가 없다: " + shaderPath);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            material.SetTexture("_BaseMap", palette);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ApplyModel(string path, Material palette, StringBuilder report)
        {
            ModelImporter importer = (ModelImporter)AssetImporter.GetAtPath(path);
            Debug.Assert(importer != null, "모델이 없다: " + path);
            // 서브디비전 뒤 스무스 노멀을 Blender 가 계산해 FBX 에 넣었다 — Unity 가 다시 계산하지 않는다
            importer.importNormals = ModelImporterNormals.Import;
            // 면마다 UV 가 셀 중심 한 점이라 MikkTSpace 탄젠트가 면마다 달라져 같은 자리 정점이 갈라진다(슬라임 4206 → 7054). 노멀 맵을 쓰지 않으니 끈다
            importer.importTangents = ModelImporterTangents.None;
            importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), FBX_MATERIAL_NAME), palette);
            ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
            report.Append(path);
            foreach (ModelImporterClipAnimation clip in clips)
            {
                clip.name = P03ModelImport.StripClipName(clip.takeName);
                clip.loopTime = System.Array.IndexOf(LOOP_CLIP_NAMES, clip.name) >= 0;
                report.Append(" | ").Append(clip.takeName).Append(" -> ").Append(clip.name).Append(clip.loopTime ? " (loop)" : "");
            }
            report.Append('\n');
            importer.clipAnimations = clips;
            importer.SaveAndReimport();
        }

        private static void AppendMeshStats(string path, StringBuilder report)
        {
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Debug.Assert(model != null, "모델이 없다: " + path);
            SkinnedMeshRenderer skinned = model.GetComponentInChildren<SkinnedMeshRenderer>(true);
            Mesh mesh = skinned != null ? skinned.sharedMesh : model.GetComponentInChildren<MeshFilter>(true).sharedMesh;
            long indexCount = 0;
            for (int i = 0; i < mesh.subMeshCount; ++i)
            {
                indexCount += mesh.GetIndexCount(i);
            }
            report.Append(path)
                .Append(" tris=").Append(indexCount / 3)
                .Append(" verts=").Append(mesh.vertexCount)
                .Append(" normals=").Append(mesh.normals.Length)
                .Append(" bones=").Append(skinned != null ? skinned.bones.Length : 0)
                .Append(" bindposes=").Append(mesh.bindposeCount)
                .Append(" boneWeights=").Append(mesh.GetAllBoneWeights().Length)
                .Append('\n');
        }
    }
}
