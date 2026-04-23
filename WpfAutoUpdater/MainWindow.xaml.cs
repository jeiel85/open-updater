using System.Diagnostics;
using System.IO;
using System.Text;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using AutoUpdate.Shared;
using AutoUpdate.Shared.Services;
using Microsoft.Win32;

namespace WpfAutoUpdater;

public partial class MainWindow : Window
{
    private string _programPath = "POPs_Renewal";
    private string _targetExeName = "POPs_Renewal.exe";
    private string _configPath;
    private string _tempPath;
    private string _backupPath;
    private bool _isStop = false;
    private bool _ftpConnected;
    private List<string> _fileList = new();
    private FtpDownloadService? _ftpService;
    private CancellationTokenSource? _downloadCts = new();
    private DispatcherTimer? _timer;

    // 설정값
    private string _ftpServer = "update.wavepos.co.kr";
    private string _ftpUser = "devel";
    private string _ftpPass = "dev1234";
    private string _ftpPort = "";

    public MainWindow()
    {
        InitializeComponent();

        _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UpdatePath.ini");
        _tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UpdateTemp");
        _backupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BACKUP");

        // 로그 폴더 설정
        Logger.LogFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");

        LoadSettings();
        Loaded += MainWindow_Load;
    }

    private void LoadSettings()
    {
        try
        {
            _programPath = IniFile.Read("업데이트정보", "업데이트경로", _configPath, "POPs_Renewal");
            _targetExeName = IniFile.Read("업데이트정보", "실행파일명", _configPath, "POPs_Renewal.exe");
            _ftpServer = IniFile.Read("업데이트정보", "FTP서버", _configPath, _ftpServer);
            _ftpUser = IniFile.Read("업데이트정보", "FTP사용자", _configPath, _ftpUser);
            _ftpPass = IniFile.Read("업데이트정보", "FTP암호", _configPath, _ftpPass);
            _ftpPort = IniFile.Read("업데이트정보", "FTP포트", _configPath, "");

            Logger.WriteLog("AUTOUPDATE", "MAIN", "LoadSettings", "설정", $"경로={_programPath}, 서버={_ftpServer}, 실행파일={_targetExeName}");
        }
        catch (Exception ex)
        {
            Logger.Error("LoadSettings", ex);
        }
    }

    private void MainWindow_Load(object sender, RoutedEventArgs e)
    {
        lblProgramName.Text = _programPath;
        lblAutoRunTarget.Text = $"(실행 파일: {_targetExeName})";
        lblVersion.Text = $"버전: {Assembly.GetExecutingAssembly().GetName().Version}";
        lblModifyDate.Text = DateTime.Now.ToString("yyyy-MM-dd");

        // 타이머로 현재 시간 표시
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (s, ev) => { lblModifyDate.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm"); };
        _timer.Start();

        // 셀프 업데이트 체크
        Dispatcher.BeginInvoke(async () => await CheckSelfUpdateAsync());

        // 자동 실행 모드 체크
        var args = Environment.GetCommandLineArgs();
        if (args.Length > 1 && args[1] == "1")
        {
            Dispatcher.BeginInvoke(async () => await StartUpdateAsync());
        }
    }

    private async Task CheckSelfUpdateAsync()
    {
        try
        {
            var ghService = new GitHubDownloadService("jeiel85", "open-updater");
            var release = await ghService.GetLatestReleaseInfoAsync();

            if (release == null) return;

            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            if (Version.TryParse(release.TagName.Replace("v", ""), out var latestVersion))
            {
                if (latestVersion > currentVersion)
                {
                    if (MessageBox.Show($"새로운 버전({release.TagName})이 발견되었습니다.\n지금 업데이트하시겠습니까?", 
                        "셀프 업데이트", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                    {
                        var asset = release.Assets.FirstOrDefault(a => a.Name == "OpenUpdater.exe");
                        if (asset != null)
                        {
                            var tempFile = Path.Combine(Path.GetTempPath(), "OpenUpdater_New.exe");
                            var result = await ghService.DownloadFileAsync(tempFile, asset.BrowserDownloadUrl);

                            if (result.Success)
                            {
                                ApplySelfUpdate(tempFile);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error("CheckSelfUpdate", ex);
        }
    }

    private void ApplySelfUpdate(string newExePath)
    {
        try
        {
            var currentExe = Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrEmpty(currentExe)) return;

            var batchPath = Path.Combine(Path.GetTempPath(), "update_swap.bat");
            var script = $@"
@echo off
timeout /t 2 /nobreak > nul
move /y ""{newExePath}"" ""{currentExe}""
start """" ""{currentExe}""
del ""%~f0""
";
            File.WriteAllText(batchPath, script, Encoding.Default);

            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{batchPath}\"",
                CreateNoWindow = true,
                UseShellExecute = false
            });

            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            Logger.Error("ApplySelfUpdate", ex);
            MessageBox.Show($"업데이트 적용 중 오류가 발생했습니다: {ex.Message}");
        }
    }

    private async Task StartUpdateAsync()
    {
        if (!_ftpConnected)
        {
            lblFileName.Text = "FTP 서버 접속 중...";
            _ftpService = new FtpDownloadService(_ftpServer, _ftpUser, _ftpPass,
                int.TryParse(_ftpPort, out var p) ? p : 21);

            _ftpConnected = await _ftpService.TestConnectionAsync();

            if (!_ftpConnected)
            {
                lblFileCheck.Text = "FTP 서버 접속 실패";
                MessageBox.Show("FTP 서버에 접속할 수 없습니다.", "접속 실패", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        try
        {
            lblFileName.Text = "파일列表 가져오는 중...";
            _fileList = await _ftpService!.GetFileListAsync(_programPath);

            pbAll.Maximum = _fileList.Count;
            lblCount.Text = $"0/{_fileList.Count}";

            // 임시 폴더 정리
            if (Directory.Exists(_tempPath))
                Directory.Delete(_tempPath, true);
            Directory.CreateDirectory(_tempPath);

            // 다운로드 실행
            await DownloadFilesAsync();
        }
        catch (Exception ex)
        {
            Logger.Error("StartUpdate", ex);
            lblFileCheck.Text = $"오류: {ex.Message}";
        }
    }

    private async Task DownloadFilesAsync()
    {
        int currentFile = 0;
        int maxRetries = 3;

        foreach (var fileInfo in _fileList)
        {
            if (_isStop) break;

            currentFile++;
            pbAll.Value = currentFile;
            lblCount.Text = $"{currentFile}/{_fileList.Count}";

            var parts = fileInfo.Split('|');
            if (parts.Length < 3) continue;

            var remotePath = parts[0];
            var fileName = parts[2];
            var localFile = Path.Combine(_tempPath, fileName);

            lblFileName.Text = fileName;

            for (int retry = 0; retry < maxRetries; retry++)
            {
                try
                {
                    var result = await _ftpService!.DownloadFileAsync(localFile, remotePath, _downloadCts);

                    if (result.Success)
                    {
                        lblFileCheck.Text = "다운로드 완료";
                        Logger.WriteLog("AUTOUPDATE", "MAIN", "Download", "성공", remotePath);
                        break;
                    }
                    else
                    {
                        lblFileCheck.Text = $"재시도 {retry + 1}/{maxRetries}";
                        await Task.Delay(2000);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("DownloadFiles", ex);
                    lblFileCheck.Text = $"오류: {ex.Message}";
                }
            }
        }

        if (!_isStop)
        {
            await ApplyUpdatesAsync();
            lblFileCheck.Text = "업데이트 완료!";

            if (chkAutoRun.IsChecked == true)
            {
                StartTargetProgram();
            }
        }
    }

    private async Task ApplyUpdatesAsync()
    {
        // ZIP 파일 해제 및 적용 로직
        if (Directory.Exists(_tempPath))
        {
            var files = Directory.GetFiles(_tempPath, "*.zip", SearchOption.AllDirectories);

            foreach (var zipFile in files)
            {
                var extractPath = Path.Combine(_tempPath, Path.GetFileNameWithoutExtension(zipFile));

                try
                {
                    System.IO.Compression.ZipFile.ExtractToDirectory(zipFile, extractPath, true);
                    lblFileCheck.Text = $"압축해제: {Path.GetFileName(zipFile)}";
                }
                catch (Exception ex)
                {
                    Logger.Error("ApplyUpdates", ex);
                }
            }
        }

        // 파일 복사
        if (Directory.Exists(_tempPath))
        {
            if (!Directory.Exists(_backupPath))
                Directory.CreateDirectory(_backupPath);

            var files = Directory.GetFiles(_tempPath, "*.*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var relativePath = file.Replace(_tempPath + Path.DirectorySeparatorChar, "");
                var destPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);

                // 프로세스 종료
                if (Path.GetExtension(destPath).ToUpper() == ".EXE")
                {
                    var procName = Path.GetFileNameWithoutExtension(destPath);
                    var procs = Process.GetProcessesByName(procName);
                    foreach (var p in procs)
                    {
                        try { p.Kill(); } catch { }
                    }
                    await Task.Delay(500);
                }

                // 백업
                if (File.Exists(destPath + "_OLD"))
                    File.Delete(destPath + "_OLD");
                if (File.Exists(destPath))
                    File.Move(destPath, destPath + "_OLD");

                // 복사
                var dir = Path.GetDirectoryName(destPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir!);
                File.Copy(file, destPath, true);

                Logger.WriteLog("AUTOUPDATE", "MAIN", "Apply", "복사", destPath);
            }
        }
    }

    private void StartTargetProgram()
    {
        try
        {
            var exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _targetExeName);
            if (File.Exists(exePath))
            {
                Process.Start(new ProcessStartInfo(exePath) { UseShellExecute = true });
                Application.Current.Shutdown();
            }
            else
            {
                MessageBox.Show($"실행 파일을 찾을 수 없습니다: {_targetExeName}\n환경설정에서 실행 파일명을 확인해 주세요.", "실행 실패", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            Logger.Error("StartTargetProgram", ex);
        }
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }

    private async void btnUpdate_Click(object sender, RoutedEventArgs e)
    {
        btnUpdate.IsEnabled = false;
        await StartUpdateAsync();
        btnUpdate.IsEnabled = true;
    }

    private void btnSettings_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow(_configPath);
        settingsWindow.Owner = this;
        if (settingsWindow.ShowDialog() == true)
        {
            LoadSettings();
            lblProgramName.Text = _programPath;
            lblAutoRunTarget.Text = $"(실행 파일: {_targetExeName})";
        }
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        _isStop = true;
        _downloadCts?.Cancel();
        Application.Current.Shutdown();
    }
}