using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JsonToPngConverter
{
    public class MainForm : Form
    {
        private TextBox txtJsonPath = null!;
        private TextBox txtOutputPath = null!;
        private Button btnBrowseJson = null!;
        private Button btnBrowseOutput = null!;
        private Button btnDownload = null!;
        private Button btnConvert = null!;
        private Button btnConvertPdf = null!;
        private Button btnDownloadAndConvert = null!;
        private TextBox txtLog = null!;
        private NumericUpDown numConcurrency = null!;
        private NumericUpDown numWidth = null!;
        private NumericUpDown numHeight = null!;
        private NumericUpDown numScale = null!;
        private CheckBox chkFullPage = null!;
        private ProgressBar progressBar = null!;
        private Label lblStatus = null!;

        public MainForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "LanhuToPng - 蓝湖原型转PNG工具";
            this.Size = new System.Drawing.Size(800, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int y = 20;
            int labelWidth = 100;
            int controlX = labelWidth + 20;

            // JSON 文件路径
            var lblJson = new Label
            {
                Text = "JSON 文件:",
                Location = new System.Drawing.Point(20, y),
                Size = new System.Drawing.Size(labelWidth, 20)
            };
            this.Controls.Add(lblJson);

            txtJsonPath = new TextBox
            {
                Location = new System.Drawing.Point(controlX, y),
                Size = new System.Drawing.Size(500, 25),
                PlaceholderText = "选择蓝湖 JSON 文件..."
            };
            this.Controls.Add(txtJsonPath);

            btnBrowseJson = new Button
            {
                Text = "浏览...",
                Location = new System.Drawing.Point(controlX + 510, y),
                Size = new System.Drawing.Size(80, 25)
            };
            btnBrowseJson.Click += BtnBrowseJson_Click;
            this.Controls.Add(btnBrowseJson);

            y += 40;

            // 输出目录
            var lblOutput = new Label
            {
                Text = "输出目录:",
                Location = new System.Drawing.Point(20, y),
                Size = new System.Drawing.Size(labelWidth, 20)
            };
            this.Controls.Add(lblOutput);

            txtOutputPath = new TextBox
            {
                Location = new System.Drawing.Point(controlX, y),
                Size = new System.Drawing.Size(500, 25),
                PlaceholderText = "选择输出目录..."
            };
            this.Controls.Add(txtOutputPath);

            btnBrowseOutput = new Button
            {
                Text = "浏览...",
                Location = new System.Drawing.Point(controlX + 510, y),
                Size = new System.Drawing.Size(80, 25)
            };
            btnBrowseOutput.Click += BtnBrowseOutput_Click;
            this.Controls.Add(btnBrowseOutput);

            y += 50;

            // 配置选项组
            var grpConfig = new GroupBox
            {
                Text = "配置选项",
                Location = new System.Drawing.Point(20, y),
                Size = new System.Drawing.Size(750, 100)
            };
            this.Controls.Add(grpConfig);

            // 并发数
            var lblConcurrency = new Label
            {
                Text = "并发数:",
                Location = new System.Drawing.Point(20, 30),
                Size = new System.Drawing.Size(80, 20)
            };
            grpConfig.Controls.Add(lblConcurrency);

            numConcurrency = new NumericUpDown
            {
                Location = new System.Drawing.Point(100, 28),
                Size = new System.Drawing.Size(80, 25),
                Minimum = 1,
                Maximum = 10,
                Value = 3
            };
            grpConfig.Controls.Add(numConcurrency);

            // 截图宽度
            var lblWidth = new Label
            {
                Text = "宽度:",
                Location = new System.Drawing.Point(200, 30),
                Size = new System.Drawing.Size(60, 20)
            };
            grpConfig.Controls.Add(lblWidth);

            numWidth = new NumericUpDown
            {
                Location = new System.Drawing.Point(260, 28),
                Size = new System.Drawing.Size(80, 25),
                Minimum = 800,
                Maximum = 3840,
                Value = 1920,
                Increment = 100
            };
            grpConfig.Controls.Add(numWidth);

            // 截图高度
            var lblHeight = new Label
            {
                Text = "高度:",
                Location = new System.Drawing.Point(360, 30),
                Size = new System.Drawing.Size(60, 20)
            };
            grpConfig.Controls.Add(lblHeight);

            numHeight = new NumericUpDown
            {
                Location = new System.Drawing.Point(420, 28),
                Size = new System.Drawing.Size(80, 25),
                Minimum = 600,
                Maximum = 2160,
                Value = 1080,
                Increment = 100
            };
            grpConfig.Controls.Add(numHeight);

            // 缩放因子
            var lblScale = new Label
            {
                Text = "缩放:",
                Location = new System.Drawing.Point(520, 30),
                Size = new System.Drawing.Size(60, 20)
            };
            grpConfig.Controls.Add(lblScale);

            numScale = new NumericUpDown
            {
                Location = new System.Drawing.Point(580, 28),
                Size = new System.Drawing.Size(80, 25),
                Minimum = 1,
                Maximum = 4,
                Value = 2,
                DecimalPlaces = 1,
                Increment = 0.5m
            };
            grpConfig.Controls.Add(numScale);

            // 全页截图
            chkFullPage = new CheckBox
            {
                Text = "截取整个页面",
                Location = new System.Drawing.Point(20, 65),
                Size = new System.Drawing.Size(150, 20),
                Checked = true
            };
            grpConfig.Controls.Add(chkFullPage);

            y += 120;

            // 操作按钮
            btnDownload = new Button
            {
                Text = "1. 下载 HTML",
                Location = new System.Drawing.Point(20, y),
                Size = new System.Drawing.Size(175, 40),
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDownload.Click += BtnDownload_Click;
            this.Controls.Add(btnDownload);

            btnConvert = new Button
            {
                Text = "2. 转换 PNG",
                Location = new System.Drawing.Point(205, y),
                Size = new System.Drawing.Size(175, 40),
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnConvert.Click += BtnConvert_Click;
            this.Controls.Add(btnConvert);

            btnConvertPdf = new Button
            {
                Text = "3. 转换 PDF",
                Location = new System.Drawing.Point(390, y),
                Size = new System.Drawing.Size(175, 40),
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnConvertPdf.Click += BtnConvertPdf_Click;
            this.Controls.Add(btnConvertPdf);

            btnDownloadAndConvert = new Button
            {
                Text = "一键完成",
                Location = new System.Drawing.Point(575, y),
                Size = new System.Drawing.Size(195, 40),
                BackColor = System.Drawing.Color.FromArgb(16, 124, 16),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Microsoft YaHei UI", 10, System.Drawing.FontStyle.Bold)
            };
            btnDownloadAndConvert.Click += BtnDownloadAndConvert_Click;
            this.Controls.Add(btnDownloadAndConvert);

            y += 60;

            // 进度条
            progressBar = new ProgressBar
            {
                Location = new System.Drawing.Point(20, y),
                Size = new System.Drawing.Size(750, 25),
                Style = ProgressBarStyle.Continuous
            };
            this.Controls.Add(progressBar);

            y += 35;

            // 状态标签
            lblStatus = new Label
            {
                Text = "就绪",
                Location = new System.Drawing.Point(20, y),
                Size = new System.Drawing.Size(750, 20),
                ForeColor = System.Drawing.Color.Green
            };
            this.Controls.Add(lblStatus);

            y += 30;

            // 日志输出
            var lblLog = new Label
            {
                Text = "执行日志:",
                Location = new System.Drawing.Point(20, y),
                Size = new System.Drawing.Size(100, 20)
            };
            this.Controls.Add(lblLog);

            y += 25;

            txtLog = new TextBox
            {
                Location = new System.Drawing.Point(20, y),
                Size = new System.Drawing.Size(750, 200),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                BackColor = System.Drawing.Color.Black,
                ForeColor = System.Drawing.Color.LightGreen,
                Font = new System.Drawing.Font("Consolas", 9)
            };
            this.Controls.Add(txtLog);

            // JSON 示例提示
            var lblExample = new Label
            {
                Text = "JSON 示例: JsonData\\11.json (包含 mapping_md5 字段的蓝湖导出文件)",
                Location = new System.Drawing.Point(20, y + 210),
                Size = new System.Drawing.Size(750, 20),
                ForeColor = System.Drawing.Color.Gray,
                Font = new System.Drawing.Font("Microsoft YaHei UI", 8)
            };
            this.Controls.Add(lblExample);
        }

        private void BtnBrowseJson_Click(object? sender, EventArgs e)
        {
            try
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Filter = "JSON 文件|*.json|所有文件|*.*";
                    dialog.Title = "选择蓝湖 JSON 文件";
                    dialog.CheckFileExists = true;
                    
                    // 设置初始目录
                    if (!string.IsNullOrEmpty(txtJsonPath.Text) && File.Exists(txtJsonPath.Text))
                    {
                        dialog.InitialDirectory = Path.GetDirectoryName(txtJsonPath.Text);
                        dialog.FileName = Path.GetFileName(txtJsonPath.Text);
                    }
                    
                    var result = dialog.ShowDialog(this);
                    if (result == DialogResult.OK)
                    {
                        txtJsonPath.Text = dialog.FileName;
                        Log($"已选择 JSON: {dialog.FileName}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"选择文件时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBrowseOutput_Click(object? sender, EventArgs e)
        {
            try
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    dialog.Description = "选择输出目录";
                    dialog.ShowNewFolderButton = true;
                    
                    // 设置初始目录
                    if (!string.IsNullOrEmpty(txtOutputPath.Text) && Directory.Exists(txtOutputPath.Text))
                    {
                        dialog.SelectedPath = txtOutputPath.Text;
                    }
                    
                    var result = dialog.ShowDialog(this);
                    if (result == DialogResult.OK && !string.IsNullOrEmpty(dialog.SelectedPath))
                    {
                        txtOutputPath.Text = dialog.SelectedPath;
                        Log($"已选择输出目录: {dialog.SelectedPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"选择目录时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnDownload_Click(object? sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            var htmlPath = Path.Combine(txtOutputPath.Text, "html");
            await ExecuteAsync("download", txtJsonPath.Text, htmlPath);
        }

        private async void BtnConvert_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtOutputPath.Text))
            {
                MessageBox.Show("请选择输出目录", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var htmlPath = Path.Combine(txtOutputPath.Text, "html");
            var pngPath = Path.Combine(txtOutputPath.Text, "png");

            if (!Directory.Exists(htmlPath))
            {
                MessageBox.Show($"HTML 目录不存在: {htmlPath}\n请先执行下载操作", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            await ExecuteAsync("convert", htmlPath, pngPath);
        }

        private async void BtnConvertPdf_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtOutputPath.Text))
            {
                MessageBox.Show("请选择输出目录", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var pngPath = Path.Combine(txtOutputPath.Text, "png");

            if (!Directory.Exists(pngPath))
            {
                MessageBox.Show($"PNG 目录不存在: {pngPath}\n请先执行 PNG 转换操作", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var pdfPath = Path.Combine(txtOutputPath.Text, "原型截图.pdf");
            await ExecuteAsync("pdf", pngPath, pdfPath);
        }

        private async void BtnDownloadAndConvert_Click(object? sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            var htmlPath = Path.Combine(txtOutputPath.Text, "html");
            var pngPath = Path.Combine(txtOutputPath.Text, "png");
            var pdfPath = Path.Combine(txtOutputPath.Text, "原型截图.pdf");

            // 下载
            var downloadSuccess = await ExecuteAsync("download", txtJsonPath.Text, htmlPath);
            if (!downloadSuccess) return;

            // 转换 PNG
            var convertSuccess = await ExecuteAsync("convert", htmlPath, pngPath);
            if (!convertSuccess) return;

            // 转换 PDF
            await ExecuteAsync("pdf", pngPath, pdfPath);
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtJsonPath.Text))
            {
                MessageBox.Show("请选择 JSON 文件", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!File.Exists(txtJsonPath.Text))
            {
                MessageBox.Show("JSON 文件不存在", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtOutputPath.Text))
            {
                MessageBox.Show("请选择输出目录", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private async Task<bool> ExecuteAsync(string mode, string input, string output)
        {
            SetControlsEnabled(false);
            progressBar.Style = ProgressBarStyle.Marquee;
            
            try
            {
                var config = new ConverterConfig
                {
                    InputPath = input,
                    OutputPath = output,
                    DownloadMode = mode == "download",
                    Concurrency = (int)numConcurrency.Value,
                    Width = (int)numWidth.Value,
                    Height = (int)numHeight.Value,
                    Scale = (double)numScale.Value,
                    FullPage = chkFullPage.Checked,
                    Timeout = 30000
                };

                if (mode == "download")
                {
                    UpdateStatus("正在下载 HTML...", System.Drawing.Color.Blue);
                    Log("========================================");
                    Log("开始下载 HTML");
                    Log("========================================");

                    var downloader = new LanhuDownloader(config);
                    downloader.OnProgress += (msg) => Log(msg);
                    
                    await Task.Run(() => downloader.Download());
                    
                    UpdateStatus("下载完成！", System.Drawing.Color.Green);
                    Log($"✓ HTML 文件已保存到: {output}");
                    MessageBox.Show($"下载完成！\n\nHTML 文件保存在:\n{output}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (mode == "convert")
                {
                    UpdateStatus("正在转换 PNG...", System.Drawing.Color.Blue);
                    Log("========================================");
                    Log("开始转换 PNG");
                    Log("========================================");

                    var converter = new HtmlToImageConverter(config);
                    converter.OnProgress += (msg) => Log(msg);
                    
                    await converter.ConvertAsync();
                    
                    UpdateStatus("转换完成！", System.Drawing.Color.Green);
                    Log($"✓ PNG 文件已保存到: {output}");
                    MessageBox.Show($"转换完成！\n\nPNG 文件保存在:\n{output}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (mode == "pdf")
                {
                    UpdateStatus("正在生成 PDF...", System.Drawing.Color.Blue);
                    Log("========================================");
                    Log("开始生成 PDF");
                    Log("========================================");

                    var pdfConverter = new PdfConverter(input, output);
                    pdfConverter.OnProgress += (msg) => Log(msg);
                    
                    await Task.Run(() => pdfConverter.Convert());
                    
                    UpdateStatus("PDF 生成完成！", System.Drawing.Color.Green);
                    Log($"✓ PDF 文件已保存到: {output}");
                    MessageBox.Show($"PDF 生成完成！\n\nPDF 文件保存在:\n{output}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                return true;
            }
            catch (Exception ex)
            {
                UpdateStatus($"错误: {ex.Message}", System.Drawing.Color.Red);
                Log($"✗ 错误: {ex.Message}");
                MessageBox.Show($"操作失败:\n\n{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                progressBar.Style = ProgressBarStyle.Continuous;
                SetControlsEnabled(true);
            }
        }

        private void SetControlsEnabled(bool enabled)
        {
            if (InvokeRequired)
            {
                // 使用 BeginInvoke 而不是 Invoke，避免阻塞
                BeginInvoke(new Action(() => SetControlsEnabled(enabled)));
                return;
            }

            btnBrowseJson.Enabled = enabled;
            btnBrowseOutput.Enabled = enabled;
            btnDownload.Enabled = enabled;
            btnConvert.Enabled = enabled;
            btnConvertPdf.Enabled = enabled;
            btnDownloadAndConvert.Enabled = enabled;
            numConcurrency.Enabled = enabled;
            numWidth.Enabled = enabled;
            numHeight.Enabled = enabled;
            numScale.Enabled = enabled;
            chkFullPage.Enabled = enabled;
        }

        private void UpdateStatus(string message, System.Drawing.Color color)
        {
            if (InvokeRequired)
            {
                // 使用 BeginInvoke 而不是 Invoke，避免阻塞
                BeginInvoke(new Action(() => UpdateStatus(message, color)));
                return;
            }

            lblStatus.Text = message;
            lblStatus.ForeColor = color;
        }

        private void Log(string message)
        {
            if (InvokeRequired)
            {
                // 使用 BeginInvoke 而不是 Invoke，避免阻塞
                BeginInvoke(new Action(() => Log(message)));
                return;
            }

            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }
    }
}
