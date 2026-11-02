using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Defense.Editor.P04
{
    /// <summary>
    /// 캡처 PNG 여러 장을 격자 한 장(대조 시트)으로 합친다 — 칸마다 위에 라벨, 맨 위에 제목.
    /// 라벨은 5×7 비트맵 글꼴(대문자 · 숫자 · 기호 몇 개)로 그린다 — 글꼴 에셋 없이 창 없는 에디터에서 같은 결과.
    /// </summary>
    public static class P04ContactSheet
    {
        private const int GLYPH_WIDTH = 5;
        private const int GLYPH_HEIGHT = 7;
        private const int TEXT_SCALE = 3;
        private const int LABEL_HEIGHT = 36;
        private const int TITLE_HEIGHT = 48;
        private const int GAP = 8;
        private static readonly Color BACKGROUND = new Color(0.12f, 0.12f, 0.14f);
        private static readonly Color TEXT = new Color(0.95f, 0.95f, 0.95f);

        /// <summary>paths[i] 를 labels[i] 아래에 columns 칸 격자로 — 칸 폭 cellWidth, 높이는 첫 그림의 비율. 결과 크기를 돌려준다.</summary>
        public static string Compose(string outPath, string title, string[] paths, string[] labels, int columns, int cellWidth)
        {
            Debug.Assert(paths.Length == labels.Length && paths.Length > 0, "그림 · 라벨 수가 다르거나 없다");
            List<Texture2D> images = new List<Texture2D>();
            foreach (string path in paths)
            {
                // 빈 경로는 빈 칸(라벨만) — 값이 둘뿐인 손잡이의 셋째 칸
                if (string.IsNullOrEmpty(path))
                {
                    images.Add(null);
                    continue;
                }
                Texture2D image = new Texture2D(2, 2, TextureFormat.RGB24, false);
                bool isLoaded = image.LoadImage(File.ReadAllBytes(path));
                Debug.Assert(isLoaded, "PNG 를 못 읽었다: " + path);
                images.Add(image);
            }
            Texture2D first = images.Find(image => image != null);
            Debug.Assert(first != null, "그림이 하나도 없다");
            int cellHeight = Mathf.RoundToInt(cellWidth * (float)first.height / first.width);
            int rows = (paths.Length + columns - 1) / columns;
            int width = columns * cellWidth + (columns + 1) * GAP;
            int height = TITLE_HEIGHT + rows * (LABEL_HEIGHT + cellHeight + GAP) + GAP;
            Texture2D sheet = new Texture2D(width, height, TextureFormat.RGB24, false);
            Color[] fill = new Color[width * height];
            for (int i = 0; i < fill.Length; ++i)
            {
                fill[i] = BACKGROUND;
            }
            sheet.SetPixels(fill);
            DrawText(sheet, title, GAP, height - TITLE_HEIGHT + (TITLE_HEIGHT - GLYPH_HEIGHT * TEXT_SCALE) / 2);

            for (int i = 0; i < images.Count; ++i)
            {
                int column = i % columns;
                int row = i / columns;
                int x = GAP + column * (cellWidth + GAP);
                int top = height - TITLE_HEIGHT - row * (LABEL_HEIGHT + cellHeight + GAP);
                DrawText(sheet, labels[i], x, top - LABEL_HEIGHT + (LABEL_HEIGHT - GLYPH_HEIGHT * TEXT_SCALE) / 2);
                if (images[i] != null)
                {
                    Blit(sheet, images[i], x, top - LABEL_HEIGHT - cellHeight, cellWidth, cellHeight);
                }
            }
            sheet.Apply();
            File.WriteAllBytes(outPath, sheet.EncodeToPNG());
            foreach (Texture2D image in images)
            {
                if (image != null)
                {
                    Object.DestroyImmediate(image);
                }
            }
            Object.DestroyImmediate(sheet);
            return outPath + " " + width + "x" + height;
        }

        // 겹선형 샘플로 줄여 넣는다 — 칸 비율이 다르면 가운데 맞춤으로 잘라 넣지 않고 늘린다(같은 크기 캡처끼리만 합친다)
        private static void Blit(Texture2D sheet, Texture2D image, int x, int y, int cellWidth, int cellHeight)
        {
            for (int py = 0; py < cellHeight; ++py)
            {
                for (int px = 0; px < cellWidth; ++px)
                {
                    Color color = image.GetPixelBilinear((px + 0.5f) / cellWidth, (py + 0.5f) / cellHeight);
                    sheet.SetPixel(x + px, y + py, color);
                }
            }
        }

        private static void DrawText(Texture2D sheet, string text, int x, int y)
        {
            int cursor = x;
            foreach (char raw in text.ToUpperInvariant())
            {
                if (GLYPHS.TryGetValue(raw, out string[] rows))
                {
                    for (int gy = 0; gy < GLYPH_HEIGHT; ++gy)
                    {
                        for (int gx = 0; gx < GLYPH_WIDTH; ++gx)
                        {
                            if (rows[gy][gx] != '#')
                            {
                                continue;
                            }
                            for (int sy = 0; sy < TEXT_SCALE; ++sy)
                            {
                                for (int sx = 0; sx < TEXT_SCALE; ++sx)
                                {
                                    int px = cursor + gx * TEXT_SCALE + sx;
                                    int py = y + (GLYPH_HEIGHT - 1 - gy) * TEXT_SCALE + sy;
                                    if (px < sheet.width && py >= 0 && py < sheet.height)
                                    {
                                        sheet.SetPixel(px, py, TEXT);
                                    }
                                }
                            }
                        }
                    }
                }
                cursor += (GLYPH_WIDTH + 1) * TEXT_SCALE;
            }
        }

        private static readonly Dictionary<char, string[]> GLYPHS = new Dictionary<char, string[]>
        {
            { 'A', new[] { " ### ", "#   #", "#   #", "#####", "#   #", "#   #", "#   #" } },
            { 'B', new[] { "#### ", "#   #", "#   #", "#### ", "#   #", "#   #", "#### " } },
            { 'C', new[] { " ### ", "#   #", "#    ", "#    ", "#    ", "#   #", " ### " } },
            { 'D', new[] { "#### ", "#   #", "#   #", "#   #", "#   #", "#   #", "#### " } },
            { 'E', new[] { "#####", "#    ", "#    ", "#### ", "#    ", "#    ", "#####" } },
            { 'F', new[] { "#####", "#    ", "#    ", "#### ", "#    ", "#    ", "#    " } },
            { 'G', new[] { " ### ", "#   #", "#    ", "# ###", "#   #", "#   #", " ####" } },
            { 'H', new[] { "#   #", "#   #", "#   #", "#####", "#   #", "#   #", "#   #" } },
            { 'I', new[] { " ### ", "  #  ", "  #  ", "  #  ", "  #  ", "  #  ", " ### " } },
            { 'J', new[] { "  ###", "   # ", "   # ", "   # ", "   # ", "#  # ", " ##  " } },
            { 'K', new[] { "#   #", "#  # ", "# #  ", "##   ", "# #  ", "#  # ", "#   #" } },
            { 'L', new[] { "#    ", "#    ", "#    ", "#    ", "#    ", "#    ", "#####" } },
            { 'M', new[] { "#   #", "## ##", "# # #", "# # #", "#   #", "#   #", "#   #" } },
            { 'N', new[] { "#   #", "#   #", "##  #", "# # #", "#  ##", "#   #", "#   #" } },
            { 'O', new[] { " ### ", "#   #", "#   #", "#   #", "#   #", "#   #", " ### " } },
            { 'P', new[] { "#### ", "#   #", "#   #", "#### ", "#    ", "#    ", "#    " } },
            { 'Q', new[] { " ### ", "#   #", "#   #", "#   #", "# # #", "#  # ", " ## #" } },
            { 'R', new[] { "#### ", "#   #", "#   #", "#### ", "# #  ", "#  # ", "#   #" } },
            { 'S', new[] { " ####", "#    ", "#    ", " ### ", "    #", "    #", "#### " } },
            { 'T', new[] { "#####", "  #  ", "  #  ", "  #  ", "  #  ", "  #  ", "  #  " } },
            { 'U', new[] { "#   #", "#   #", "#   #", "#   #", "#   #", "#   #", " ### " } },
            { 'V', new[] { "#   #", "#   #", "#   #", "#   #", "#   #", " # # ", "  #  " } },
            { 'W', new[] { "#   #", "#   #", "#   #", "# # #", "# # #", "# # #", " # # " } },
            { 'X', new[] { "#   #", "#   #", " # # ", "  #  ", " # # ", "#   #", "#   #" } },
            { 'Y', new[] { "#   #", "#   #", " # # ", "  #  ", "  #  ", "  #  ", "  #  " } },
            { 'Z', new[] { "#####", "    #", "   # ", "  #  ", " #   ", "#    ", "#####" } },
            { '0', new[] { " ### ", "#   #", "#  ##", "# # #", "##  #", "#   #", " ### " } },
            { '1', new[] { "  #  ", " ##  ", "  #  ", "  #  ", "  #  ", "  #  ", " ### " } },
            { '2', new[] { " ### ", "#   #", "    #", "   # ", "  #  ", " #   ", "#####" } },
            { '3', new[] { "#####", "   # ", "  #  ", "   # ", "    #", "#   #", " ### " } },
            { '4', new[] { "   # ", "  ## ", " # # ", "#  # ", "#####", "   # ", "   # " } },
            { '5', new[] { "#####", "#    ", "#### ", "    #", "    #", "#   #", " ### " } },
            { '6', new[] { "  ## ", " #   ", "#    ", "#### ", "#   #", "#   #", " ### " } },
            { '7', new[] { "#####", "    #", "   # ", "  #  ", " #   ", " #   ", " #   " } },
            { '8', new[] { " ### ", "#   #", "#   #", " ### ", "#   #", "#   #", " ### " } },
            { '9', new[] { " ### ", "#   #", "#   #", " ####", "    #", "   # ", " ##  " } },
            { '-', new[] { "     ", "     ", "     ", "#####", "     ", "     ", "     " } },
            { '_', new[] { "     ", "     ", "     ", "     ", "     ", "     ", "#####" } },
            { '.', new[] { "     ", "     ", "     ", "     ", "     ", "     ", "  #  " } },
            { ',', new[] { "     ", "     ", "     ", "     ", "     ", "  #  ", " #   " } },
            { ':', new[] { "     ", "  #  ", "     ", "     ", "     ", "  #  ", "     " } },
            { '/', new[] { "    #", "    #", "   # ", "  #  ", " #   ", "#    ", "#    " } },
            { '(', new[] { "   # ", "  #  ", " #   ", " #   ", " #   ", "  #  ", "   # " } },
            { ')', new[] { " #   ", "  #  ", "   # ", "   # ", "   # ", "  #  ", " #   " } },
            { '+', new[] { "     ", "  #  ", "  #  ", "#####", "  #  ", "  #  ", "     " } },
            { '=', new[] { "     ", "     ", "#####", "     ", "#####", "     ", "     " } },
            { '#', new[] { " # # ", "#####", " # # ", " # # ", "#####", " # # ", "     " } },
        };
    }
}
