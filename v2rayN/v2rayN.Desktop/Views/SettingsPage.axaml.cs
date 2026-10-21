namespace v2rayN.Desktop.Views;

public partial class SettingsPage : UserControl
{
    public ThemeSettingView ThemeSetting => themeSetting;

    public event EventHandler? BackupRequested;
    public event EventHandler? CheckUpdateRequested;

    public SettingsPage()
    {
        InitializeComponent();
        btnBackup.Click += (_, _) => BackupRequested?.Invoke(this, EventArgs.Empty);
        btnUpdate.Click += (_, _) => CheckUpdateRequested?.Invoke(this, EventArgs.Empty);
    }
}
