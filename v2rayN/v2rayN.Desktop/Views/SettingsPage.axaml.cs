using v2rayN.Desktop.Common;

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
        foreach (var row in themeSetting.DetachRows())
        {
            appearanceBoard.Children.Add(row);
        }

        btnBackup.Click += (_, _) => BackupRequested?.Invoke(this, EventArgs.Empty);
        btnUpdate.Click += (_, _) => CheckUpdateRequested?.Invoke(this, EventArgs.Empty);
        btnUwp.Click += (_, _) => UwpRequested?.Invoke(this, EventArgs.Empty);
        btnClearStats.Click += async (_, _) => await ConfirmClearStatsAsync();
        btnPresetDefault.Click += Preset_Click;
        btnPresetRussia.Click += Preset_Click;
        btnPresetIran.Click += Preset_Click;
    }

    private async Task ConfirmClearStatsAsync()
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        if (await UI.ShowYesNo(ShellStrings.ClearStatsConfirm, danger: true) != ButtonResult.Yes)
        {
            return;
        }

        vm.ClearServerStatisticsCmd.Execute(RxVoid.Default).Subscribe(_ => { }, _ => { });
    }

    private void Preset_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button || DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        foreach (var preset in new[] { btnPresetDefault, btnPresetRussia, btnPresetIran })
        {
            preset.Classes.Remove("selected");
        }

        button.Classes.Add("selected");
        var command = button == btnPresetRussia
            ? vm.RegionalPresetRussiaCmd
            : button == btnPresetIran
                ? vm.RegionalPresetIranCmd
                : vm.RegionalPresetDefaultCmd;
        command.Execute(RxVoid.Default).Subscribe(_ => { }, _ => { });
    }
}
