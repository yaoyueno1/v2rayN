using Avalonia.VisualTree;
using DialogHostAvalonia;
using v2rayN.Desktop.Base;
using v2rayN.Desktop.Common;
using v2rayN.Desktop.Controls;
using v2rayN.Desktop.Manager;

namespace v2rayN.Desktop.Views;

public partial class MainWindow : WindowBase<MainWindowViewModel>
{
    private static Config _config;
    private int _page;
    private int _pageBeforeDetails;
    private bool _navSync;
    private CheckUpdateView? _checkUpdateView;
    private int _paperDialogEpoch;
    private BackupAndRestoreView? _backupAndRestoreView;
    private bool _blCloseByUser = false;

    public MainWindow()
    {
        InitializeComponent();

        _config = AppManager.Instance.Config;

        KeyDown += MainWindow_KeyDown;
        menuPromotion.Click += MenuPromotion_Click;
        menuCheckUpdate.Click += MenuCheckUpdate_Click;
        btnNewUpdate.Click += MenuCheckUpdate_Click;
        menuClose.Click += MenuClose_Click;
        Opened += (_, _) => StartPreviewWatcher();
        btnTheme.Click += BtnTheme_Click;
        navHome.IsCheckedChanged += Nav_Checked;
        navProfiles.IsCheckedChanged += Nav_Checked;
        navSubs.IsCheckedChanged += Nav_Checked;
        navRouting.IsCheckedChanged += Nav_Checked;
        navSettings.IsCheckedChanged += Nav_Checked;
        navHelp.IsCheckedChanged += Nav_Checked;
        connectionCard.DetailsRequested += (_, _) => ShowDetails();
        settingsPage.BackupRequested += (_, _) => MenuBackupAndRestore_Click(this, new RoutedEventArgs());
        settingsPage.CheckUpdateRequested += (_, _) => MenuCheckUpdate_Click(this, new RoutedEventArgs());
        settingsPage.UwpRequested += (_, _) => MenuSettingsSetUWP_Click(this, new RoutedEventArgs());

        this.WhenActivated(disposables =>
        {
            //servers
            this.BindCommand(ViewModel, vm => vm.AddVmessServerCmd, v => v.menuAddVmessServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddVlessServerCmd, v => v.menuAddVlessServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddShadowsocksServerCmd, v => v.menuAddShadowsocksServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddSocksServerCmd, v => v.menuAddSocksServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddHttpServerCmd, v => v.menuAddHttpServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddTrojanServerCmd, v => v.menuAddTrojanServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddHysteria2ServerCmd, v => v.menuAddHysteria2Server).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddTuicServerCmd, v => v.menuAddTuicServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddWireguardServerCmd, v => v.menuAddWireguardServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddAnytlsServerCmd, v => v.menuAddAnytlsServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddNaiveServerCmd, v => v.menuAddNaiveServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddMasqueServerCmd, v => v.menuAddMasqueServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddCustomServerCmd, v => v.menuAddCustomServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddCustomOutboundServerCmd, v => v.menuAddCustomOutboundServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddPolicyGroupServerCmd, v => v.menuAddPolicyGroupServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddProxyChainServerCmd, v => v.menuAddProxyChainServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddServerViaClipboardCmd, v => v.menuAddServerViaClipboard).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddServerViaScanCmd, v => v.menuAddServerViaScan).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddServerViaImageCmd, v => v.menuAddServerViaImage).DisposeWith(disposables);

            //sub
            this.BindCommand(ViewModel, vm => vm.SubSettingCmd, v => v.menuSubSetting).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.SubUpdateCmd, v => v.menuSubUpdate).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.SubUpdateViaProxyCmd, v => v.menuSubUpdateViaProxy).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.SubGroupUpdateCmd, v => v.menuSubGroupUpdate).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.SubGroupUpdateViaProxyCmd, v => v.menuSubGroupUpdateViaProxy).DisposeWith(disposables);

            this.BindCommand(ViewModel, vm => vm.ReloadCmd, v => v.menuReload).DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.BlReloadEnabled, v => v.menuReload.IsEnabled).DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.BlNewUpdate, v => v.btnNewUpdate.IsVisible).DisposeWith(disposables);

            this.OneWayBind(ViewModel, vm => vm.ProfilesViewModel, v => v.profilesHost.Content).DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.MsgViewModel, v => v.tabMsgView.Content).DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.ClashProxiesViewModel, v => v.tabClashProxies.Content).DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.ClashConnectionsViewModel, v => v.tabClashConnections.Content).DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.ShowClashUI, v => v.tabClashProxies.IsVisible).DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.ShowClashUI, v => v.tabClashConnections.IsVisible).DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.TabMainSelectedIndex, v => v.tabMain.SelectedIndex).DisposeWith(disposables);

            ViewModel.ReadTextFromClipboardInteraction.RegisterHandler(async interaction =>
            {
                var result = await AvaUtils.GetClipboardData(this);
                interaction.SetOutput(result);
            }).DisposeWith(disposables);

            ViewModel.ScanScreenInteraction.RegisterHandler(async interaction =>
            {
                ShowHideWindow(false);
                await Task.Delay(200);
                var result = QRCodeAvaloniaUtils.CaptureScreen();
                ShowHideWindow(true);
                interaction.SetOutput(result);
            }).DisposeWith(disposables);

            ViewModel.BrowseImageFileInteraction.RegisterHandler(async interaction =>
            {
                var result = await UI.OpenFileDialog(null);
                interaction.SetOutput(result);
            }).DisposeWith(disposables);

            ViewModel.ShowHideWindowInteraction.RegisterHandler(interaction =>
            {
                ShowHideWindow(interaction.Input);
                interaction.SetOutput(RxVoid.Default);
            }).DisposeWith(disposables);

            AppEvents.SendSnackMsgRequested
              .AsObservable()
              .ObserveOn(RxSchedulers.MainThreadScheduler)
              .Subscribe(async content => await DelegateSnackMsg(content))
              .DisposeWith(disposables);

            AppEvents.AppExitRequested
              .AsObservable()
              .ObserveOn(RxSchedulers.MainThreadScheduler)
              .Subscribe(_ => StorageUI())
              .DisposeWith(disposables);

            AppEvents.ShutdownRequested
              .AsObservable()
              .ObserveOn(RxSchedulers.MainThreadScheduler)
              .Subscribe(Shutdown)
              .DisposeWith(disposables);
        });

        if (Utils.IsWindows())
        {
            Title = $"{Utils.GetVersion()} - {(Utils.IsAdministrator() ? ResUI.RunAsAdmin : ResUI.NotRunAsAdmin)}";

            if (!Design.IsDesignMode)
            {
                ThreadPool.RegisterWaitForSingleObject(Program.ProgramStarted, OnProgramStarted, null, -1, false);
                HotkeyManager.Instance.Init(_config, OnHotkeyHandler);
            }
        }
        else
        {
            Title = $"{Utils.GetVersion()}";
            menuAddServerViaScan.IsVisible = false;
        }

        if (_config.UiItem.AutoHideStartup && Utils.IsWindows())
        {
            WindowState = WindowState.Minimized;
        }

        AddHelpMenuItem();
    }

    #region Event

    private void OnProgramStarted(object state, bool timeout)
    {
        Dispatcher.UIThread.Post(() =>
                ShowHideWindow(true),
            DispatcherPriority.Default);
    }

    private async Task DelegateSnackMsg(string content)
    {
        PaperToasts.Show(toastHost, content);
        await Task.CompletedTask;
    }

    private void OnHotkeyHandler(EGlobalHotkey e)
    {
        switch (e)
        {
            case EGlobalHotkey.ShowForm:
                Dispatcher.UIThread.Post(() => ShowHideWindow(null));
                break;

            case EGlobalHotkey.SystemProxyClear:
            case EGlobalHotkey.SystemProxySet:
            case EGlobalHotkey.SystemProxyUnchanged:
            case EGlobalHotkey.SystemProxyPac:
                AppEvents.SysProxyChangeRequested.Publish((ESysProxyType)((int)e - 1));
                break;
        }
    }

    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        if (_blCloseByUser)
        {
            return;
        }

        Logging.SaveLog("OnClosing -> " + e.CloseReason.ToString());

        switch (e.CloseReason)
        {
            case WindowCloseReason.WindowClosing:
                e.Cancel = true;
                await ExitNowAsync();
                break;

            case WindowCloseReason.OwnerWindowClosing:
                e.Cancel = true;
                ShowHideWindow(false);
                break;

            case WindowCloseReason.ApplicationShutdown or WindowCloseReason.OSShutdown:
                await AppManager.Instance.AppExitAsync(false);
                break;
        }

        base.OnClosing(e);
    }

    private async void MainWindow_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers is KeyModifiers.Control or KeyModifiers.Meta)
        {
            switch (e.Key)
            {
                case Key.V:
                    await AddServerViaClipboardAsync();
                    break;

                case Key.S:
                    await ScanScreenTaskAsync();
                    break;
            }
        }
        else
        {
            if (e.Key == Key.F5)
            {
                ViewModel?.Reload();
            }
        }
    }

    private void MenuPromotion_Click(object? sender, RoutedEventArgs e)
    {
        ProcUtils.ProcessStart($"{Utils.Base64Decode(Global.PromotionUrl)}?t={DateTime.Now.Ticks}");
    }

    private void MenuSettingsSetUWP_Click(object? sender, RoutedEventArgs e)
    {
        ProcUtils.ProcessStart(Utils.GetBinPath("EnableLoopback.exe"));
    }

    public async Task AddServerViaClipboardAsync()
    {
        var clipboardData = await AvaUtils.GetClipboardData(this);
        if (clipboardData.IsNotEmpty() && ViewModel != null)
        {
            await ViewModel.AddServerViaClipboardAsync(clipboardData);
        }
    }

    public async Task ScanScreenTaskAsync()
    {
        ShowHideWindow(false);

        await Task.Delay(200);

        var bytes = QRCodeAvaloniaUtils.CaptureScreen();
        if (bytes != null && ViewModel != null)
        {
            await ViewModel.ScanScreenResult(bytes);
        }

        ShowHideWindow(true);
    }

    private async void MenuCheckUpdate_Click(object? sender, RoutedEventArgs e)
    {
        _checkUpdateView ??= new CheckUpdateView();
        _checkUpdateView.ViewModel = ViewModel?.CheckUpdateViewModel;
        await ShowCheckUpdateAsync();

        AppEvents.HasUpdateNotified.Publish(false);
    }

    private async Task ShowCheckUpdateAsync()
    {
        if (DialogHost.IsDialogOpen(null!))
        {
            return;
        }

        var epoch = UsePaperDialogChrome();
        try
        {
            await DialogHost.Show(_checkUpdateView);
        }
        finally
        {
            RestoreDialogChrome(epoch);
        }
    }

    private int UsePaperDialogChrome()
    {
        var epoch = ++_paperDialogEpoch;
        rootDialog.Background = Brushes.Transparent;
        rootDialog.DialogMargin = new Thickness(0);
        DialogHostStyle.SetBorderThickness(rootDialog, new Thickness(0));
        DialogHostStyle.SetBorderBrush(rootDialog, Brushes.Transparent);
        DialogHostStyle.SetCornerRadius(rootDialog, new CornerRadius(0));
        DialogHostStyle.SetClipToBounds(rootDialog, false);
        DialogHostStyle.SetBoxShadow(rootDialog, new BoxShadows(new BoxShadow { Color = Colors.Transparent }));
        return epoch;
    }

    private void RestoreDialogChrome(int epoch)
    {
        if (epoch != _paperDialogEpoch || DialogHost.IsDialogOpen(null!))
        {
            return;
        }

        rootDialog.ClearValue(DialogHost.BackgroundProperty);
        rootDialog.ClearValue(DialogHost.DialogMarginProperty);
        rootDialog.ClearValue(DialogHostStyle.BorderThicknessProperty);
        rootDialog.ClearValue(DialogHostStyle.BorderBrushProperty);
        rootDialog.ClearValue(DialogHostStyle.CornerRadiusProperty);
        rootDialog.ClearValue(DialogHostStyle.ClipToBoundsProperty);
        rootDialog.ClearValue(DialogHostStyle.BoxShadowProperty);
    }

    private void MenuBackupAndRestore_Click(object? sender, RoutedEventArgs e)
    {
        _backupAndRestoreView ??= new BackupAndRestoreView();
        _backupAndRestoreView.ViewModel = ViewModel?.BackupAndRestoreViewModel;
        DialogHost.Show(_backupAndRestoreView);
    }

    private async Task ExitNowAsync()
    {
        if (_blCloseByUser)
        {
            return;
        }

        _blCloseByUser = true;
        StorageUI();
        await AppManager.Instance.AppExitAsync(true);
    }

    private async void MenuClose_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            await ExitNowAsync();
        }
        catch
        {
            // Ignore
        }
    }

    private void Shutdown(bool obj)
    {
        if (obj is bool b && _blCloseByUser == false)
        {
            _blCloseByUser = b;
        }
        StorageUI();
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            HotkeyManager.Instance.Dispose();
            desktop.Shutdown();
        }
    }

    #endregion Event

    #region UI

    public void ShowHideWindow(bool? blShow)
    {
        var bl = blShow ??
                    (Utils.IsLinux() || Utils.IsMacOS()
                    ? (!AppManager.Instance.ShowInTaskbar ^ (WindowState == WindowState.Minimized))
                    : !AppManager.Instance.ShowInTaskbar);
        if (bl)
        {
            Show();
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
            Activate();
            Focus();
        }
        else
        {
            if (Utils.IsLinux() && _config.UiItem.Hide2TrayWhenClose == false)
            {
                WindowState = WindowState.Minimized;
                return;
            }

            foreach (var ownedWindow in OwnedWindows)
            {
                ownedWindow.Close();
            }
            Hide();
        }

        AppManager.Instance.ShowInTaskbar = bl;
    }

    protected override void OnLoaded(object? sender, RoutedEventArgs e)
    {
        base.OnLoaded(sender, e);
        var version = Utils.GetVersionInfo();
        txtVersion.Text = version;
        txtAboutVersion.Text = $"{Global.AppName} {version}";
        ShowPage(0);
        if (_config.UiItem.AutoHideStartup)
        {
            ShowHideWindow(false);
        }
    }

    private void StorageUI()
    {
        ConfigHandler.SaveWindowSizeItem(_config, GetType().Name, Width, Height);
    }

    private void Nav_Checked(object? sender, RoutedEventArgs e)
    {
        if (_navSync || !IsLoaded || sender is not RadioButton { IsChecked: true, Tag: string tag })
        {
            return;
        }

        if (int.TryParse(tag, out var page))
        {
            ShowPage(page);
        }
    }

    private void ShowDetails()
    {
        _pageBeforeDetails = _page;
        ShowPage(6);
    }

    private void DetailsBack_Click(object? sender, RoutedEventArgs e)
    {
        ShowPage(_pageBeforeDetails);
    }

    private void ShowPage(int page)
    {
        _page = page;
        pageServers.IsVisible = page is 0 or 1;
        pageSubs.IsVisible = page == 2;
        pageRouting.IsVisible = page == 3;
        pageSettings.IsVisible = page == 4;
        pageHelp.IsVisible = page == 5;
        pageDetails.IsVisible = page == 6;
        configActions.IsVisible = page == 1;
        Grid.SetColumn(profilesHost, page == 1 ? 1 : 0);
        Grid.SetColumnSpan(profilesHost, page == 1 ? 1 : 2);
        ApplyCardLayout();

        if (page == 2)
        {
            _ = subscriptionPage.RefreshAsync();
        }

        if (page == 6)
        {
            return;
        }

        _navSync = true;
        navHome.IsChecked = page == 0;
        navProfiles.IsChecked = page == 1;
        navSubs.IsChecked = page == 2;
        navRouting.IsChecked = page == 3;
        navSettings.IsChecked = page == 4;
        navHelp.IsChecked = page == 5;
        _navSync = false;
    }

    private void ApplyCardLayout()
    {
        var card = _page == 0;
        var views = profilesHost.GetVisualDescendants().OfType<ProfilesView>().ToList();
        if (views.Count == 0)
        {
            Dispatcher.UIThread.Post(() =>
            {
                foreach (var view in profilesHost.GetVisualDescendants().OfType<ProfilesView>())
                {
                    view.UseCardLayout = _page == 0;
                }
            }, DispatcherPriority.Background);
            return;
        }

        foreach (var view in views)
        {
            view.UseCardLayout = card;
        }
    }

    private void BtnTheme_Click(object? sender, RoutedEventArgs e)
    {
        if (settingsPage.ThemeSetting.ViewModel is not { } theme)
        {
            return;
        }

        var dark = Application.Current?.ActualThemeVariant == ThemeVariant.Dark;
        theme.CurrentTheme = dark ? nameof(ETheme.Light) : nameof(ETheme.Dark);
    }

    private void AddHelpMenuItem()
    {
        var coreInfo = CoreInfoManager.Instance.GetCoreInfo();
        foreach (var it in coreInfo
            .Where(t => t.CoreType is not ECoreType.v2fly
                        and not ECoreType.hysteria))
        {
            var url = it.Url?.Replace(@"/releases", "");
            var title = string.Format(ResUI.menuWebsiteItem, it.CoreType.ToString().Replace("_", " ")).UpperFirstChar();
            var host = string.Empty;
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                host = uri.Host;
            }

            var body = new StackPanel { Spacing = 2 };
            var titleBlock = new TextBlock { Text = title };
            titleBlock.Classes.Add("help-title");
            body.Children.Add(titleBlock);
            if (!string.IsNullOrEmpty(host))
            {
                var domain = new TextBlock { Text = host };
                domain.Classes.Add("help-domain");
                body.Children.Add(domain);
            }

            var item = new Button
            {
                Tag = url,
                Content = body,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Left
            };
            item.Classes.Add("help-card");
            item.Click += HelpLink_Click;
            helpLinks.Children.Add(item);
        }
    }

    private void HelpLink_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button item)
        {
            ProcUtils.ProcessStart(item.Tag?.ToString());
        }
    }

    private MenuFlyout? _trayPreview;
    private string _previewShot = string.Empty;

    private void StartPreviewWatcher()
    {
        if (Environment.GetEnvironmentVariable("V2RAYN_PREVIEW") != "1")
        {
            return;
        }

        _ = WatchPreviewAsync();
    }

    private async Task WatchPreviewAsync()
    {
        const string path = "/tmp/v2rayn-preview.txt";
        while (true)
        {
            try
            {
                await Task.Delay(250);
                if (!File.Exists(path))
                {
                    continue;
                }

                var shot = (await File.ReadAllTextAsync(path)).Trim();
                if (shot.Length == 0 || shot == _previewShot)
                {
                    continue;
                }

                _previewShot = shot;
                await Dispatcher.UIThread.InvokeAsync(() => ApplyPreviewShot(shot));
            }
            catch (Exception ex)
            {
                Logging.SaveLog("WatchPreviewAsync", ex);
            }
        }
    }

    private void ApplyPreviewShot(string shot)
    {
        _trayPreview?.Hide();
        settingsPage.ThemeSetting.CloseDropDowns();
        routingPage.CloseOutboundDropDown();
        connectionCard.CloseDropDowns();
        PaperToasts.Clear(toastHost);
        if (DialogHost.IsDialogOpen(null!))
        {
            DialogHost.Close(null);
        }

        switch (shot)
        {
            case "settings":
                ShowPage(4);
                break;
            case "theme":
                ShowPage(4);
                Dispatcher.UIThread.Post(() => settingsPage.ThemeSetting.OpenThemeDropDown(), DispatcherPriority.Background);
                break;
            case "subs":
                ShowPage(2);
                break;
            case "routing":
                ShowPage(3);
                break;
            case "routing-open":
                ShowPage(3);
                Dispatcher.UIThread.Post(() => routingPage.OpenFirstOutbound(), DispatcherPriority.Loaded);
                break;
            case "update":
                if (ViewModel is not null)
                {
                    ViewModel.BlNewUpdate = true;
                }

                break;
            case "connect-on":
                connectionCard.PreviewConnected("预览节点");
                break;
            case "connect-off":
                connectionCard.HoldDisconnected();
                break;
            case "tray":
                ShowTrayPreview();
                break;
            case "sidebar":
                ShowPage(0);
                connectionCard.PreviewTunAndProxy(true, (int)ESysProxyType.ForcedChange);
                break;
            case "proxy-open":
                ShowPage(0);
                connectionCard.PreviewTunAndProxy(true, (int)ESysProxyType.ForcedChange);
                Dispatcher.UIThread.Post(() => connectionCard.OpenProxyDropDown(), DispatcherPriority.Background);
                break;
            case "routing-side":
                ShowPage(0);
                connectionCard.PreviewTunAndProxy(true, (int)ESysProxyType.ForcedChange);
                Dispatcher.UIThread.Post(() => connectionCard.OpenRoutingDropDown(), DispatcherPriority.Background);
                break;
            case "toast":
                ShowPage(0);
                PaperToasts.Show(toastHost, "[VLESS] sg新加坡高速08|BGP| CTCU(cfyes***xyz:443)\n186 ms", pin: true);
                PaperToasts.Show(toastHost, ResUI.SpeedtestingCompleted, pin: true);
                break;
            case "check-update":
                _ = ShowCheckUpdatePreviewAsync(false);
                break;
            case "check-update-dark":
                _ = ShowCheckUpdatePreviewAsync(true);
                break;
        }
    }

    private async Task ShowCheckUpdatePreviewAsync(bool dark)
    {
        if (Application.Current is not null)
        {
            Application.Current.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
        }

        if (DialogHost.IsDialogOpen(null!))
        {
            DialogHost.Close(null);
            await Task.Delay(250);
        }

        _checkUpdateView ??= new CheckUpdateView();
        _checkUpdateView.ViewModel = ViewModel?.CheckUpdateViewModel;
        _checkUpdateView.PreviewSample();
        if (DialogHost.IsDialogOpen(null!))
        {
            return;
        }

        var epoch = UsePaperDialogChrome();
        var shown = DialogHost.Show(_checkUpdateView);
        _ = shown.ContinueWith(_ => Dispatcher.UIThread.Post(() => RestoreDialogChrome(epoch)));
        await Task.Delay(350);
        _checkUpdateView.SelectLastRow();
    }

    private void ShowTrayPreview()
    {
        var status = StatusBarViewModel.Instance;
        var flyout = new MenuFlyout
        {
            Placement = PlacementMode.BottomEdgeAlignedLeft,
        };

        MenuItem Item(string header, System.Windows.Input.ICommand? command = null, bool radio = false, bool check = false)
        {
            return new MenuItem
            {
                Header = header,
                Command = command,
                ToggleType = radio ? MenuItemToggleType.Radio : MenuItemToggleType.None,
                IsChecked = check,
            };
        }

        flyout.Items.Add(Item(ShellStrings.TrayCopyProxy, status.CopyProxyCmdToClipboardCmd));
        flyout.Items.Add(new Separator());
        flyout.Items.Add(Item(ResUI.menuSystemProxyClear, status.SystemProxyClearCmd, true, status.BlSystemProxyClear));
        flyout.Items.Add(Item(ResUI.menuSystemProxySet, status.SystemProxySetCmd, true, status.BlSystemProxySet));
        flyout.Items.Add(Item(ResUI.menuSystemProxyNothing, status.SystemProxyNothingCmd, true, status.BlSystemProxyNothing));
        flyout.Items.Add(Item(ShellStrings.TrayPac, status.SystemProxyPacCmd, true, status.BlSystemProxyPac));
        flyout.Items.Add(new Separator());
        var import = Item(ShellStrings.TrayImport);
        import.Click += async (_, _) => await AddServerViaClipboardAsync();
        flyout.Items.Add(import);
        flyout.Items.Add(Item(ShellStrings.TrayUpdate, status.SubUpdateCmd));
        flyout.Items.Add(Item(ShellStrings.TrayUpdateViaProxy, status.SubUpdateViaProxyCmd));
        flyout.Items.Add(new Separator());
        var exit = Item(ResUI.menuExit);
        exit.Click += (_, _) => MenuClose_Click(this, new RoutedEventArgs());
        flyout.Items.Add(exit);

        _trayPreview = flyout;
        flyout.ShowAt(trayPreviewAnchor, false);
    }

    #endregion UI
}
