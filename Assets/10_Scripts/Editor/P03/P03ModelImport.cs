using System.Text;
using UnityEditor;
using UnityEngine;

namespace Defense.Editor.P03
{
    /// <summary>
    /// P03 샘플 모델(Blender CLI 가 낸 FBX · 팔레트)의 가져오기 설정을 코드로 맞춘다 —
    /// 팔레트 텍스처 Point 필터 · 공유 머티리얼 하나 · 뼈대 접두를 벗긴 클립 이름 · 반복 클립.
    /// </summary>
    public static class P03ModelImport
    {
        public const string FOLDER = "Assets/30_Assets/Models/P03_CLI";
        public const string PALETTE_MATERIAL_PATH = FOLDER + "/palette.mat";

        private const string PALETTE_TEXTURE_PATH = FOLDER + "/palette.png";
        private const string LIT_SHADER_PATH = "Packages/com.unity.render-pipelines.universal/Shaders/Lit.shader";
        private const string FBX_MATERIAL_NAME = "Palette";
        private const float PALETTE_SMOOTHNESS = 0.1f;

        private static readonly string[] MODEL_NAMES = { "slime", "slime_bst", "bat", "bat_bst", "tower", "tower_bst" };
        private static readonly string[] LOOP_CLIP_NAMES = { "Idle", "Hop", "Flap" };

        /// <summary>팔레트 텍스처 · 공유 머티리얼 · 모델 여섯의 클립과 머티리얼 연결을 설정하고, 모델마다 한 줄씩 결과를 돌려준다.</summary>
        public static string Apply()
        {
            StringBuilder report = new StringBuilder();
            ApplyPaletteTexture();
            Material palette = GetOrCreatePaletteMaterial();
            foreach (string modelName in MODEL_NAMES)
            {
                ApplyModel(FOLDER + "/" + modelName + ".fbx", palette, report);
            }
            return report.ToString();
        }

        /// <summary>Blender 가 붙인 뼈대 접두(`SlimeRig|`)와 이름 겹침 꼬리(`.001`)를 벗긴 클립 이름.</summary>
        public static string StripClipName(string takeName)
        {
            int bar = takeName.LastIndexOf('|');
            string name = bar >= 0 ? takeName.Substring(bar + 1) : takeName;
            int dot = name.LastIndexOf('.');
            // Blender 는 동작 이름이 전역이라 슬라임 Hit 뒤에 만든 박쥐 Hit 을 Hit.001 로 바꿔 내보낸다
            if (dot > 0 && IsDigits(name, dot + 1))
            {
                name = name.Substring(0, dot);
            }
            return name;
        }

        private static void ApplyPaletteTexture()
        {
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(PALETTE_TEXTURE_PATH);
            Debug.Assert(importer != null, "palette.png 가 없다: " + PALETTE_TEXTURE_PATH);
            // 4x4 셀 팔레트 — 보간 · 밉맵 · 블록 압축은 이웃 셀 색을 섞는다
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
        }

        private static Material GetOrCreatePaletteMaterial()
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(PALETTE_TEXTURE_PATH);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(PALETTE_MATERIAL_PATH);
            if (material == null)
            {
                Shader lit = AssetDatabase.LoadAssetAtPath<Shader>(LIT_SHADER_PATH);
                Debug.Assert(lit != null, "URP Lit 셰이더가 없다: " + LIT_SHADER_PATH);
                material = new Material(lit);
                AssetDatabase.CreateAsset(material, PALETTE_MATERIAL_PATH);
            }
            material.SetTexture("_BaseMap", texture);
            material.SetFloat("_Smoothness", PALETTE_SMOOTHNESS);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            return material;
        }

        private static void ApplyModel(string path, Material palette, StringBuilder report)
        {
            ModelImporter importer = (ModelImporter)AssetImporter.GetAtPath(path);
            Debug.Assert(importer != null, "모델이 없다: " + path);
            importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), FBX_MATERIAL_NAME), palette);
            ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
            report.Append(path);
            foreach (ModelImporterClipAnimation clip in clips)
            {
                clip.name = StripClipName(clip.takeName);
                clip.loopTime = System.Array.IndexOf(LOOP_CLIP_NAMES, clip.name) >= 0;
                report.Append(" | ").Append(clip.takeName).Append(" -> ").Append(clip.name).Append(clip.loopTime ? " (loop)" : "");
            }
            report.Append('\n');
            importer.clipAnimations = clips;
            importer.SaveAndReimport();
        }

        private static bool IsDigits(string text, int start)
        {
            if (start >= text.Length)
            {
                return false;
            }
            for (int i = start; i < text.Length; ++i)
            {
                if (!char.IsDigit(text[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
