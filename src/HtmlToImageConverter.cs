using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace JsonToPngConverter
{
    /// <summary>
    /// 直接将现有 HTML 文件转换为 PNG 的转换器
    /// 适用于已经生成好 HTML 的场景
    /// </summary>
    public class HtmlToImageConverter
    {
        private readonly string _inputDirectory;
        private readonly string _outputDirectory;
        private readonly ConverterConfig _config;
        private IBrowser? _browser;
        private readonly SemaphoreSlim _semaphore;

        public event Action<string>? OnProgress;

        public HtmlToImageConverter(ConverterConfig config)
            : this(config.InputPath, config.OutputPath, config)
        {
        }

        public HtmlToImageConverter(string inputDirectory, string outputDirectory, ConverterConfig? config = null)
        {
            _inputDirectory = inputDirectory;
            _outputDirectory = outputDirectory;
            _config = config ?? new ConverterConfig();
            _semaphore = new SemaphoreSlim(_config.MaxConcurrency);
        }

        public async Task ConvertAsync()
        {
            await ConvertAllAsync();
        }

        public async Task ConvertAllAsync()
        {
            Log("=== HTML to PNG Converter ===");
            Log($"输入目录: {_inputDirectory}");
            Log($"输出目录: {_outputDirectory}");
            Log("");

            try
            {
                // 初始化浏览器
                await InitializeBrowserAsync();

                // 扫描 HTML 文件
                var allHtmlFiles = Directory.GetFiles(_inputDirectory, "*.html", SearchOption.TopDirectoryOnly);
                
                // 过滤掉导航页面（index, start 等）
                var excludeFiles = new[] { "index.html", "start.html", "start_c_1.html", "start_with_pages.html" };
                var htmlFiles = allHtmlFiles
                    .Where(f => !excludeFiles.Contains(Path.GetFileName(f).ToLower()))
                    .ToArray();
                
                Log($"找到 {htmlFiles.Length} 个 HTML 文件（已过滤 {allHtmlFiles.Length - htmlFiles.Length} 个导航页面）");
                Log("");

                // 创建输出目录
                Directory.CreateDirectory(_outputDirectory);

                // 批量转换
                await ConvertFilesAsync(htmlFiles);

                Log("");
                Log("✓ 转换完成！");
            }
            finally
            {
                await CleanupAsync();
            }
        }

        private async Task InitializeBrowserAsync()
        {
            Log("正在初始化浏览器...");

            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();

            _browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[]
                {
                    "--no-sandbox",
                    "--disable-setuid-sandbox",
                    "--disable-dev-shm-usage",
                    "--disable-accelerated-2d-canvas",
                    "--disable-gpu",
                    "--disable-web-security",
                    "--allow-file-access-from-files",
                    "--disable-features=IsolateOrigins,site-per-process",
                    "--no-referrers"
                }
            });

            Log("✓ 浏览器初始化完成");
            Log("");
        }

        private void Log(string message)
        {
            Console.WriteLine(message);
            OnProgress?.Invoke(message);
        }

        private async Task ConvertFilesAsync(string[] htmlFiles)
        {
            var tasks = new List<Task>();
            var successCount = 0;
            var failedCount = 0;
            var totalCount = htmlFiles.Length;
            var currentCount = 0;

            foreach (var htmlFile in htmlFiles)
            {
                var task = Task.Run(async () =>
                {
                    await _semaphore.WaitAsync();
                    try
                    {
                        var fileName = Path.GetFileNameWithoutExtension(htmlFile);
                        var success = await ConvertFileWithRetryAsync(htmlFile, fileName);

                        Interlocked.Increment(ref currentCount);

                        if (success)
                        {
                            Interlocked.Increment(ref successCount);
                            Log($"[{currentCount}/{totalCount}] ✓ {fileName}");
                        }
                        else
                        {
                            Interlocked.Increment(ref failedCount);
                            Log($"[{currentCount}/{totalCount}] ✗ {fileName}");
                        }
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            Log("");
            Log($"成功: {successCount}, 失败: {failedCount}, 总计: {totalCount}");
        }

        private async Task<bool> ConvertFileWithRetryAsync(string htmlPath, string fileName)
        {
            for (int attempt = 1; attempt <= _config.RetryCount; attempt++)
            {
                try
                {
                    await ConvertFileAsync(htmlPath, fileName);
                    return true;
                }
                catch (Exception ex)
                {
                    if (attempt == _config.RetryCount)
                    {
                        Log($"  警告: {fileName} 转换失败 - {ex.Message}");
                        return false;
                    }

                    await Task.Delay(_config.RetryDelay);
                }
            }

            return false;
        }

        private async Task ConvertFileAsync(string htmlPath, string fileName)
        {
            if (_browser == null)
                throw new InvalidOperationException("浏览器未初始化");

            var page = await _browser.NewPageAsync();

            try
            {
                // 设置视口
                await page.SetViewportAsync(new ViewPortOptions
                {
                    Width = _config.Width,
                    Height = _config.Height,
                    DeviceScaleFactor = _config.DeviceScaleFactor
                });

                // 加载 HTML 文件 - 使用 file:// 协议以保持相对路径资源可访问
                var fileUri = new Uri(Path.GetFullPath(htmlPath)).AbsoluteUri;
                
                // 使用旧版本 API，应该不会有 referrerPolicy 问题
                await page.GoToAsync(fileUri, new NavigationOptions
                {
                    WaitUntil = new[] { WaitUntilNavigation.Networkidle2 },
                    Timeout = _config.Timeout
                });

                // 截图
                var outputPath = Path.Combine(_outputDirectory, $"{fileName}.png");
                await page.ScreenshotAsync(outputPath, new ScreenshotOptions
                {
                    FullPage = _config.FullPage,
                    Type = ScreenshotType.Png
                });
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        private async Task CleanupAsync()
        {
            if (_browser != null)
            {
                await _browser.CloseAsync();
                _browser = null;
            }
        }
    }
}
