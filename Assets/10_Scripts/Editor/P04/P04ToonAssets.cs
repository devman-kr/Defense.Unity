using System.IO;
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
        public const string BLOB_MATERIAL_PATH = "Assets/30_Assets/Materials/BlobShadow.mat";
        private const string BLOB_SHADER_NAME = "Defense/BlobShadow";
        // 폴더 체계(CLAUDE.md)에 없는 새 폴더 — 렌더링 전용 데이터(램프 · 볼륨 프로필). 보고 「발견」에 적는다
        public const string RENDERING_FOLDER = "Assets/30_Assets/Rendering";
        public const string RAMP_TEXTURE_PATH = RENDERING_FOLDER + "/ToonRamp.png";
        private const int RAMP_WIDTH = 256;

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

        /// <summary>
        /// 램프 텍스처(256×1, 회색)를 `PaletteToon.mat` 의 단계 값(`_LowStep` · `_HighStep` · `_MidLevel` · `_StepSoftness`)으로 굽는다 —
        /// 곡선의 정본은 머티리얼 값 하나. `_RAMP_TEXTURE` 가 켜져 있으면 셰이더는 그 값 대신 이 텍스처를 읽으므로, 값을 바꾸면 다시 굽는다.
        /// 같은 모양에서 텍스처 샘플과 단계 함수의 비용만 비교하려는 것(미지수 ⑬). 블롭 머티리얼도 (없으면) 만든다.
        /// </summary>
        public static string CreateRampAndBlob()
        {
            Directory.CreateDirectory(RENDERING_FOLDER);
            Material toon = LoadToon();
            float lowStep = toon.GetFloat("_LowStep");
            float highStep = toon.GetFloat("_HighStep");
            float midLevel = toon.GetFloat("_MidLevel");
            float softness = toon.GetFloat("_StepSoftness");
            Texture2D ramp = new Texture2D(RAMP_WIDTH, 1, TextureFormat.RGB24, false);
            for (int x = 0; x < RAMP_WIDTH; ++x)
            {
                float amount = (x + 0.5f) / RAMP_WIDTH;
                float low = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(lowStep - softness, lowStep + softness, amount));
                float high = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(highStep - softness, highStep + softness, amount));
                float value = low * midLevel + high * (1f - midLevel);
                ramp.SetPixel(x, 0, new Color(value, value, value));
            }
            File.WriteAllBytes(RAMP_TEXTURE_PATH, ramp.EncodeToPNG());
            Object.DestroyImmediate(ramp);
            AssetDatabase.ImportAsset(RAMP_TEXTURE_PATH);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(RAMP_TEXTURE_PATH);
            // 값 텍스처 — 감마 변환 · 밉맵 · 압축 · 반복 없이
            importer.sRGBTexture = false;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();

            // 다시 가져오기를 동기로 끝낸 뒤 읽는다 — 이 세션 첫 실행에서 null 이 걸려 머티리얼의 램프가 비었다(셰이더 기본 흰색)
            AssetDatabase.ImportAsset(RAMP_TEXTURE_PATH, ImportAssetOptions.ForceSynchronousImport);
            Texture2D rampAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(RAMP_TEXTURE_PATH);
            Debug.Assert(rampAsset != null, "램프 텍스처를 못 읽었다: " + RAMP_TEXTURE_PATH);
            toon.SetTexture("_RampMap", rampAsset);
            EditorUtility.SetDirty(toon);
            StringBuilder report = new StringBuilder();
            CreateOrLoad(BLOB_MATERIAL_PATH, BLOB_SHADER_NAME, report);
            AssetDatabase.SaveAssetIfDirty(toon);
            return report.Append(RAMP_TEXTURE_PATH).Append(" written\n").ToString();
        }

        /// <summary>
        /// 승인 지점 ③(2026-11-02) 사용자 선택을 `PaletteToon.mat` 에 넣는다 — 3단 · 경계 0.02 · 청보라 그림자 · 어두운 단 채도 0.3 ·
        /// 받는 그림자 양자화 · 캐릭터 자기 그림자 끔 · 림 · 스페큘러 · 램프 텍스처 켬, 조명 방향 고정 끔, 외곽선 A 2‰.
        /// 블롭 그림자는 머티리얼이 아니라 씬(캐릭터 그림자 드리우기 끔 + 발밑 쿼드)이 정한다.
        /// </summary>
        public static string ApplyApprovedDefaults()
        {
            Material toon = LoadToon();
            toon.SetFloat("_MidLevel", 0.6f);
            toon.SetFloat("_StepSoftness", 0.02f);
            toon.SetColor("_ShadowTint", new Color(0.58f, 0.62f, 0.78f));
            toon.SetFloat("_ShadowSaturation", 0.3f);
            toon.SetFloat("_OutlineWidth", 2f);
            SetKeyword(toon, SHADOW_BANDS_KEYWORD, "_ShadowBands", true);
            SetKeyword(toon, "_SELF_SHADOW_OFF", "_SelfShadowOff", true);
            SetKeyword(toon, "_RIM", "_Rim", true);
            SetKeyword(toon, "_SPECULAR", "_Specular", true);
            SetKeyword(toon, "_RAMP_TEXTURE", "_UseRamp", true);
            SetKeyword(toon, "_FIXED_LIGHT_DIR", "_FixedLight", false);
            SetOutline(toon, true);
            EditorUtility.SetDirty(toon);
            AssetDatabase.SaveAssetIfDirty(toon);
            return TOON_MATERIAL_PATH + " keywords=" + string.Join(",", toon.shaderKeywords) + " ramp=" + toon.GetTexture("_RampMap");
        }

        public static Material LoadBlob()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(BLOB_MATERIAL_PATH);
            Debug.Assert(material != null, "머티리얼이 없다 — CreateRampAndBlob 을 먼저: " + BLOB_MATERIAL_PATH);
            return material;
        }

        /// <summary>셰이더 키워드 토글 — [Toggle] 속성 값도 같이 맞춰 인스펙터와 어긋나지 않게.</summary>
        public static void SetKeyword(Material material, string keyword, string toggleProperty, bool isOn)
        {
            material.SetFloat(toggleProperty, isOn ? 1f : 0f);
            if (isOn)
            {
                material.EnableKeyword(keyword);
            }
            else
            {
                material.DisableKeyword(keyword);
            }
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
