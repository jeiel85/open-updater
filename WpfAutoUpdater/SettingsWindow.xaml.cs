using System.IO;
using System.Windows;
using AutoUpdate.Shared;

namespace WpfAutoUpdater;

public partial class SettingsWindow : Window
{
    private readonly string _configPath;

    public SettingsWindow(string configPath)
    {
        InitializeComponent();
        _configPath = configPath;
        LoadSettings();
    }

    private void LoadSettings()
    {
        txtFtpServer.Text = IniFile.Read("업데이트정보", "FTP서버", _configPath, "update.wavepos.co.kr");
        txtFtpPort.Text = IniFile.Read("업데이트정보", "FTP포트", _configPath, "21");
        txtFtpUser.Text = IniFile.Read("업데이트정보", "FTP사용자", _configPath, "devel");
        txtFtpPass.Password = IniFile.Read("업데이트정보", "FTP암호", _configPath, "");
        txtProgramPath.Text = IniFile.Read("업데이트정보", "업데이트경로", _configPath, "POPs_Renewal");
        txtExeName.Text = IniFile.Read("업데이트정보", "실행파일명", _configPath, "POPs_Renewal.exe");
    }

    private void btnSave_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            IniFile.Write("업데이트정보", "FTP서버", txtFtpServer.Text, _configPath);
            IniFile.Write("업데이트정보", "FTP포트", txtFtpPort.Text, _configPath);
            IniFile.Write("업데이트정보", "FTP사용자", txtFtpUser.Text, _configPath);
            IniFile.Write("업데이트정보", "FTP암호", txtFtpPass.Password, _configPath);
            IniFile.Write("업데이트정보", "업데이트경로", txtProgramPath.Text, _configPath);
            IniFile.Write("업데이트정보", "실행파일명", txtExeName.Text, _configPath);

            Logger.WriteLog("AUTOUPDATE", "SETTINGS", "Save", "설정저장", "성공");
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            Logger.Error("SettingsSave", ex);
            MessageBox.Show($"설정 저장 실패: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}