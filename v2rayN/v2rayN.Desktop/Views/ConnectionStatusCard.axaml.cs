using System.Text.RegularExpressions;
using Avalonia.Controls.Shapes;
using DialogHostAvalonia;
using v2rayN.Desktop.Common;

namespace v2rayN.Desktop.Views;

public partial class ConnectionStatusCard : ReactiveUserControl<StatusBarViewModel>
{
    private static readonly Regex SpeedRegex = new(@":\s*(?<up>.+?)/s↑\s*\|\s*(?<down>.+?)/s↓", RegexOptions.Compiled);

    private readonly List<double> _upSamples = [];
    private readonly List<double> _downSamples = [];
    private bool _syncingMode;
    private DateTime? _connectedAt;
    private bool _wasConnected;

    public event EventHandler? DetailsRequested;

    public ConnectionStatusCard()
    {
        InitializeComponent();

        if (!Design.IsDesignMode)
        {
            ViewModel = StatusBarViewModel.Instance;
        }

        modeSystem.IsCheckedChanged += ModeChanged;
        modeTun.IsCheckedChanged += ModeChanged;
        modePac.IsCheckedChanged += ModeChanged;
        btnDetails.Click += (_, _) => DetailsRequested?.Invoke(this, EventArgs.Empty);

        this.WhenActivated(disposables =>
        {
            if (ViewModel is null)
            {
                return;
            }

            this.WhenAnyValue(
                    x => x.ViewModel!.RunningServerDisplay,
                    x => x.ViewModel!.SpeedProxyDisplay,
                    x => x.ViewModel!.EnableTun,
                    x => x.ViewModel!.SystemProxySelected,
                    x => x.ViewModel!.BlSystemProxyPacVisible)
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(_ => Render())
                .DisposeWith(disposables);

            ViewModel.SetClipboardDataInteraction.RegisterHandler(async interaction =>
            {
                await AvaUtils.SetClipboardData(this, interaction.Input);
                interaction.SetOutput(RxVoid.Default);
            }).DisposeWith(disposables);

            ViewModel.PasswordInputInteraction.RegisterHandler(async interaction =>
            {
                interaction.SetOutput(await PasswordInputAsync());
            }).DisposeWith(disposables);

            ViewModel.DispatcherRefreshIconInteraction.RegisterHandler(interaction =>
            {
                Dispatcher.UIThread.Post(RefreshIcon, DispatcherPriority.Default);
                interaction.SetOutput(RxVoid.Default);
            }).DisposeWith(disposables);

            Render();
        });

        if (!Design.IsDesignMode)
        {
            RefreshIcon();
        }
    }

    private void Render()
    {
        if (ViewModel is null)
        {
            return;
        }

        var connected = ViewModel.RunningServerDisplay.IsNotEmpty()
                        && ViewModel.RunningServerDisplay != ResUI.CheckServerSettings;
        txtState.Text = connected ? ShellStrings.Connected : ShellStrings.Disconnected;
        txtServer.Text = connected ? ViewModel.RunningServerDisplay : string.Empty;
        statusDot.Fill = connected
            ? (IBrush?)this.FindResource("MyPrimary") ?? Brushes.Green
            : Brushes.Gray;
        statusBadge.Background = connected
            ? (IBrush?)this.FindResource("MyPrimary") ?? Brushes.Green
            : (IBrush?)this.FindResource("MySurfaceDim") ?? Brushes.LightGray;
        ringArc.IsVisible = connected;

        if (connected && !_wasConnected)
        {
            _connectedAt = DateTime.Now;
        }

        if (!connected)
        {
            _connectedAt = null;
        }

        _wasConnected = connected;
        txtSince.Text = _connectedAt is { } at
            ? ShellStrings.Since(at.ToString("hh:mm:ss tt", CultureInfo.CurrentCulture))
            : string.Empty;

        var up = "0 B/s";
        var down = "0 B/s";
        var match = SpeedRegex.Match(ViewModel.SpeedProxyDisplay ?? string.Empty);
        if (match.Success)
        {
            up = match.Groups["up"].Value.Trim() + "/s";
            down = match.Groups["down"].Value.Trim() + "/s";
            PushSample(_upSamples, ParseSpeed(match.Groups["up"].Value));
            PushSample(_downSamples, ParseSpeed(match.Groups["down"].Value));
        }

        txtUp.Text = up;
        txtDown.Text = down;
        DrawSpark(sparkUp, _upSamples);
        DrawSpark(sparkDown, _downSamples);

        modePac.IsVisible = ViewModel.BlSystemProxyPacVisible;

        _syncingMode = true;
        if (ViewModel.EnableTun)
        {
            modeTun.IsChecked = true;
        }
        else if (ViewModel.SystemProxySelected == (int)ESysProxyType.Pac && ViewModel.BlSystemProxyPacVisible)
        {
            modePac.IsChecked = true;
        }
        else if (ViewModel.SystemProxySelected == (int)ESysProxyType.ForcedChange)
        {
            modeSystem.IsChecked = true;
        }
        else
        {
            modeSystem.IsChecked = false;
            modeTun.IsChecked = false;
            modePac.IsChecked = false;
        }

        _syncingMode = false;
    }

    private void ModeChanged(object? sender, RoutedEventArgs e)
    {
        if (_syncingMode || ViewModel is null || sender is not RadioButton { IsChecked: true })
        {
            return;
        }

        if (ReferenceEquals(sender, modeTun))
        {
            ViewModel.EnableTun = true;
            return;
        }

        ViewModel.EnableTun = false;
        ViewModel.SystemProxySelected = ReferenceEquals(sender, modePac)
            ? (int)ESysProxyType.Pac
            : (int)ESysProxyType.ForcedChange;
    }

    private static void PushSample(List<double> samples, double value)
    {
        if (samples.Count > 0 && Math.Abs(samples[^1] - value) < 0.0001 && samples.Count > 24)
        {
            return;
        }

        samples.Add(value);
        if (samples.Count > 24)
        {
            samples.RemoveAt(0);
        }
    }

    private static double ParseSpeed(string text)
    {
        var number = Regex.Match(text, @"[\d.]+");
        if (!number.Success || !double.TryParse(number.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            return 0;
        }

        if (text.Contains("GB", StringComparison.OrdinalIgnoreCase))
        {
            return value * 1024;
        }

        if (text.Contains("KB", StringComparison.OrdinalIgnoreCase))
        {
            return value / 1024;
        }

        if (text.Contains("B", StringComparison.OrdinalIgnoreCase) && !text.Contains("MB", StringComparison.OrdinalIgnoreCase))
        {
            return value / (1024 * 1024);
        }

        return value;
    }

    private static void DrawSpark(Polyline line, List<double> samples)
    {
        if (samples.Count == 0)
        {
            line.Points?.Clear();
            return;
        }

        var max = Math.Max(samples.Max(), 0.01);
        const double width = 140;
        const double height = 28;
        var points = new List<Point>(samples.Count);
        for (var i = 0; i < samples.Count; i++)
        {
            var x = samples.Count == 1 ? 0 : i * (width / (samples.Count - 1));
            var y = height - 2 - (samples[i] / max) * (height - 6);
            points.Add(new Point(x, y));
        }

        line.Points = points;
    }

    private void RefreshIcon()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        var config = AppManager.Instance.Config;
        desktop.MainWindow?.Icon = AvaUtils.GetAppIcon(config.SystemProxyItem.SysProxyType);
        var icons = TrayIcon.GetIcons(Application.Current);
        if (icons is { Count: > 0 })
        {
            icons[0].Icon = desktop.MainWindow?.Icon;
            TrayIcon.SetIcons(Application.Current, icons);
        }
    }

    private async Task<string?> PasswordInputAsync()
    {
        var dialog = new SudoPasswordInputView();
        var obj = await DialogHost.Show(dialog);
        var password = obj?.ToString();
        if (password.IsNullOrEmpty())
        {
            if (ViewModel != null)
            {
                ViewModel.EnableTun = false;
            }

            return password;
        }

        AppManager.Instance.LinuxSudoPwd = password;
        return password;
    }
}
