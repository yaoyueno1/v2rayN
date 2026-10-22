namespace v2rayN.Desktop.Views;

public partial class SettingsPage : UserControl
{
    public ThemeSettingView ThemeSetting => themeSetting;

    public event EventHandler? BackupRequested;
    public event EventHandler? CheckUpdateRequested;
    public event EventHandler? UwpRequested;

    public SettingsPage()
    {
        InitializeComponent();
        var index = 0;
        foreach (var row in themeSetting.DetachRows())
        {
            settingsBoard.Children.Insert(index++, row);
        }

        btnBackup.Click += (_, _) => BackupRequested?.Invoke(this, EventArgs.Empty);
        btnUpdate.Click += (_, _) => CheckUpdateRequested?.Invoke(this, EventArgs.Empty);
        btnUwp.Click += (_, _) => UwpRequested?.Invoke(this, EventArgs.Empty);
    }
}
