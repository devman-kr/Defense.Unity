using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Defense.Editor.P04
{
    /// <summary>
    /// P04 S3 승인 지점 ③ — 손잡이마다 2~3 값을 ③ 직전 기준에서 한 곳만 바꿔 Mobile 품질로 찍고, 손잡이 한 줄 · 값 한 칸의 타일 시트로 합친다.
    /// 기준 칸은 다시 찍지 않고 「BASE」 캡처를 같이 쓴다. 값 둘뿐인 줄은 셋째 칸을 비운다.
    /// </summary>
    public static class P04ToonTiles
    {
        private const string BASE_LABEL = "T00_Base";
        private const string NEUTRAL_VOLUME_PATH = P04ToonAssets.RENDERING_FOLDER + "/ToonVolume_Neutral.asset";
        private const int COLUMNS = 3;
        private static readonly string[] SHOTS = { "Lineup5", "CloseSlime", "CloseArcher", "Portrait" };
        private static readonly int[] SHOT_CELL_WIDTHS = { 640, 400, 400, 300 };
        private static readonly int[] MOBILE_ONLY = { P04ToonCapture.MOBILE_QUALITY };

        private sealed class Tile
        {
            public readonly string Label;
            public readonly string Caption;
            public readonly P04ToonCapture.Options Options;

            public Tile(string label, string caption, P04ToonCapture.Options options)
            {
                Label = label;
                Caption = caption;
                Options = options;
            }
        }

        private sealed class Knob
        {
            public readonly string Name;
            public readonly Tile[] Tiles;   // 기준 칸은 null

            public Knob(string name, params Tile[] tiles)
            {
                Name = name;
                Tiles = tiles;
            }
        }

        /// <summary>
        /// 기준 + 손잡이 값마다 Mobile 4장씩 outDir 에 찍고, 손잡이 표 한 줄씩 돌려준다.
        /// 기준은 지금 머티리얼 값이 아니라 ③ 직전의 고정값(<see cref="ApplyBaseline"/>) — ③ 선택이 머티리얼에 들어간 뒤 다시 돌려도 라벨이 그림과 맞게(센서 P04.S3.UNI.04).
        /// </summary>
        public static string CaptureAll(string outDir)
        {
            Material toon = P04ToonAssets.LoadToon();
            StringBuilder report = new StringBuilder();
            P04ToonCapture.CaptureSet(outDir, BASE_LABEL, toon, new P04ToonCapture.Options { Configure = ApplyBaseline, Qualities = MOBILE_ONLY });
            foreach (Knob knob in GetKnobs())
            {
                report.Append(knob.Name).Append(':');
                foreach (Tile tile in knob.Tiles)
                {
                    if (tile == null)
                    {
                        report.Append(" BASE");
                        continue;
                    }
                    System.Action<Material> change = tile.Options.Configure;
                    tile.Options.Configure = material =>
                    {
                        ApplyBaseline(material);
                        if (change != null)
                        {
                            change(material);
                        }
                    };
                    tile.Options.Qualities = MOBILE_ONLY;
                    P04ToonCapture.CaptureSet(outDir, tile.Label, toon, tile.Options);
                    report.Append(' ').Append(tile.Label);
                }
                report.Append('\n');
            }
            return report.ToString();
        }

        /// <summary>찍은 타일을 장면마다 한 장씩(손잡이 줄 × 값 칸) sheetDir 에 합친다.</summary>
        public static string ComposeSheets(string tileDir, string sheetDir, string prefix)
        {
            StringBuilder report = new StringBuilder();
            for (int s = 0; s < SHOTS.Length; ++s)
            {
                List<string> paths = new List<string>();
                List<string> labels = new List<string>();
                foreach (Knob knob in GetKnobs())
                {
                    for (int column = 0; column < COLUMNS; ++column)
                    {
                        Tile tile = column < knob.Tiles.Length ? knob.Tiles[column] : null;
                        bool isBase = column < knob.Tiles.Length && tile == null;
                        if (column >= knob.Tiles.Length)
                        {
                            paths.Add(string.Empty);
                            labels.Add(string.Empty);
                            continue;
                        }
                        string label = isBase ? BASE_LABEL : tile.Label;
                        paths.Add(Path.Combine(tileDir, label + "_" + P04ToonCapture.GetQualityName(P04ToonCapture.MOBILE_QUALITY) + "_" + SHOTS[s] + ".png"));
                        labels.Add(knob.Name + ": " + (isBase ? GetBaseCaption(knob) : tile.Caption));
                    }
                }
                string outPath = Path.Combine(sheetDir, prefix + SHOTS[s] + ".png");
                report.Append(P04ContactSheet.Compose(outPath, "3 TILES - MOBILE - " + SHOTS[s] + " - ONE KNOB PER ROW (BASE = BASELINE BEFORE CHOICE 3)", paths.ToArray(), labels.ToArray(), COLUMNS, SHOT_CELL_WIDTHS[s])).Append('\n');
            }
            return report.ToString();
        }

        // 기준 칸의 라벨 — 그 손잡이에서 PaletteToon.mat 의 지금 값
        private static string GetBaseCaption(Knob knob)
        {
            switch (knob.Name)
            {
                case "1 BANDS": return "3 (BASE)";
                case "2 EDGE": return "0.02 (BASE)";
                case "3 TINT": return "COOL (BASE)";
                case "4 DARK SAT": return "0.3 (BASE)";
                case "5 SHADOW QUANT": return "ON (BASE)";
                case "6 SHADOW": return "REALTIME (BASE)";
                case "7 SELF SHADOW": return "RECEIVE (BASE)";
                case "8 RIM": return "OFF (BASE)";
                case "9 SPECULAR": return "OFF (BASE)";
                case "10 FIXED LIGHT": return "OFF (BASE)";
                case "11 RAMP TEX": return "OFF (BASE)";
                case "12 TONEMAP": return "NONE POST OFF (BASE)";
                case "13 OUTLINE": return "2 (BASE)";
                default:
                    Debug.Assert(false, "모르는 손잡이: " + knob.Name);
                    Debug.LogError("모르는 손잡이: " + knob.Name);
                    return "BASE";
            }
        }

        /// <summary>
        /// ③ 직전의 기준 — 3단 · 경계 0.02 · 청보라 · 채도 0.3 · 외곽선 2 · 받는 그림자 양자화만 켬(캐릭터 전용 기법 · 램프 · 조명 고정 끔).
        /// 타일 칸은 이 기준에서 한 값만 바꾼다.
        /// </summary>
        private static void ApplyBaseline(Material material)
        {
            material.SetFloat("_MidLevel", 0.6f);
            material.SetFloat("_StepSoftness", 0.02f);
            material.SetColor("_ShadowTint", new Color(0.58f, 0.62f, 0.78f));
            material.SetFloat("_ShadowSaturation", 0.3f);
            material.SetFloat("_OutlineWidth", 2f);
            P04ToonAssets.SetOutline(material, true);
            P04ToonAssets.SetKeyword(material, P04ToonAssets.SHADOW_BANDS_KEYWORD, "_ShadowBands", true);
            P04ToonAssets.SetKeyword(material, "_SELF_SHADOW_OFF", "_SelfShadowOff", false);
            P04ToonAssets.SetKeyword(material, "_RIM", "_Rim", false);
            P04ToonAssets.SetKeyword(material, "_SPECULAR", "_Specular", false);
            P04ToonAssets.SetKeyword(material, "_RAMP_TEXTURE", "_UseRamp", false);
            P04ToonAssets.SetKeyword(material, "_FIXED_LIGHT_DIR", "_FixedLight", false);
        }

        private static Knob[] GetKnobs()
        {
            return new Knob[]
            {
                new Knob("1 BANDS", Configure("T01_TwoTone", "2", m => m.SetFloat("_MidLevel", 1f)), null),
                new Knob("2 EDGE", Configure("T02_Edge005", "0.005", m => m.SetFloat("_StepSoftness", 0.005f)), null,
                    Configure("T02_Edge06", "0.06", m => m.SetFloat("_StepSoftness", 0.06f))),
                new Knob("3 TINT", null, Configure("T03_Warm", "WARM", m => m.SetColor("_ShadowTint", new Color(0.74f, 0.62f, 0.66f))),
                    Configure("T03_Deep", "DEEP", m => m.SetColor("_ShadowTint", new Color(0.45f, 0.48f, 0.68f)))),
                new Knob("4 DARK SAT", Configure("T04_Sat0", "0", m => m.SetFloat("_ShadowSaturation", 0f)), null,
                    Configure("T04_Sat06", "0.6", m => m.SetFloat("_ShadowSaturation", 0.6f))),
                new Knob("5 SHADOW QUANT", Configure("T05_QuantOff", "OFF", m => P04ToonAssets.SetKeyword(m, P04ToonAssets.SHADOW_BANDS_KEYWORD, "_ShadowBands", false)), null),
                new Knob("6 SHADOW", null, new Tile("T06_Blob", "BLOB", new P04ToonCapture.Options { PrepareScene = scene => P04ToonCapture.UseBlobShadows(scene) })),
                new Knob("7 SELF SHADOW", null, Configure("T07_SelfOff", "CHARACTERS OFF", m => P04ToonAssets.SetKeyword(m, "_SELF_SHADOW_OFF", "_SelfShadowOff", true))),
                new Knob("8 RIM", null, Configure("T08_Rim", "ON", m => P04ToonAssets.SetKeyword(m, "_RIM", "_Rim", true))),
                new Knob("9 SPECULAR", null, Configure("T09_Specular", "ON", m => P04ToonAssets.SetKeyword(m, "_SPECULAR", "_Specular", true))),
                new Knob("10 FIXED LIGHT", null, Configure("T10_FixedLight", "ON", m => P04ToonAssets.SetKeyword(m, "_FIXED_LIGHT_DIR", "_FixedLight", true))),
                new Knob("11 RAMP TEX", null, Configure("T11_Ramp", "ON (SAME CURVE)", m => P04ToonAssets.SetKeyword(m, "_RAMP_TEXTURE", "_UseRamp", true))),
                new Knob("12 TONEMAP", null, new Tile("T12_PostOnly", "NONE (POST ON)", new P04ToonCapture.Options { IsPostProcessing = true }),
                    new Tile("T12_Neutral", "NEUTRAL (POST ON)", new P04ToonCapture.Options
                {
                    PrepareScene = scene => P04ToonCapture.AddGlobalVolume(scene, NEUTRAL_VOLUME_PATH),
                    IsPostProcessing = true,
                })),
                new Knob("13 OUTLINE", Configure("T13_Outline1", "1", m => m.SetFloat("_OutlineWidth", 1f)), null,
                    Configure("T13_Outline35", "3.5", m => m.SetFloat("_OutlineWidth", 3.5f))),
            };
        }

        private static Tile Configure(string label, string caption, System.Action<Material> configure)
        {
            return new Tile(label, caption, new P04ToonCapture.Options { Configure = configure });
        }
    }
}
