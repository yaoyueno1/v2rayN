using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using AvaloniaEdit;
using Material.Styles.Themes;

namespace v2rayN.Desktop.ViewModels;

public partial class ThemeSettingViewModel : MyReactiveObject
{
    [Reactive] public partial string CurrentTheme { get; set; }

    [Reactive] public partial int CurrentFontSize { get; set; }

    [Reactive] public partial string CurrentLanguage { get; set; }

    public ThemeSettingViewModel()
    {
        _config = AppManager.Instance.Config;

        BindingUI();
        RestoreUI();
    }

    private void RestoreUI()
    {
        ModifyTheme();
        ModifyFontFamily();
        ModifyFontSize();
    }

    private void BindingUI()
    {
        CurrentTheme = _config.UiItem.CurrentTheme;
        CurrentFontSize = _config.UiItem.CurrentFontSize;
        CurrentLanguage = _config.UiItem.CurrentLanguage;

        this.WhenAnyValue(x => x.CurrentTheme)
            .Where(y => y.IsNotEmpty())
            .SubscribeAsync(async _ =>
            {
                if (_config.UiItem.CurrentTheme != CurrentTheme)
                {
                    _config.UiItem.CurrentTheme = CurrentTheme;
                    ModifyTheme();
                    await ConfigHandler.SaveConfig(_config);
                }
            });

        this.WhenAnyValue(x => x.CurrentFontSize)
            .Where(y => y > 0)
            .SubscribeAsync(async _ =>
            {
                if (_config.UiItem.CurrentFontSize != CurrentFontSize && CurrentFontSize >= Global.MinFontSize)
                {
                    _config.UiItem.CurrentFontSize = CurrentFontSize;
                    ModifyFontSize();
                    await ConfigHandler.SaveConfig(_config);
                }
            });

        this.WhenAnyValue(x => x.CurrentLanguage)
            .Where(y => !y.IsNullOrEmpty())
            .SubscribeAsync(async _ =>
            {
                if (CurrentLanguage.IsNotEmpty() && _config.UiItem.CurrentLanguage != CurrentLanguage)
                {
                    _config.UiItem.CurrentLanguage = CurrentLanguage;
                    Thread.CurrentThread.CurrentUICulture = new(CurrentLanguage);
                    await ConfigHandler.SaveConfig(_config);
                    NoticeManager.Instance.Enqueue(ResUI.NeedRebootTips);
                }
            });
    }

    private void ModifyTheme()
    {
        var app = Application.Current;
        if (app is null)
        {
            return;
        }

        var (variant, primary, secondary) = CurrentTheme switch
        {
            nameof(ETheme.Dark) => (ThemeVariant.Dark, Color.Parse("#3DDC97"), Color.Parse("#D0BCFF")),
            nameof(ETheme.Light) => (ThemeVariant.Light, Color.Parse("#0F8F6A"), Color.Parse("#6750A4")),
            nameof(ETheme.Aquatic) => (ThemeVariant.Light, Color.Parse("#0277BD"), Color.Parse("#00897B")),
            nameof(ETheme.Desert) => (ThemeVariant.Light, Color.Parse("#C47B2B"), Color.Parse("#8D6E63")),
            nameof(ETheme.Dusk) => (ThemeVariant.Dark, Color.Parse("#B388FF"), Color.Parse("#80CBC4")),
            nameof(ETheme.NightSky) => (ThemeVariant.Dark, Color.Parse("#8AB4F8"), Color.Parse("#80DEEA")),
            _ => (ThemeVariant.Default, Color.Parse("#0F8F6A"), Color.Parse("#6750A4")),
        };

        try
        {
            var material = app.LocateMaterialTheme<CustomMaterialTheme>();
            material.PrimaryColor = primary;
            material.SecondaryColor = secondary;
        }
        catch (Exception ex)
        {
            Logging.SaveLog("ModifyTheme", ex);
        }

        app.RequestedThemeVariant = variant;
    }

    private void ModifyFontSize()
    {
        double size = CurrentFontSize;
        if (size < Global.MinFontSize)
        {
            return;
        }

        Style style = new(x => Selectors.Or(
            x.OfType<Button>(),
            x.OfType<TextBox>(),
            x.OfType<TextBlock>(),
            x.OfType<SelectableTextBlock>(),
            x.OfType<Menu>(),
            x.OfType<ContextMenu>(),
            x.OfType<DataGridRow>(),
            x.OfType<ListBoxItem>(),
            x.OfType<HeaderedContentControl>(),
            x.OfType<TextEditor>()
        ));
        style.Add(new Setter()
        {
            Property = TemplatedControl.FontSizeProperty,
            Value = size,
        });
        Application.Current?.Styles.Add(style);

        ModifyFontSizeEx(size);
    }

    private void ModifyFontSizeEx(double size)
    {
        //DataGrid
        var rowHeight = 20 + (size / 2);
        var style = new Style(x => x.OfType<DataGrid>());
        style.Add(new Setter(DataGrid.RowHeightProperty, rowHeight));
        Application.Current?.Styles.Add(style);
    }

    private void ModifyFontFamily()
    {
        var currentFontFamily = _config.UiItem.CurrentFontFamily;
        if (currentFontFamily.IsNullOrEmpty())
        {
            return;
        }

        try
        {
            Style style = new(x => Selectors.Or(
                x.OfType<Button>(),
                x.OfType<TextBox>(),
                x.OfType<TextBlock>(),
                x.OfType<SelectableTextBlock>(),
                x.OfType<Menu>(),
                x.OfType<ContextMenu>(),
                x.OfType<DataGridRow>(),
                x.OfType<ListBoxItem>(),
                x.OfType<HeaderedContentControl>(),
                x.OfType<WindowNotificationManager>(),
                x.OfType<TextEditor>()
            ));
            style.Add(new Setter()
            {
                Property = TemplatedControl.FontFamilyProperty,
                Value = new FontFamily(currentFontFamily),
            });
            Application.Current?.Styles.Add(style);
        }
        catch (Exception ex)
        {
            Logging.SaveLog("ModifyFontFamily", ex);
        }
    }
}
