using System;
using NUnit.Framework;
using UnityEngine;
using IUHCampus.Guardian.AR;

namespace IUHCampus.Editor
{
    public static class IUHARVisionTests
    {
        [Test]
        public static void Test_Laptop_ScreenShowingWebOrDocument_DetectedAsLaptop_NotBook()
        {
            // Tình huống: Người chơi quét màn hình Laptop đang mở trang web / tài liệu / code màu sáng
            var f = new VisionFeatures
            {
                globalContrast = 85f,
                avgPatchStdDev = 7.5f,
                maxPatchStdDev = 16.0f,
                richTexturePatchCount = 5,
                brightPaperRatio = 0.55f, // Trình duyệt/tài liệu nền sáng
                darkPixelsRatio = 0.12f,  // Viền bezel máy tính / thanh tiêu đề / dock
                avgColorSat = 0.15f,
                microEdgeFraction = 0.042f,
                highFreqTextLines = 3,    // Chữ in trên trang web
                mediumFreqKeyLines = 0,
                totalScanTransitions = 32
            };

            var result = IUHARVisionAnalyzer.Analyze(f, targetPropIndex: 1);

            Assert.IsTrue(result.isSuccess, "Màn hình laptop hiển thị tài liệu/web phải được nhận diện thành công là LAPTOP!");
            Assert.GreaterOrEqual(result.confidence, 85f);
            StringAssert.Contains("LAPTOP", result.message);
            StringAssert.DoesNotContain("KHÔNG PHẢI LAPTOP", result.message);
            StringAssert.DoesNotContain("Sách", result.message);
            Debug.Log($"[PASS] Test_Laptop_ScreenShowingWebOrDocument: {result.message} ({result.confidence:F0}%)");
        }

        [Test]
        public static void Test_Laptop_Keyboard_DetectedAsLaptop()
        {
            // Tình huống: Người chơi quét bàn phím Laptop
            var f = new VisionFeatures
            {
                globalContrast = 110f,
                avgPatchStdDev = 9.2f,
                maxPatchStdDev = 22.0f,
                richTexturePatchCount = 8,
                brightPaperRatio = 0.05f,
                darkPixelsRatio = 0.25f, // Bàn phím tối màu
                avgColorSat = 0.08f,
                microEdgeFraction = 0.065f,
                highFreqTextLines = 0,
                mediumFreqKeyLines = 3,   // Các hàng phím bấm chu kỳ
                totalScanTransitions = 28
            };

            var result = IUHARVisionAnalyzer.Analyze(f, targetPropIndex: 1);

            Assert.IsTrue(result.isSuccess, "Bàn phím laptop phải được nhận diện thành công là LAPTOP!");
            Assert.GreaterOrEqual(result.confidence, 88f);
            StringAssert.Contains("LAPTOP", result.message);
            Debug.Log($"[PASS] Test_Laptop_Keyboard: {result.message} ({result.confidence:F0}%)");
        }

        [Test]
        public static void Test_Laptop_DarkThemeIDE_DetectedAsLaptop()
        {
            // Tình huống: Màn hình Laptop mở VS Code / IDE nền tối
            var f = new VisionFeatures
            {
                globalContrast = 135f,
                avgPatchStdDev = 8.0f,
                maxPatchStdDev = 18.5f,
                richTexturePatchCount = 7,
                brightPaperRatio = 0.02f,
                darkPixelsRatio = 0.58f, // Nền tối
                avgColorSat = 0.22f,     // Syntax highlighting
                microEdgeFraction = 0.048f,
                highFreqTextLines = 4,
                mediumFreqKeyLines = 0,
                totalScanTransitions = 40
            };

            var result = IUHARVisionAnalyzer.Analyze(f, targetPropIndex: 1);

            Assert.IsTrue(result.isSuccess, "Màn hình laptop nền tối phải được nhận diện thành công là LAPTOP!");
            StringAssert.Contains("LAPTOP", result.message);
            Debug.Log($"[PASS] Test_Laptop_DarkThemeIDE: {result.message} ({result.confidence:F0}%)");
        }

        [Test]
        public static void Test_PhysicalBook_DetectedAsBook_WhenTargetIsBook()
        {
            // Tình huống: Quét sách giáo trình thật khi đang chọn Tab Sách (targetPropIndex = 2)
            var f = new VisionFeatures
            {
                globalContrast = 72f,
                avgPatchStdDev = 6.8f,
                maxPatchStdDev = 15.0f,
                richTexturePatchCount = 5,
                brightPaperRatio = 0.70f, // Trang sách giấy trắng
                darkPixelsRatio = 0.04f,  // Chỉ có mực in, không có viền bezel máy tính
                avgColorSat = 0.04f,
                microEdgeFraction = 0.045f,
                highFreqTextLines = 3,    // Dòng chữ in
                mediumFreqKeyLines = 0,
                totalScanTransitions = 36
            };

            var result = IUHARVisionAnalyzer.Analyze(f, targetPropIndex: 2);

            Assert.IsTrue(result.isSuccess, "Sách giáo trình thật phải được nhận diện khi chọn Tab Sách!");
            Assert.GreaterOrEqual(result.confidence, 85f);
            StringAssert.Contains("SÁCH GIÁO TRÌNH", result.message);
            Debug.Log($"[PASS] Test_PhysicalBook_DetectedAsBook: {result.message} ({result.confidence:F0}%)");
        }

        [Test]
        public static void Test_PhysicalBook_RejectedAsLaptop_WhenTargetIsLaptop()
        {
            // Tình huống: Người chơi đưa cuốn sách giấy trắng vào khi đang chọn Tab Laptop
            var f = new VisionFeatures
            {
                globalContrast = 60f,
                avgPatchStdDev = 5.5f,
                maxPatchStdDev = 13.0f,
                richTexturePatchCount = 3,
                brightPaperRatio = 0.82f, // Giấy trắng chiếm > 75%
                darkPixelsRatio = 0.03f,  // Hoàn toàn không có viền bezel hay bàn phím
                avgColorSat = 0.03f,
                microEdgeFraction = 0.038f,
                highFreqTextLines = 2,
                mediumFreqKeyLines = 0,
                totalScanTransitions = 25
            };

            var result = IUHARVisionAnalyzer.Analyze(f, targetPropIndex: 1);

            Assert.IsFalse(result.isSuccess, "Sách giấy trắng không có viền bezel/bàn phím phải bị từ chối khi đang chọn Tab Laptop!");
            StringAssert.Contains("KHÔNG PHẢI LAPTOP", result.message);
            Debug.Log($"[PASS] Test_PhysicalBook_RejectedAsLaptop: {result.message}");
        }

        [Test]
        public static void Test_Backpack_DetectedAsBackpack()
        {
            // Tình huống: Quét Balo vải đen/xám có quai đeo và khối 3D (targetPropIndex = 0)
            var f = new VisionFeatures
            {
                globalContrast = 75f,
                avgPatchStdDev = 5.2f,
                maxPatchStdDev = 14.5f,
                richTexturePatchCount = 3,
                brightPaperRatio = 0.06f,
                darkPixelsRatio = 0.38f, // Vải balo tối màu
                avgColorSat = 0.12f,
                microEdgeFraction = 0.040f,
                highFreqTextLines = 0,
                mediumFreqKeyLines = 0,
                totalScanTransitions = 8
            };

            var result = IUHARVisionAnalyzer.Analyze(f, targetPropIndex: 0);

            Assert.IsTrue(result.isSuccess, "Balo thật phải được nhận diện khi chọn Tab Balo!");
            Assert.GreaterOrEqual(result.confidence, 86f);
            StringAssert.Contains("BALO THẬT", result.message);
            Debug.Log($"[PASS] Test_Backpack_DetectedAsBackpack: {result.message} ({result.confidence:F0}%)");
        }

        [Test]
        public static void Test_PlainWall_RejectedForAllTargets()
        {
            // Tình huống: Quét vách tường sơn trơn nhẵn phẳng
            var f = new VisionFeatures
            {
                globalContrast = 22f,     // Tương phản rất thấp
                avgPatchStdDev = 2.4f,    // Độ nhẵn cao
                maxPatchStdDev = 7.5f,
                richTexturePatchCount = 0,
                brightPaperRatio = 0.10f,
                darkPixelsRatio = 0.02f,
                avgColorSat = 0.05f,
                microEdgeFraction = 0.012f,
                highFreqTextLines = 0,
                mediumFreqKeyLines = 0,
                totalScanTransitions = 1
            };

            for (int target = 0; target <= 2; target++)
            {
                var result = IUHARVisionAnalyzer.Analyze(f, target);
                Assert.IsFalse(result.isSuccess, $"Vách tường phải bị từ chối cho mục tiêu {target}!");
                StringAssert.Contains("Vách Tường", result.message);
            }
            Debug.Log("[PASS] Test_PlainWall_RejectedForAllTargets: Đã chặn thành công vách tường trên cả 3 mục tiêu.");
        }

        [Test]
        public static void Test_SynthesizedPixels_LaptopScreenWithBezel()
        {
            // Tạo ảnh tổng hợp 128x128 mô phỏng màn hình laptop: viền đen (bezel) xung quanh và nội dung sáng bên trong
            int w = 128;
            int h = 128;
            Color32[] pixels = new Color32[w * h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool isBezel = (x < 12 || x > w - 12 || y < 12 || y > h - 12);
                    if (isBezel)
                    {
                        pixels[y * w + x] = new Color32(25, 25, 25, 255); // Viền đen bezel
                    }
                    else
                    {
                        // Nội dung màn hình: nền sáng và các dòng chữ/cửa sổ
                        bool isTextLine = (y % 14 >= 5 && y % 14 <= 7) && (x % 8 >= 2 && x % 8 <= 5);
                        if (isTextLine)
                            pixels[y * w + x] = new Color32(30, 30, 40, 255); // Chữ tối
                        else
                            pixels[y * w + x] = new Color32(230, 235, 240, 255); // Nền sáng
                    }
                }
            }

            VisionFeatures f = IUHARVisionAnalyzer.ExtractFeatures(pixels, w, h);
            var result = IUHARVisionAnalyzer.Analyze(f, targetPropIndex: 1);

            Assert.IsTrue(result.isSuccess, "Màn hình laptop tổng hợp có viền bezel và nội dung phải được nhận diện là LAPTOP!");
            StringAssert.Contains("LAPTOP", result.message);
            Debug.Log($"[PASS] Test_SynthesizedPixels_LaptopScreenWithBezel: {result.message} ({result.confidence:F0}%)");
        }

        /// <summary>
        /// Phương thức thực thi toàn bộ test từ Unity CommandLine (batchmode)
        /// </summary>
        public static void RunTestsFromCommandLine()
        {
            int pass = 0;
            int fail = 0;

            Action<string, Action> run = (name, test) =>
            {
                try
                {
                    test();
                    pass++;
                    Debug.Log($"[UNIT TEST PASSED] {name}");
                }
                catch (Exception ex)
                {
                    fail++;
                    Debug.LogError($"[UNIT TEST FAILED] {name}: {ex.Message}");
                }
            };

            Debug.Log("==================================================");
            Debug.Log("BAT DAU CHAY UNIT TESTS NHAN DIEN THI GIAC AR (IUHARVisionTests)");
            Debug.Log("==================================================");

            run("Test_Laptop_ScreenShowingWebOrDocument_DetectedAsLaptop_NotBook", Test_Laptop_ScreenShowingWebOrDocument_DetectedAsLaptop_NotBook);
            run("Test_Laptop_Keyboard_DetectedAsLaptop", Test_Laptop_Keyboard_DetectedAsLaptop);
            run("Test_Laptop_DarkThemeIDE_DetectedAsLaptop", Test_Laptop_DarkThemeIDE_DetectedAsLaptop);
            run("Test_PhysicalBook_DetectedAsBook_WhenTargetIsBook", Test_PhysicalBook_DetectedAsBook_WhenTargetIsBook);
            run("Test_PhysicalBook_RejectedAsLaptop_WhenTargetIsLaptop", Test_PhysicalBook_RejectedAsLaptop_WhenTargetIsLaptop);
            run("Test_Backpack_DetectedAsBackpack", Test_Backpack_DetectedAsBackpack);
            run("Test_PlainWall_RejectedForAllTargets", Test_PlainWall_RejectedForAllTargets);
            run("Test_SynthesizedPixels_LaptopScreenWithBezel", Test_SynthesizedPixels_LaptopScreenWithBezel);

            Debug.Log("==================================================");
            Debug.Log($"KET QUA: {pass} PASSED, {fail} FAILED");
            Debug.Log("==================================================");

            if (fail > 0)
            {
                UnityEditor.EditorApplication.Exit(1);
            }
        }
    }
}
