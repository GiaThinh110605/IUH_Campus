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

            // 1. Phân tích 6x6 lưới cục bộ
            int gridCols = 6;
            int gridRows = 6;
            int cellW = regionW / gridCols;
            int cellH = regionH / gridRows;

            float patchStdDevSum = 0f;
            float maxStd = 0f;
            int richPatches = 0;
            int totalPatches = gridCols * gridRows;

            float minLuma = 255f;
            float maxLuma = 0f;
            int brightPaperCount = 0;
            int darkCount = 0;
            int totalSamples = 0;
            float sumSat = 0f;

            for (int gy = 0; gy < gridRows; gy++)
            {
                for (int gx = 0; gx < gridCols; gx++)
                {
                    int cx0 = startX + gx * cellW;
                    int cy0 = startY + gy * cellH;

                    float cellSumLuma = 0f;
                    int cellSamples = 0;
                    int stepX = Mathf.Max(2, cellW / 6);
                    int stepY = Mathf.Max(2, cellH / 6);

                    List<float> cellLumas = new List<float>(36);

                    for (int py = cy0; py < cy0 + cellH; py += stepY)
                    {
                        for (int px = cx0; px < cx0 + cellW; px += stepX)
                        {
                            int idx = py * w + px;
                            if (idx >= 0 && idx < pixels.Length)
                            {
                                Color32 c = pixels[idx];
                                float lum = 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
                                cellSumLuma += lum;
                                cellLumas.Add(lum);
                                cellSamples++;

                                if (lum < minLuma) minLuma = lum;
                                if (lum > maxLuma) maxLuma = lum;

                                totalSamples++;
                                if (lum > 155f && Mathf.Abs(c.r - c.g) < 28 && Mathf.Abs(c.g - c.b) < 28)
                                    brightPaperCount++;
                                if (lum < 75f)
                                    darkCount++;

                                float maxC = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
                                float minC = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
                                float sat = maxC > 0 ? (maxC - minC) / maxC : 0f;
                                sumSat += sat;
                            }
                        }
                    }

                    if (cellSamples >= 8)
                    {
                        float mean = cellSumLuma / cellSamples;
                        float variance = 0f;
                        for (int i = 0; i < cellLumas.Count; i++)
                        {
                            float d = cellLumas[i] - mean;
                            variance += d * d;
                        }
                        float std = Mathf.Sqrt(variance / cellSamples);
                        patchStdDevSum += std;
                        if (std > maxStd) maxStd = std;
                        if (std >= 11.5f) richPatches++;
                    }
                }
            }

            f.globalContrast = maxLuma - minLuma;
            f.avgPatchStdDev = totalPatches > 0 ? (patchStdDevSum / totalPatches) : 0f;
            f.maxPatchStdDev = maxStd;
            f.richTexturePatchCount = richPatches;
            f.brightPaperRatio = totalSamples > 0 ? ((float)brightPaperCount / totalSamples) : 0f;
            f.darkPixelsRatio = totalSamples > 0 ? ((float)darkCount / totalSamples) : 0f;
            f.avgColorSat = totalSamples > 0 ? (sumSat / totalSamples) : 0f;

            // 2. Vi cạnh vi mô (Micro-edges)
            int totalEdgeSamples = 0;
            int sharpEdges = 0;
            int edgeStep = Mathf.Max(3, regionW / 50);

            for (int y = startY; y < endY - 2; y += edgeStep)
            {
                for (int x = startX; x < endX - 2; x += edgeStep)
                {
                    int idxC = y * w + x;
                    int idxR = y * w + (x + 2);
                    int idxU = (y + 2) * w + x;

                    if (idxU < pixels.Length && idxR < pixels.Length)
                    {
                        Color32 cC = pixels[idxC];
                        Color32 cR = pixels[idxR];
                        Color32 cU = pixels[idxU];

                        float lumC = 0.299f * cC.r + 0.587f * cC.g + 0.114f * cC.b;
                        float lumR = 0.299f * cR.r + 0.587f * cR.g + 0.114f * cR.b;
                        float lumU = 0.299f * cU.r + 0.587f * cU.g + 0.114f * cU.b;

                        float grad = (Mathf.Abs(lumR - lumC) + Mathf.Abs(lumU - lumC)) * 0.5f;
                        if (grad >= 20.0f) sharpEdges++;
                        totalEdgeSamples++;
                    }
                }
            }
            f.microEdgeFraction = totalEdgeSamples > 0 ? ((float)sharpEdges / totalEdgeSamples) : 0f;

            // 3. Quét phổ tần số dao động (Scan lines)
            int lineScanCount = 7;
            int lineStepY = Mathf.Max(2, regionH / (lineScanCount + 1));
            int scanStepX = Mathf.Max(2, regionW / 50);

            int highLines = 0;
            int medLines = 0;
            int totalTrans = 0;

            for (int l = 1; l <= lineScanCount; l++)
            {
                int ly = startY + l * lineStepY;
                if (ly >= endY) break;

                int transitions = 0;
                float prevVal = -1f;
                int prevSign = 0;

                for (int lx = startX; lx < endX; lx += scanStepX)
                {
                    int pIdx = ly * w + lx;
                    if (pIdx >= 0 && pIdx < pixels.Length)
                    {
                        Color32 pc = pixels[pIdx];
                        float curVal = 0.299f * pc.r + 0.587f * pc.g + 0.114f * pc.b;
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
                }

                totalTrans += transitions;
                if (transitions >= 12) highLines++;
                else if (transitions >= 4 && transitions <= 11) medLines++;
            }

            f.highFreqTextLines = highLines;
            f.mediumFreqKeyLines = medLines;
            f.totalScanTransitions = totalTrans;

            return f;
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
            var res = new VisionAnalysisResult();
            res.targetPropIndex = targetPropIndex;

            bool isWall = IsPlainWall(f);

            // =====================================================================
            // TAB 1: LAPTOP / MÁY TÍNH
            // =====================================================================
            if (targetPropIndex == 1)
            {
                // A. Chặn tường phẳng / mặt trơn
                if (isWall || (f.richTexturePatchCount < 2 && f.microEdgeFraction < 0.030f && f.globalContrast < 45f))
                {
                    res.isSuccess = false;
                    res.confidence = 18f;
                    res.message = "KHÔNG PHẢI LAPTOP! Đây là Vách Tường/Mặt Trơn. Hãy hướng camera vào Laptop thật.";
                    res.details = $"Tương phản: {f.globalContrast:F0}, Ô chi tiết: {f.richTexturePatchCount}/36";
                    return res;
                }

                // B. Chỉ chặn nếu là SÁCH GIẤY THẬT (toàn trang giấy trắng chiếm > 75%, không có viền máy tính / bàn phím / dock / bezel)
                bool isPurePhysicalPaperBook = (f.brightPaperRatio >= 0.75f && f.darkPixelsRatio < 0.05f && f.mediumFreqKeyLines == 0 && f.richTexturePatchCount <= 4);
                if (isPurePhysicalPaperBook)
                {
                    res.isSuccess = false;
                    res.confidence = 25f;
                    res.message = "KHÔNG PHẢI LAPTOP! Đây là Sách / Trang Giấy Trắng. Hãy hướng camera vào Màn hình & Bàn phím Laptop.";
                    res.details = $"Tỉ lệ giấy trắng cao ({f.brightPaperRatio * 100f:F0}%), không thấy viền bezel/bàn phím";
                    return res;
                }

                // C. Đặc trưng Laptop thật:
                // 1) Màn hình Laptop (kể cả khi hiển thị trang web, code, tài liệu văn bản, video):
                //    Có độ tương phản màn hình (globalContrast >= 45f), có chi tiết UI/nội dung (richTexturePatchCount >= 3 hoặc microEdgeFraction >= 0.032f),
                //    hoặc có viền màn hình bezel / thanh tiêu đề (darkPixelsRatio >= 0.06f).
                bool hasLaptopScreen = (f.globalContrast >= 45f && (f.richTexturePatchCount >= 2 || f.microEdgeFraction >= 0.030f) && f.maxPatchStdDev >= 11.0f);

                // 2) Bàn phím Laptop:
                //    Có các hàng phím bấm chu kỳ (mediumFreqKeyLines >= 1) kết hợp với các ô phím (richTexturePatchCount >= 2 hoặc darkPixelsRatio >= 0.08f).
                bool hasLaptopKeyboard = (f.mediumFreqKeyLines >= 1 && (f.richTexturePatchCount >= 2 || f.darkPixelsRatio >= 0.08f))
                                      || (f.totalScanTransitions >= 15 && f.darkPixelsRatio >= 0.08f);

                if (hasLaptopScreen || hasLaptopKeyboard)
                {
                    res.isSuccess = true;
                    float baseConf = hasLaptopKeyboard ? 88f : 86f;
                    res.confidence = Mathf.Clamp(baseConf + (f.mediumFreqKeyLines * 2.5f) + (f.darkPixelsRatio * 15f) + (f.richTexturePatchCount * 0.8f), 86f, 98f);
                    res.message = "ĐÃ KHÓA MỤC TIÊU: LAPTOP / MÁY TÍNH THẬT";
                    res.details = $"Màn hình/Bàn phím: {f.mediumFreqKeyLines} hàng phím, Viền/Vùng tối: {f.darkPixelsRatio * 100f:F0}%, Vi cạnh: {f.microEdgeFraction * 100f:F1}%";
                    return res;
                }
                else
                {
                    res.isSuccess = false;
                    res.confidence = 25f;
                    res.message = "KHÔNG PHẢI LAPTOP! Hãy đưa camera lại gần Bàn phím & Màn hình máy tính.";
                    res.details = $"Chưa thấy rõ khung màn hình hoặc bàn phím";
                    return res;
                }
            }
            // =====================================================================
            // TAB 0: BALO / TÚI XÁCH
            // =====================================================================
            else if (targetPropIndex == 0)
            {
                if (isWall)
                {
                    res.isSuccess = false;
                    res.confidence = 18f;
                    res.message = "KHÔNG PHẢI BALO! Đây là Vách Tường/Mặt Trơn. Hãy hướng camera vào Balo của bạn.";
                    res.details = $"Độ nhẵn: {f.avgPatchStdDev:F1}, Tương phản: {f.globalContrast:F0}";
                    return res;
                }

                if (f.brightPaperRatio > 0.50f && f.darkPixelsRatio < 0.20f)
                {
                    res.isSuccess = false;
                    res.confidence = 22f;
                    res.message = "KHÔNG PHẢI BALO! Đây là Sách / Trang Giấy Trắng. Hãy hướng camera vào Balo của bạn.";
                    return res;
                }

                bool hasBackpackBody = (f.globalContrast >= 45f && f.maxPatchStdDev >= 11.0f)
                    || (f.richTexturePatchCount >= 2)
                    || (f.darkPixelsRatio >= 0.15f && f.avgPatchStdDev >= 4.2f);

                if (hasBackpackBody)
                {
                    res.isSuccess = true;
                    res.confidence = Mathf.Clamp(86f + (f.maxPatchStdDev - 11f) * 0.5f + f.darkPixelsRatio * 12f, 86f, 98f);
                    res.message = "ĐÃ KHÓA MỤC TIÊU: BALO THẬT";
                    res.details = $"Vải/Quai đeo: {f.maxPatchStdDev:F1}, Vùng vải tối: {f.darkPixelsRatio * 100f:F0}%, Độ nổi khối: {f.globalContrast:F0}";
                    return res;
                }
                else
                {
                    res.isSuccess = false;
                    res.confidence = 20f;
                    res.message = "KHÔNG PHẢI BALO! Hãy đưa camera lại gần Balo để bắt rõ quai đeo hoặc nếp gấp vải.";
                    return res;
                }
            }
            // =====================================================================
            // TAB 2: SÁCH GIÁO TRÌNH
            // =====================================================================
            else
            {
                if (isWall || f.richTexturePatchCount < 2)
                {
                    res.isSuccess = false;
                    res.confidence = 18f;
                    res.message = "KHÔNG PHẢI SÁCH! Đây là Vách Tường/Mặt Trơn. Hãy hướng camera vào Sách giáo trình.";
                    return res;
                }

                // Sách thật: các dòng chữ in trang sách (highFreqTextLines >= 1 hoặc transitions >= 20) trên nền giấy sáng
                bool hasBookFeatures = (f.highFreqTextLines >= 1 && f.brightPaperRatio >= 0.30f)
                    || (f.totalScanTransitions >= 25 && f.brightPaperRatio >= 0.30f)
                    || (f.highFreqTextLines >= 2 && f.microEdgeFraction >= 0.040f)
                    || (f.richTexturePatchCount >= 6 && f.microEdgeFraction >= 0.045f);

                if (hasBookFeatures)
                {
                    res.isSuccess = true;
                    res.confidence = Mathf.Clamp(85f + (f.highFreqTextLines * 3f) + (f.richTexturePatchCount * 1.0f), 85f, 98f);
                    res.message = "ĐÃ KHÓA MỤC TIÊU: SÁCH GIÁO TRÌNH";
                    res.details = $"Dòng chữ in: {f.highFreqTextLines} dòng, Giấy: {f.brightPaperRatio * 100f:F0}%";
                    return res;
                }
                else
                {
                    res.isSuccess = false;
                    res.confidence = 22f;
                    res.message = "KHÔNG PHẢI SÁCH! Hãy đưa camera lại gần trang sách có chữ in rõ nét.";
                    return res;
                }
            }
        }
    }
}
