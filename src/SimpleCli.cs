using System;
using System.IO;
using System.Threading.Tasks;

namespace JsonToPngConverter
{
    /// <summary>
    /// 简化的命令行接口 - 支持 JSON 和 HTML 输入
    /// </summary>
    public class SimpleCli
    {
        public static async Task<int> RunAsync(string[] args)
        {
            if (args.Length < 2)
            {
                ShowUsage();
                return 1;
            }

            var input = args[0];
            var outputDir = args[1];

            // 判断输入类型：JSON 文件还是 HTML 目录
            bool isJsonInput = File.Exists(input) && input.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
            bool isHtmlDir = Directory.Exists(input);

            if (!isJsonInput && !isHtmlDir)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"错误: 输入必须是 JSON 文件或包含 HTML 的目录");
                Console.WriteLine($"输入: {input}");
                Console.ResetColor();
                return 1;
            }

            try
            {
                // 检查是否是下载模式
                bool downloadMode = HasArg(args, "--download");

                // 如果是 JSON 且要求下载
                if (isJsonInput && downloadMode)
                {
                    Console.WriteLine("模式: JSON → Download HTML from Lanhu");
                    Console.WriteLine();

                    var downloader = new LanhuDownloader(input, outputDir, GetIntArg(args, "--concurrency", 5));
                    await downloader.DownloadAsync();
                    return 0;
                }

                // JSON 输入但没有 --download 标志，提示用户
                if (isJsonInput)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("提示: JSON 文件需要先下载 HTML。");
                    Console.WriteLine("请使用 --download 参数：");
                    Console.WriteLine($"  {System.AppDomain.CurrentDomain.FriendlyName} \"{input}\" \"{outputDir}\" --download");
                    Console.ResetColor();
                    return 1;
                }

                // HTML → PNG
                var config = new ConverterConfig
                {
                    Width = GetIntArg(args, "--width", 1920),
                    Height = GetIntArg(args, "--height", 1080),
                    Concurrency = GetIntArg(args, "--concurrency", 5),
                    FullPage = !HasArg(args, "--no-full-page"),
                    Scale = GetIntArg(args, "--scale", 2),
                    Timeout = GetIntArg(args, "--timeout", 30000)
                };

                Console.WriteLine("模式: HTML → PNG");
                Console.WriteLine();
                
                var converter = new HtmlToImageConverter(input, outputDir, config);
                await converter.ConvertAllAsync();

                return 0;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"错误: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
                return 1;
            }
        }

        private static void ShowUsage()
        {
            Console.WriteLine("用法: JsonToPngConverter <输入> <输出目录> [选项]");
            Console.WriteLine();
            Console.WriteLine("输入可以是:");
            Console.WriteLine("  - JSON 文件路径 (例如: JsonData/11.json)");
            Console.WriteLine("  - HTML 目录路径 (例如: PageData/11/v1.3.3)");
            Console.WriteLine();
            Console.WriteLine("示例:");
            Console.WriteLine("  # 从蓝湖下载 HTML（真实内容）");
            Console.WriteLine("  JsonToPngConverter \"JsonData/11.json\" \"PageData/11/v1.4.0\" --download");
            Console.WriteLine();
            Console.WriteLine("  # 从 HTML 目录生成 PNG");
            Console.WriteLine("  JsonToPngConverter \"PageData/11/v1.3.3\" \"Output/PNG\"");
            Console.WriteLine();
            Console.WriteLine("选项:");
            Console.WriteLine("  --download             从蓝湖服务器下载真实 HTML 内容");
            Console.WriteLine("  --width <像素>         截图宽度（默认: 1920）");
            Console.WriteLine("  --height <像素>        截图高度（默认: 1080）");
            Console.WriteLine("  --concurrency <数量>   并发数量（默认: 5）");
            Console.WriteLine("  --scale <倍数>         设备缩放因子（默认: 2）");
            Console.WriteLine("  --timeout <毫秒>       超时时间（默认: 30000）");
            Console.WriteLine("  --no-full-page         不截取整个页面");
            Console.WriteLine();
        }

        private static int GetIntArg(string[] args, string name, int defaultValue)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(args[i + 1], out int value))
                        return value;
                }
            }
            return defaultValue;
        }

        private static bool HasArg(string[] args, string name)
        {
            foreach (var arg in args)
            {
                if (arg.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
