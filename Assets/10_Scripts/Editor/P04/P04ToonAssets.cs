using System.Text;
using UnityEditor;
using UnityEngine;

namespace Defense.Editor.P04
{
    /// <summary>
    /// P04 S3 카툰 렌더링 에셋 — `PaletteToon.mat`(모델 전부가 쓰는 하나) · `ToonEdgeDetect.mat`(외곽선 B 풀스크린)를 만들고,
    /// 시험 때 머티리얼 사본의 토글(외곽선 패스 · 셰이더 키워드)을 바꾼다. 사본은 에셋으로 저장하지 않는다.
    /// </summary>
    public static class P04ToonAssets
    {
        public const string TOON_MATERIAL_PATH = "Assets/30_Assets/Materials/PaletteToon.mat";
        public const string EDGE_MATERIAL_PATH = "Assets/30_Assets/Materials/ToonEdgeDetect.mat";
        public const string SHADOW_BANDS_KEYWORD = "_SHADOW_BANDS";
        // 외피 패스의 LightMode — URP 포워드가 태그 없는 둘째 패스로 그린다
        private const string OUTLINE_LIGHT_MODE = "SRPDefaultUnlit";
        private const string TOON_SHADER_NAME = "Defense/PaletteToon";
        private const string EDGE_SHADER_NAME = "Defense/ToonEdgeDetect";
        private const string PALETTE_TEXTURE_PATH = "Assets/30_Assets/Models/P04_Smooth/Palette.png";

        /// <summary>두 머티리얼을 (없으면) 만들고 셰이더 기본값으로 둔다 — 외곽선 패스 켬, 받는 그림자 양자화 켬.</summary>
        public static string CreateMaterials()
        {
            StringBuilder report = new StringBuilder();
            Material toon = CreateOrLoad(TOON_MATERIAL_PATH, TOON_SHADER_NAME, report);
            toon.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(PALETTE_TEXTURE_PATH));
            toon.EnableKeyword(SHADOW_BANDS_KEYWORD);
            toon.SetShaderPassEnabled(OUTLINE_LIGHT_MODE, true);
            EditorUtility.SetDirty(toon);
            Material edge = CreateOrLoad(EDGE_MATERIAL_PATH, EDGE_SHADER_NAME, report);
            EditorUtility.SetDirty(edge);
            AssetDatabase.SaveAssets();
            report.Append(TOON_MATERIAL_PATH).Append(" keywords=").Append(string.Join(",", toon.shaderKeywords))
                .Append(" outline=").Append(toon.GetShaderPassEnabled(OUTLINE_LIGHT_MODE)).Append('\n');
            return report.ToString();
        }

        public static Material LoadToon()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(TOON_MATERIAL_PATH);
            Debug.Assert(material != null, "머티리얼이 없다 — CreateMaterials 를 먼저: " + TOON_MATERIAL_PATH);
            return material;
        }

        public static void SetOutline(Material material, bool isOutline)
        {
            material.SetShaderPassEnabled(OUTLINE_LIGHT_MODE, isOutline);
        }

        private static Material CreateOrLoad(string path, string shaderName, StringBuilder report)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                report.Append(path).Append(" exists\n");
                return material;
            }
            Shader shader = Shader.Find(shaderName);
            Debug.Assert(shader != null, "셰이더가 없다: " + shaderName);
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
            report.Append(path).Append(" created\n");
            return material;
        }
    }
}
