using Avalonia.Data;
using v2rayN.Desktop.Common;
using v2rayN.Desktop.ViewModels;

namespace v2rayN.Desktop.Views;

/// <summary>
/// ThemeSettingView.xaml
/// </summary>
public partial class ThemeSettingView : ReactiveUserControl<ThemeSettingViewModel>
{
    private sealed record ThemeChoice(string Value, string Display);

    private static readonly ThemeChoice[] ThemeChoices =
    [
        new(nameof(ETheme.FollowSystem), ShellStrings.ThemeFollowSystem),
        new(nameof(ETheme.Light), ShellStrings.ThemeLight),
        new(nameof(ETheme.Dark), ShellStrings.ThemeDark),
        new(nameof(ETheme.Aquatic), ShellStrings.ThemeAquatic),
        new(nameof(ETheme.Desert), ShellStrings.ThemeDesert),
        new(nameof(ETheme.Dusk), ShellStrings.ThemeDusk),
        new(nameof(ETheme.NightSky), ShellStrings.ThemeNightSky),
    ];

    public Control[] DetachRows()
    {
        var rows = appearanceRows.Children.ToArray();
        appearanceRows.Children.Clear();
        return rows;
    }

    public void OpenThemeDropDown() => cmbCurrentTheme.IsDropDownOpen = true;

    public void CloseDropDowns()
    {
        cmbCurrentTheme.IsDropDownOpen = false;
        cmbCurrentFontSize.IsDropDownOpen = false;
        cmbCurrentLanguage.IsDropDownOpen = false;
    }

    public ThemeSettingView()
    {
        InitializeComponent();
        ViewModel = new ThemeSettingViewModel();

        cmbCurrentTheme.ItemsSource = ThemeChoices;
        cmbCurrentTheme.DisplayMemberBinding = new Binding(nameof(ThemeChoice.Display));
        cmbCurrentTheme.SelectedValueBinding = new Binding(nameof(ThemeChoice.Value));
        cmbCurrentFontSize.ItemsSource = Enumerable.Range(Global.MinFontSize, Global.MinFontSizeCount).ToList();
        cmbCurrentLanguage.ItemsSource = Global.LanguageOptions;
        cmbCurrentLanguage.DisplayMemberBinding = new Binding(nameof(LanguageOption.Display));
        cmbCurrentLanguage.SelectedValueBinding = new Binding(nameof(LanguageOption.Value));

        this.WhenActivated(disposables =>
        {
            this.Bind(ViewModel, vm => vm.CurrentTheme, v => v.cmbCurrentTheme.SelectedValue).DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.CurrentFontSize, v => v.cmbCurrentFontSize.SelectedValue).DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.CurrentLanguage, v => v.cmbCurrentLanguage.SelectedValue).DisposeWith(disposables);
        });
    }
}
