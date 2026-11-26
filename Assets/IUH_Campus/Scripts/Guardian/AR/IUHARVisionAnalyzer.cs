using System;
using System.Collections.Generic;
using UnityEngine;

namespace IUHCampus.Guardian.AR
{
    /// <summary>
    /// Các đặc trưng thị giác trích xuất từ khung hình camera AR
    /// </summary>
    public struct VisionFeatures
    {
        public float globalContrast;       // Độ tương phản toàn cảnh (maxLuma - minLuma)
        public float avgPatchStdDev;       // Độ lệch chuẩn trung bình của lưới 6x6 ô
        public float maxPatchStdDev;       // Độ lệch chuẩn cao nhất trong 36 ô
        public int richTexturePatchCount;  // Số ô có chi tiết cao (stdDev >= 11.5f)
        public float brightPaperRatio;     // Tỉ lệ điểm ảnh giấy sáng trắng
        public float darkPixelsRatio;      // Tỉ lệ điểm ảnh tối (viền máy, bezel, bàn phím, vải tối)
        public float avgColorSat;          // Độ bão hòa màu trung bình
        public float microEdgeFraction;    // Mật độ vi cạnh sắc nét
        public int highFreqTextLines;      // Số dòng quét có dao động tần số cao (chữ in san sát)
        public int mediumFreqKeyLines;     // Số dòng quét có dao động tần số trung bình (hàng phím bấm)
        public int totalScanTransitions;   // Tổng số lần chuyển đổi sáng-tối trên các dòng quét
    }

    public struct VisionAnalysisResult
    {
        public bool isSuccess;
        public int targetPropIndex; // 0: Balo, 1: Laptop, 2: Sách
        public float confidence;
        public string message;
        public string details;
    }

    /// <summary>
    /// Bộ phân tích thị giác nhận diện đồ vật AR: Laptop, Balo, Sách Giáo Trình & Vách Tường
    /// </summary>
    public static class IUHARVisionAnalyzer
    {
        /// <summary>
        /// Trích xuất đặc trưng thị giác từ mảng điểm ảnh (Color32[])
        /// </summary>
        public static VisionFeatures ExtractFeatures(Color32[] pixels, int w, int h)
        {
            var f = new VisionFeatures();
            if (pixels == null || pixels.Length < w * h || w < 32 || h < 32)
                return f;

            int startX = (int)(w * 0.20f);
            int endX = (int)(w * 0.80f);
            int startY = (int)(h * 0.20f);
            int endY = (int)(h * 0.80f);
            int regionW = endX - startX;
            int regionH = endY - startY;

            AnalyzeGridPatches(pixels, w, startX, startY, regionW, regionH, ref f);
            f.microEdgeFraction = ComputeMicroEdgeFraction(pixels, w, startX, endX, startY, endY, regionW);
            ComputeScanLineMetrics(pixels, w, startX, endX, startY, endY, regionW, regionH, ref f);
            return f;
        }

        private static float ComputeLuma(Color32 c) => 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;

        private static void AnalyzeGridPatches(
            Color32[] pixels, int w, int startX, int startY, int regionW, int regionH, ref VisionFeatures f)
        {
            const int gridCols = 6;
            const int gridRows = 6;
            int cellW = regionW / gridCols;
            int cellH = regionH / gridRows;

            float patchStdDevSum = 0f, maxStd = 0f, minLuma = 255f, maxLuma = 0f, sumSat = 0f;
            int richPatches = 0, brightPaperCount = 0, darkCount = 0, totalSamples = 0;

            for (int gy = 0; gy < gridRows; gy++)
            {
                for (int gx = 0; gx < gridCols; gx++)
                {
                    float std = SamplePatchStdDev(
                        pixels, w, startX + gx * cellW, startY + gy * cellH, cellW, cellH,
                        ref minLuma, ref maxLuma, ref brightPaperCount, ref darkCount, ref totalSamples, ref sumSat);
                    if (std < 0f) continue;
                    patchStdDevSum += std;
                    if (std > maxStd) maxStd = std;
                    if (std >= 11.5f) richPatches++;
                }
            }

            int totalPatches = gridCols * gridRows;
            f.globalContrast = maxLuma - minLuma;
            f.avgPatchStdDev = totalPatches > 0 ? (patchStdDevSum / totalPatches) : 0f;
            f.maxPatchStdDev = maxStd;
            f.richTexturePatchCount = richPatches;
            f.brightPaperRatio = totalSamples > 0 ? ((float)brightPaperCount / totalSamples) : 0f;
            f.darkPixelsRatio = totalSamples > 0 ? ((float)darkCount / totalSamples) : 0f;
            f.avgColorSat = totalSamples > 0 ? (sumSat / totalSamples) : 0f;
        }

        private static float SamplePatchStdDev(
            Color32[] pixels, int w, int cx0, int cy0, int cellW, int cellH,
            ref float minLuma, ref float maxLuma, ref int brightPaperCount,
            ref int darkCount, ref int totalSamples, ref float sumSat)
        {
            int stepX = Mathf.Max(2, cellW / 6);
            int stepY = Mathf.Max(2, cellH / 6);
            float cellSumLuma = 0f;
            var cellLumas = new List<float>(36);

            for (int py = cy0; py < cy0 + cellH; py += stepY)
            {
                for (int px = cx0; px < cx0 + cellW; px += stepX)
                {
                    int idx = py * w + px;
                    if (idx < 0 || idx >= pixels.Length) continue;

                    Color32 c = pixels[idx];
                    float lum = ComputeLuma(c);
                    cellSumLuma += lum;
                    cellLumas.Add(lum);

                    if (lum < minLuma) minLuma = lum;
                    if (lum > maxLuma) maxLuma = lum;

                    totalSamples++;
                    if (lum > 155f && Mathf.Abs(c.r - c.g) < 28 && Mathf.Abs(c.g - c.b) < 28) brightPaperCount++;
                    if (lum < 75f) darkCount++;

                    float maxC = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
                    float minC = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
                    sumSat += maxC > 0 ? (maxC - minC) / maxC : 0f;
                }
            }

            if (cellLumas.Count < 8) return -1f;
            float mean = cellSumLuma / cellLumas.Count;
            float variance = 0f;
            for (int i = 0; i < cellLumas.Count; i++)
            {
                float d = cellLumas[i] - mean;
                variance += d * d;
            }
            return Mathf.Sqrt(variance / cellLumas.Count);
        }

        private static float ComputeMicroEdgeFraction(
            Color32[] pixels, int w, int startX, int endX, int startY, int endY, int regionW)
        {
            int totalEdgeSamples = 0, sharpEdges = 0;
            int edgeStep = Mathf.Max(3, regionW / 50);

            for (int y = startY; y < endY - 2; y += edgeStep)
            {
                for (int x = startX; x < endX - 2; x += edgeStep)
                {
                    int idxC = y * w + x;
                    int idxR = y * w + (x + 2);
                    int idxU = (y + 2) * w + x;
                    if (idxU >= pixels.Length || idxR >= pixels.Length) continue;

                    float lumC = ComputeLuma(pixels[idxC]);
                    float lumR = ComputeLuma(pixels[idxR]);
                    float lumU = ComputeLuma(pixels[idxU]);
                    float grad = (Mathf.Abs(lumR - lumC) + Mathf.Abs(lumU - lumC)) * 0.5f;
                    if (grad >= 20.0f) sharpEdges++;
                    totalEdgeSamples++;
                }
            }
            return totalEdgeSamples > 0 ? ((float)sharpEdges / totalEdgeSamples) : 0f;
        }

        private static void ComputeScanLineMetrics(
            Color32[] pixels, int w, int startX, int endX, int startY, int endY, int regionW, int regionH, ref VisionFeatures f)
        {
            const int lineScanCount = 7;
            int lineStepY = Mathf.Max(2, regionH / (lineScanCount + 1));
            int scanStepX = Mathf.Max(2, regionW / 50);

            int highLines = 0, medLines = 0, totalTrans = 0;
            for (int l = 1; l <= lineScanCount; l++)
            {
                int ly = startY + l * lineStepY;
                if (ly >= endY) break;

                int transitions = CountScanLineTransitions(pixels, w, ly, startX, endX, scanStepX);
                totalTrans += transitions;
                if (transitions >= 12) highLines++;
                else if (transitions >= 4 && transitions <= 11) medLines++;
            }

            f.highFreqTextLines = highLines;
            f.mediumFreqKeyLines = medLines;
            f.totalScanTransitions = totalTrans;
        }

        private static int CountScanLineTransitions(Color32[] pixels, int w, int ly, int startX, int endX, int scanStepX)
        {
            int transitions = 0, prevSign = 0;
            float prevVal = -1f;
            for (int lx = startX; lx < endX; lx += scanStepX)
            {
                int pIdx = ly * w + lx;
                if (pIdx < 0 || pIdx >= pixels.Length) continue;

                float curVal = ComputeLuma(pixels[pIdx]);
                if (prevVal >= 0f)
                {
                    float delta = curVal - prevVal;
                    if (Mathf.Abs(delta) > 20f)
                    {
                        int curSign = delta > 0 ? 1 : -1;
                        if (prevSign != 0 && curSign != prevSign) transitions++;
                        prevSign = curSign;
                    }
                }
                prevVal = curVal;
            }
            return transitions;
        }

        /// <summary>
        /// Kiểm tra xem bề mặt có phải là vách tường hoặc mặt phẳng trơn hay không
        /// </summary>
        public static bool IsPlainWall(VisionFeatures f)
        {
            return (f.globalContrast < 40f && f.maxPatchStdDev < 12.0f)
                || (f.richTexturePatchCount == 0 && f.maxPatchStdDev < 11.0f && f.microEdgeFraction < 0.028f)
                || (f.avgPatchStdDev < 3.8f && f.maxPatchStdDev < 11.5f);
        }

        /// <summary>
        /// Phân tích mục tiêu cụ thể theo tab đang chọn (0: Balo, 1: Laptop, 2: Sách)
        /// </summary>
        public static VisionAnalysisResult Analyze(VisionFeatures f, int targetPropIndex)
        {
            bool isWall = IsPlainWall(f);
            if (targetPropIndex == 1) return AnalyzeLaptop(f, isWall);
            if (targetPropIndex == 0) return AnalyzeBackpack(f, isWall);
            return AnalyzeBook(f, targetPropIndex, isWall);
        }

        private static VisionAnalysisResult MakeResult(int targetPropIndex, bool success, float confidence, string message, string details = null)
        {
            return new VisionAnalysisResult
            {
                targetPropIndex = targetPropIndex,
                isSuccess = success,
                confidence = confidence,
                message = message,
                details = details
            };
        }

        private static VisionAnalysisResult AnalyzeLaptop(VisionFeatures f, bool isWall)
        {
            if (isWall || (f.richTexturePatchCount < 2 && f.microEdgeFraction < 0.030f && f.globalContrast < 45f))
            {
                return MakeResult(1, false, 18f,
                    "KHÔNG PHẢI LAPTOP! Đây là Vách Tường/Mặt Trơn. Hãy hướng camera vào Laptop thật.",
                    $"Tương phản: {f.globalContrast:F0}, Ô chi tiết: {f.richTexturePatchCount}/36");
            }

            bool isPurePhysicalPaperBook = f.brightPaperRatio >= 0.75f && f.darkPixelsRatio < 0.05f && f.mediumFreqKeyLines == 0 && f.richTexturePatchCount <= 4;
            if (isPurePhysicalPaperBook)
            {
                return MakeResult(1, false, 25f,
                    "KHÔNG PHẢI LAPTOP! Đây là Sách / Trang Giấy Trắng. Hãy hướng camera vào Màn hình & Bàn phím Laptop.",
                    $"Tỉ lệ giấy trắng cao ({f.brightPaperRatio * 100f:F0}%), không thấy viền bezel/bàn phím");
            }

            bool hasLaptopScreen = f.globalContrast >= 45f && (f.richTexturePatchCount >= 2 || f.microEdgeFraction >= 0.030f) && f.maxPatchStdDev >= 11.0f;
            bool hasLaptopKeyboard = (f.mediumFreqKeyLines >= 1 && (f.richTexturePatchCount >= 2 || f.darkPixelsRatio >= 0.08f))
                || (f.totalScanTransitions >= 15 && f.darkPixelsRatio >= 0.08f);

            if (hasLaptopScreen || hasLaptopKeyboard)
            {
                float baseConf = hasLaptopKeyboard ? 88f : 86f;
                float conf = Mathf.Clamp(baseConf + (f.mediumFreqKeyLines * 2.5f) + (f.darkPixelsRatio * 15f) + (f.richTexturePatchCount * 0.8f), 86f, 98f);
                return MakeResult(1, true, conf, "ĐÃ KHÓA MỤC TIÊU: LAPTOP / MÁY TÍNH THẬT",
                    $"Màn hình/Bàn phím: {f.mediumFreqKeyLines} hàng phím, Viền/Vùng tối: {f.darkPixelsRatio * 100f:F0}%, Vi cạnh: {f.microEdgeFraction * 100f:F1}%");
            }

            return MakeResult(1, false, 25f,
                "KHÔNG PHẢI LAPTOP! Hãy đưa camera lại gần Bàn phím & Màn hình máy tính.",
                "Chưa thấy rõ khung màn hình hoặc bàn phím");
        }

        private static VisionAnalysisResult AnalyzeBackpack(VisionFeatures f, bool isWall)
        {
            if (isWall)
            {
                return MakeResult(0, false, 18f,
                    "KHÔNG PHẢI BALO! Đây là Vách Tường/Mặt Trơn. Hãy hướng camera vào Balo của bạn.",
                    $"Độ nhẵn: {f.avgPatchStdDev:F1}, Tương phản: {f.globalContrast:F0}");
            }

            if (f.brightPaperRatio > 0.50f && f.darkPixelsRatio < 0.20f)
            {
                return MakeResult(0, false, 22f,
                    "KHÔNG PHẢI BALO! Đây là Sách / Trang Giấy Trắng. Hãy hướng camera vào Balo của bạn.");
            }

            bool hasBackpackBody = (f.globalContrast >= 45f && f.maxPatchStdDev >= 11.0f)
                || f.richTexturePatchCount >= 2
                || (f.darkPixelsRatio >= 0.15f && f.avgPatchStdDev >= 4.2f);

            if (hasBackpackBody)
            {
                float conf = Mathf.Clamp(86f + (f.maxPatchStdDev - 11f) * 0.5f + f.darkPixelsRatio * 12f, 86f, 98f);
                return MakeResult(0, true, conf, "ĐÃ KHÓA MỤC TIÊU: BALO THẬT",
                    $"Vải/Quai đeo: {f.maxPatchStdDev:F1}, Vùng vải tối: {f.darkPixelsRatio * 100f:F0}%, Độ nổi khối: {f.globalContrast:F0}");
            }

            return MakeResult(0, false, 20f,
                "KHÔNG PHẢI BALO! Hãy đưa camera lại gần Balo để bắt rõ quai đeo hoặc nếp gấp vải.");
        }

        private static VisionAnalysisResult AnalyzeBook(VisionFeatures f, int targetPropIndex, bool isWall)
        {
            if (isWall || f.richTexturePatchCount < 2)
            {
                return MakeResult(targetPropIndex, false, 18f,
                    "KHÔNG PHẢI SÁCH! Đây là Vách Tường/Mặt Trơn. Hãy hướng camera vào Sách giáo trình.");
            }

            bool hasBookFeatures = (f.highFreqTextLines >= 1 && f.brightPaperRatio >= 0.30f)
                || (f.totalScanTransitions >= 25 && f.brightPaperRatio >= 0.30f)
                || (f.highFreqTextLines >= 2 && f.microEdgeFraction >= 0.040f)
                || (f.richTexturePatchCount >= 6 && f.microEdgeFraction >= 0.045f);

            if (hasBookFeatures)
            {
                float conf = Mathf.Clamp(85f + (f.highFreqTextLines * 3f) + (f.richTexturePatchCount * 1.0f), 85f, 98f);
                return MakeResult(targetPropIndex, true, conf, "ĐÃ KHÓA MỤC TIÊU: SÁCH GIÁO TRÌNH",
                    $"Dòng chữ in: {f.highFreqTextLines} dòng, Giấy: {f.brightPaperRatio * 100f:F0}%");
            }

            return MakeResult(targetPropIndex, false, 22f,
                "KHÔNG PHẢI SÁCH! Hãy đưa camera lại gần trang sách có chữ in rõ nét.");
        }
    }
}
