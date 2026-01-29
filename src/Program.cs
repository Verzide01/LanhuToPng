using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JsonToPngConverter
{
    class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            // 如果有命令行参数，使用 CLI 模式
            if (args.Length > 0)
            {
                Console.WriteLine("╔════════════════════════════════════════╗");
                Console.WriteLine("║   JSON/HTML to PNG Converter v1.0     ║");
                Console.WriteLine("║   Powered by PuppeteerSharp           ║");
                Console.WriteLine("╚════════════════════════════════════════╝");
                Console.WriteLine();

                return SimpleCli.RunAsync(args).GetAwaiter().GetResult();
            }
            else
            {
                // 否则启动 GUI
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.Run(new MainForm());
                return 0;
            }
        }
    }
}
