using System;
using System.IO;
using System.Linq;
using PdfSharp.Pdf;
using PdfSharp.Drawing;

namespace JsonToPngConverter
{
    /// <summary>
    /// PNG 转 PDF 转换器
    /// </summary>
    public class PdfConverter
    {
        private readonly string _inputDirectory;
        private readonly string _outputPath;

        public event Action<string>? OnProgress;

        public PdfConverter(string inputDirectory, string outputPath)
        {
            _inputDirectory = inputDirectory;
            _outputPath = outputPath;
        }

        public void Convert()
        {
            Log("========================================");
            Log("   PNG to PDF Converter");
            Log("========================================");
            Log("");

            Log($"输入目录: {_inputDirectory}");
            Log($"输出文件: {_outputPath}");
            Log("");

            try
            {
                // 扫描 PNG 文件
                var pngFiles = Directory.GetFiles(_inputDirectory, "*.png", SearchOption.TopDirectoryOnly)
                    .OrderBy(f => f)
                    .ToArray();

                if (pngFiles.Length == 0)
                {
                    Log("错误: 未找到 PNG 文件");
                    return;
                }

                Log($"找到 {pngFiles.Length} 个 PNG 文件");
                Log("");

                // 创建 PDF 文档
                using (var document = new PdfDocument())
                {
                    document.Info.Title = "Lanhu Prototype Screenshots";
                    document.Info.Creator = "LanhuToPng";
                    document.Info.CreationDate = DateTime.Now;

                    int count = 0;
                    foreach (var pngFile in pngFiles)
                    {
                        count++;
                        var fileName = Path.GetFileNameWithoutExtension(pngFile);
                        
                        try
                        {
                            // 读取图片
                            using (var image = XImage.FromFile(pngFile))
                            {
                                // 创建页面，使用图片的实际尺寸
                                var page = document.AddPage();
                                page.Width = XUnit.FromPoint(image.PixelWidth * 0.75).Point;
                                page.Height = XUnit.FromPoint(image.PixelHeight * 0.75).Point;

                                // 绘制图片
                                using (var gfx = XGraphics.FromPdfPage(page))
                                {
                                    gfx.DrawImage(image, 0, 0, page.Width, page.Height);
                                }

                                Log($"[{count}/{pngFiles.Length}] ✓ {fileName}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log($"[{count}/{pngFiles.Length}] ✗ {fileName} - {ex.Message}");
                        }
                    }

                    // 保存 PDF
                    var outputDir = Path.GetDirectoryName(_outputPath);
                    if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                    {
                        Directory.CreateDirectory(outputDir);
                    }

                    document.Save(_outputPath);
                }

                Log("");
                Log($"✓ PDF 已生成: {_outputPath}");
                Log($"总页数: {pngFiles.Length}");
            }
            catch (Exception ex)
            {
                Log($"错误: {ex.Message}");
                throw;
            }
        }

        private void Log(string message)
        {
            Console.WriteLine(message);
            OnProgress?.Invoke(message);
        }
    }
}
