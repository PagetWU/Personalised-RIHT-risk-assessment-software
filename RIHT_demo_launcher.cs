using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public class RihtDemoLauncher : Form
{
    private readonly TextBox caseDirBox = new TextBox();
    private readonly TextBox ageBox = new TextBox();
    private readonly TextBox genderBox = new TextBox();
    private readonly TextBox nStageBox = new TextBox();
    private readonly TextBox ctWindowLevelBox = new TextBox();
    private readonly TextBox ctWindowWidthBox = new TextBox();
    private readonly TextBox hotspotThresholdBox = new TextBox();
    private readonly TextBox outDirBox = new TextBox();
    private readonly TextBox logBox = new TextBox();
    private readonly Button runButton = new Button();
    private readonly Button openReportButton = new Button();
    private string lastReportPath = "";

    private static readonly string PythonExe = @"C:\Users\Jiaming\.conda\envs\python\python.exe";

    public RihtDemoLauncher()
    {
        Text = "RIHT Single-Patient Demo";
        Width = 880;
        Height = 620;
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9F);

        var baseDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
        var defaultOut = Path.Combine(baseDir, "outputs", "case_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));

        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(14),
            ColumnCount = 3,
            RowCount = 11
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));

        AddRow(panel, 0, "Case folder", caseDirBox, BrowseCase);
        AddRow(panel, 1, "Output folder", outDirBox, BrowseOutput);
        AddSimpleRow(panel, 2, "Age", ageBox);
        AddSimpleRow(panel, 3, "Gender", genderBox);
        AddSimpleRow(panel, 4, "N-stage", nStageBox);
        AddSimpleRow(panel, 5, "CT WL", ctWindowLevelBox);
        AddSimpleRow(panel, 6, "CT WW", ctWindowWidthBox);
        AddSimpleRow(panel, 7, "Hotspot Gy", hotspotThresholdBox);

        outDirBox.Text = defaultOut;
        genderBox.Text = "Male";
        nStageBox.Text = "2";
        ctWindowLevelBox.Text = "50";
        ctWindowWidthBox.Text = "400";
        hotspotThresholdBox.Text = "40";

        runButton.Text = "Run";
        runButton.Height = 34;
        runButton.Click += async (sender, args) => await RunDemoAsync();

        openReportButton.Text = "Open report";
        openReportButton.Height = 34;
        openReportButton.Enabled = false;
        openReportButton.Click += (sender, args) => OpenReport();

        var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        buttonPanel.Controls.Add(runButton);
        buttonPanel.Controls.Add(openReportButton);
        panel.Controls.Add(new Label(), 0, 8);
        panel.Controls.Add(buttonPanel, 1, 8);
        panel.SetColumnSpan(buttonPanel, 2);

        logBox.Multiline = true;
        logBox.ScrollBars = ScrollBars.Vertical;
        logBox.ReadOnly = true;
        logBox.Font = new Font("Consolas", 9F);
        logBox.Dock = DockStyle.Fill;
        panel.Controls.Add(new Label { Text = "Log", TextAlign = ContentAlignment.TopLeft, Dock = DockStyle.Fill }, 0, 9);
        panel.Controls.Add(logBox, 1, 9);
        panel.SetColumnSpan(logBox, 2);
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        Controls.Add(panel);
    }

    private static void AddRow(TableLayoutPanel panel, int row, string label, TextBox box, EventHandler browseHandler)
    {
        panel.Controls.Add(new Label { Text = label, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, row);
        box.Dock = DockStyle.Fill;
        panel.Controls.Add(box, 1, row);
        var button = new Button { Text = "Browse...", Dock = DockStyle.Fill };
        button.Click += browseHandler;
        panel.Controls.Add(button, 2, row);
    }

    private static void AddSimpleRow(TableLayoutPanel panel, int row, string label, TextBox box)
    {
        panel.Controls.Add(new Label { Text = label, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, row);
        box.Dock = DockStyle.Left;
        box.Width = 160;
        panel.Controls.Add(box, 1, row);
        panel.SetColumnSpan(box, 2);
    }

    private void BrowseCase(object sender, EventArgs args)
    {
        using (var dialog = new FolderBrowserDialog())
        {
            dialog.Description = "Select case folder containing CT, dose, thyroid mask, and optional target masks.";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                caseDirBox.Text = dialog.SelectedPath;
                if (string.IsNullOrWhiteSpace(outDirBox.Text))
                {
                    outDirBox.Text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "outputs", Path.GetFileName(dialog.SelectedPath));
                }
            }
        }
    }

    private void BrowseOutput(object sender, EventArgs args)
    {
        using (var dialog = new FolderBrowserDialog())
        {
            dialog.Description = "Select output folder.";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                outDirBox.Text = dialog.SelectedPath;
            }
        }
    }

    private async Task RunDemoAsync()
    {
        if (!File.Exists(PythonExe))
        {
            MessageBox.Show("Python executable not found:\n" + PythonExe, "RIHT Demo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        if (!Directory.Exists(caseDirBox.Text))
        {
            MessageBox.Show("Case folder does not exist.", "RIHT Demo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(ageBox.Text) || string.IsNullOrWhiteSpace(nStageBox.Text))
        {
            MessageBox.Show("Please enter Age and N-stage.", "RIHT Demo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(genderBox.Text))
        {
            genderBox.Text = "Unknown";
        }
        if (string.IsNullOrWhiteSpace(ctWindowLevelBox.Text))
        {
            ctWindowLevelBox.Text = "50";
        }
        if (string.IsNullOrWhiteSpace(ctWindowWidthBox.Text))
        {
            ctWindowWidthBox.Text = "400";
        }
        if (string.IsNullOrWhiteSpace(hotspotThresholdBox.Text))
        {
            hotspotThresholdBox.Text = "40";
        }

        Directory.CreateDirectory(outDirBox.Text);
        logBox.Clear();
        openReportButton.Enabled = false;
        runButton.Enabled = false;
        lastReportPath = Path.Combine(outDirBox.Text, "RIHT_demo_report.html");

        var args = "-m riht_demo.cli predict"
            + " --case-dir " + Quote(caseDirBox.Text)
            + " --age " + Quote(ageBox.Text)
            + " --gender " + Quote(genderBox.Text)
            + " --n-stage " + Quote(nStageBox.Text)
            + " --ct-window-level " + Quote(ctWindowLevelBox.Text)
            + " --ct-window-width " + Quote(ctWindowWidthBox.Text)
            + " --hotspot-threshold-gy " + Quote(hotspotThresholdBox.Text)
            + " --out-dir " + Quote(outDirBox.Text);

        AppendLog("> " + PythonExe + " " + args + Environment.NewLine);
        var psi = new ProcessStartInfo
        {
            FileName = PythonExe,
            Arguments = args,
            WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        await Task.Run(() =>
        {
            using (var proc = new Process { StartInfo = psi })
            {
                proc.OutputDataReceived += (s, e) => { if (e.Data != null) AppendLog(e.Data + Environment.NewLine); };
                proc.ErrorDataReceived += (s, e) => { if (e.Data != null) AppendLog(e.Data + Environment.NewLine); };
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();
                AppendLog(Environment.NewLine + "Exit code: " + proc.ExitCode + Environment.NewLine);
            }
        });

        runButton.Enabled = true;
        openReportButton.Enabled = File.Exists(lastReportPath);
        if (File.Exists(lastReportPath))
        {
            OpenReport();
        }
    }

    private static string Quote(string value)
    {
        return "\"" + value.Replace("\"", "\\\"") + "\"";
    }

    private void AppendLog(string text)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action<string>(AppendLog), text);
            return;
        }
        logBox.AppendText(text);
    }

    private void OpenReport()
    {
        if (!File.Exists(lastReportPath))
        {
            MessageBox.Show("No report found yet.", "RIHT Demo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        Process.Start(new ProcessStartInfo { FileName = lastReportPath, UseShellExecute = true });
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new RihtDemoLauncher());
    }
}
