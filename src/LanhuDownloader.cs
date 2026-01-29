using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;

namespace JsonToPngConverter
{
    /// <summary>
    /// 蓝湖下载器 - 基于 lanhuapp.exe 的逻辑
    /// 从 JSON 自动下载完整的 HTML 包
    /// </summary>
    public class LanhuDownloader
    {
        private readonly string _jsonPath;
        private readonly string _outputDir;
        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _semaphore;
        private const string BASE_URL = "https://axure-file.lanhuapp.com/";
        private string _projectName = string.Empty;
        private const string IMAGE_FOLDER = "images";
        private static readonly object _fileLock = new object();

        public event Action<string>? OnProgress;

        public LanhuDownloader(ConverterConfig config)
            : this(config.InputPath, config.OutputPath, config.Concurrency)
        {
        }

        public LanhuDownloader(string jsonPath, string outputDir, int maxConcurrency = 5)
        {
            _jsonPath = jsonPath;
            _outputDir = outputDir;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
            _semaphore = new SemaphoreSlim(maxConcurrency);
        }

        public void Download()
        {
            DownloadAsync().GetAwaiter().GetResult();
        }

        public async Task DownloadAsync()
        {
            Log("========================================");
            Log("   Lanhu Downloader");
            Log("   Auto download from Lanhu API");
            Log("========================================");
            Log("");

            Log($"JSON: {_jsonPath}");
            Log($"Output: {_outputDir}");
            Log("");

            try
            {
                // 1. 读取 JSON
                var jsonContent = File.ReadAllText(_jsonPath, Encoding.UTF8);
                var jsonObj = JObject.Parse(jsonContent);
                var pagesObj = jsonObj["pages"] as JObject;

                if (pagesObj == null || pagesObj.Count == 0)
                {
                    Log("ERROR: No pages found in JSON");
                    return;
                }

                _projectName = Path.GetFileNameWithoutExtension(_jsonPath);
                Directory.CreateDirectory(_outputDir);

                Log($"✓ Found {pagesObj.Count} pages");
                Log("");

                // 2. 下载所有页面
                await DownloadPagesAsync(pagesObj);

                // 3. 解压 override.zip
                await ExtractOverrideAsync();

                Log("");
                Log("✓ Download complete!");
                Log("");
                Log($"Files saved to: {_outputDir}");
            }
            catch (Exception ex)
            {
                Log($"ERROR: {ex.Message}");
                throw;
            }
        }

        private void Log(string message)
        {
            Console.WriteLine(message);
            OnProgress?.Invoke(message);
        }

        private async Task DownloadPagesAsync(JObject pagesObj)
        {
            var tasks = new List<Task>();
            var successCount = 0;
            var failedCount = 0;
            var currentCount = 0;
            var totalCount = pagesObj.Count;

            foreach (var page in pagesObj)
            {
                var pageName = page.Key.Replace(".html", "");
                var pageData = page.Value as JObject;

                if (pageData == null)
                    continue;

                var task = Task.Run(async () =>
                {
                    await _semaphore.WaitAsync();
                    try
                    {
                        var success = await DownloadPageAsync(pageName, pageData);

                        Interlocked.Increment(ref currentCount);

                        if (success)
                        {
                            Interlocked.Increment(ref successCount);
                            Log($"[{currentCount}/{totalCount}] ✓ {pageName}");
                        }
                        else
                        {
                            Interlocked.Increment(ref failedCount);
                            Log($"[{currentCount}/{totalCount}] ✗ {pageName}");
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
            Log($"Success: {successCount}, Failed: {failedCount}, Total: {totalCount}");
        }

        private async Task<bool> DownloadPageAsync(string pageName, JObject pageData)
        {
            try
            {
                // 获取 mapping_md5
                var mappingMd5 = pageData["mapping_md5"]?.ToString();
                if (string.IsNullOrEmpty(mappingMd5))
                    return false;

                // 下载页面配置
                var configUrl = BASE_URL + mappingMd5;
                var configJson = await DownloadStringAsync(configUrl);
                if (string.IsNullOrEmpty(configJson))
                    return false;

                var config = JObject.Parse(configJson);

                // 下载 HTML
                var htmlSignMd5 = config["html"]?["sign_md5"]?.ToString();
                var htmlSrc = config["html"]?["src"]?.ToString();
                if (!string.IsNullOrEmpty(htmlSignMd5) && !string.IsNullOrEmpty(htmlSrc))
                {
                    await SaveFileAsync(BASE_URL + htmlSignMd5, Path.Combine(_outputDir, htmlSrc));
                }

                // 下载 data.js
                var dataJsSignMd5 = config["dataJs"]?["sign_md5"]?.ToString();
                var dataJsSrc = config["dataJs"]?["src"]?.ToString();
                if (!string.IsNullOrEmpty(dataJsSignMd5) && !string.IsNullOrEmpty(dataJsSrc))
                {
                    await SaveFileAsync(BASE_URL + dataJsSignMd5, Path.Combine(_outputDir, dataJsSrc));
                }

                // 下载 styles
                var stylesObj = config["styles"] as JObject;
                if (stylesObj != null)
                {
                    foreach (var style in stylesObj)
                    {
                        var signMd5 = style.Value["sign_md5"]?.ToString();
                        if (!string.IsNullOrEmpty(signMd5))
                        {
                            await SaveFileAsync(BASE_URL + signMd5, Path.Combine(_outputDir, style.Key));
                        }
                    }
                }

                // 下载 scripts
                var scriptsObj = config["scripts"] as JObject;
                if (scriptsObj != null)
                {
                    foreach (var script in scriptsObj)
                    {
                        var signMd5 = script.Value["sign_md5"]?.ToString();
                        if (!string.IsNullOrEmpty(signMd5))
                        {
                            var url = signMd5.StartsWith("http") ? signMd5 : BASE_URL + signMd5;
                            await SaveFileAsync(url, Path.Combine(_outputDir, script.Key));
                        }
                    }
                }

                // 下载 images
                var imagesObj = config["images"] as JObject;
                if (imagesObj != null)
                {
                    foreach (var image in imagesObj)
                    {
                        var signMd5 = image.Value["sign_md5"]?.ToString();
                        if (!string.IsNullOrEmpty(signMd5))
                        {
                            await SaveFileAsync(BASE_URL + signMd5, Path.Combine(_outputDir, image.Key));
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log($"Error downloading {pageName}: {ex.Message}");
                return false;
            }
        }

        private async Task SaveFileAsync(string url, string fileFullName)
        {
            var dir = Path.GetDirectoryName(fileFullName);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            // 如果文件已存在，跳过下载（避免并发冲突）
            if (File.Exists(fileFullName))
            {
                return;
            }

            var ext = Path.GetExtension(fileFullName).ToLower();
            var imageExts = new[] { ".gif", ".png", ".jpg", ".jpeg", ".bmp" };

            lock (_fileLock)
            {
                // 双重检查：再次确认文件是否存在
                if (File.Exists(fileFullName))
                {
                    return;
                }

                if (imageExts.Contains(ext))
                {
                    // 下载图片
                    var bytes = _httpClient.GetByteArrayAsync(url).Result;
                    File.WriteAllBytes(fileFullName, bytes);
                }
                else
                {
                    // 下载文本文件
                    var content = DownloadStringAsync(url).Result;

                    if (ext == ".html")
                    {
                        content = HttpUtility.HtmlDecode(content);
                        content = ProcessHtmlContent(content);
                    }
                    else if (ext == ".js" && Path.GetFileName(fileFullName) == "data.js")
                    {
                        content = ProcessDataJs(content);
                    }
                    else if (ext == ".css")
                    {
                        content = ProcessCssContent(content, fileFullName).Result;
                    }

                    File.WriteAllText(fileFullName, content, Encoding.UTF8);
                }
            }
        }

        private string ProcessHtmlContent(string html)
        {
            return html
                .Replace("rel=\"stylesheet\" data-src=\"", "rel=\"stylesheet\" href=\"")
                .Replace("<script data-src=\"", "<script src=\"")
                .Replace("<body style=\"display: none; opacity: 0;\">", "<body>")
                .Replace("data-src=\"images/", "src=\"images/");
        }

        private string ProcessDataJs(string js)
        {
            if (js.StartsWith("lanhu_Axure_Mapping_Data(") && js.EndsWith(")"))
            {
                js = js.Substring("lanhu_Axure_Mapping_Data(".Length);
                js = js.Substring(0, js.Length - 1);
            }
            return js;
        }

        private async Task<string> ProcessCssContent(string css, string cssFilePath)
        {
            var imgUrls = PickupImgUrl(css);
            if (imgUrls == null || imgUrls.Count == 0)
                return css;

            var fileName = Path.GetFileName(Path.GetDirectoryName(cssFilePath));
            var imageDir = Path.Combine(_outputDir, IMAGE_FOLDER, fileName);

            if (!Directory.Exists(imageDir))
            {
                Directory.CreateDirectory(imageDir);
            }

            foreach (var imgUrl in imgUrls)
            {
                var cleanUrl = imgUrl.StartsWith("./") ? imgUrl.Substring(2) : imgUrl;
                var imagePath = Path.Combine(imageDir, cleanUrl);

                try
                {
                    await SaveFileAsync(BASE_URL + cleanUrl, imagePath);
                    css = css.Replace(imgUrl, $"../../{IMAGE_FOLDER}/{fileName}/{cleanUrl}");
                }
                catch
                {
                    // Ignore image download errors
                }
            }

            return css;
        }

        private List<string> PickupImgUrl(string css)
        {
            var regex = new Regex(@"background-image\b[^);]*?\b:url[\s\t\r\n]*\([\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*\);", RegexOptions.IgnoreCase);
            var matches = regex.Matches(css);
            var urls = new List<string>();

            foreach (Match match in matches)
            {
                urls.Add(match.Groups["imgUrl"].Value);
            }

            return urls;
        }

        private async Task<string> DownloadStringAsync(string url)
        {
            try
            {
                return await _httpClient.GetStringAsync(url);
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task ExtractOverrideAsync()
        {
            var overrideZip = "override.zip";
            if (!File.Exists(overrideZip))
            {
                Log("Warning: override.zip not found, skipping extraction");
                return;
            }

            Log("");
            Log("Extracting override.zip...");

            try
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(overrideZip, _outputDir, true);
                Log("✓ Override extracted");
            }
            catch (Exception ex)
            {
                Log($"Warning: Failed to extract override.zip: {ex.Message}");
            }
        }
    }
}
