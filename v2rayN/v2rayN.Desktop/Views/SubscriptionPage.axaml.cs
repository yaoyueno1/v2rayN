using DialogHostAvalonia;
using v2rayN.Desktop.Common;

namespace v2rayN.Desktop.Views;

public sealed class SubscriptionCard
{
    public required SubItem Source { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string Meta { get; init; } = string.Empty;
    public string CountText { get; init; } = string.Empty;
}

public partial class SubscriptionPage : ReactiveUserControl<SubSettingViewModel>
{
    public SubscriptionPage()
    {
        InitializeComponent();

        if (!Design.IsDesignMode)
        {
            ViewModel = new SubSettingViewModel();
        }

        btnAdd.Click += async (_, _) =>
        {
            if (ViewModel is null)
            {
                return;
            }

            await ViewModel.EditSubAsync(true);
            await ReloadCardsAsync();
        };
        btnRefreshAll.Click += async (_, _) => await UpdateAsync(null);

        this.WhenActivated(disposables =>
        {
            if (ViewModel is null)
            {
                return;
            }

            ViewModel.ShowYesNoInteraction.RegisterHandler(async interaction =>
            {
                var result = await UI.ShowYesNo(interaction.Input);
                interaction.SetOutput(result == ButtonResult.Yes);
            }).DisposeWith(disposables);

            ViewModel.ShareSubInteraction.RegisterHandler(async interaction =>
            {
                var url = interaction.Input;
                if (url.IsNotEmpty())
                {
                    await DialogHost.Show(new QrcodeView(url));
                }

                interaction.SetOutput(RxVoid.Default);
            }).DisposeWith(disposables);

            _ = ReloadCardsAsync();
        });
    }

    public Task RefreshAsync() => ReloadCardsAsync();

    private async void Update_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: SubscriptionCard card })
        {
            await UpdateAsync(card.Source.Id);
        }
    }

    private async void Edit_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: SubscriptionCard card } || ViewModel is null)
        {
            return;
        }

        ViewModel.SelectedSource = card.Source;
        ViewModel.SelectedSources = [card.Source];
        await ViewModel.EditSubAsync(false);
        await ReloadCardsAsync();
    }

    private async Task UpdateAsync(string? subId)
    {
        if (TopLevel.GetTopLevel(this) is not Window { DataContext: MainWindowViewModel main })
        {
            return;
        }

        await main.UpdateSubscriptionProcess(subId ?? string.Empty, false);
        await main.ProfilesViewModel.RefreshSubscriptions();
        await main.ProfilesViewModel.RefreshServers();
        await ReloadCardsAsync();
    }

    private async Task ReloadCardsAsync()
    {
        if (ViewModel is null || Design.IsDesignMode)
        {
            return;
        }

        await ViewModel.RefreshSubItems();
        var cards = new List<SubscriptionCard>();
        foreach (var item in ViewModel.SubItems)
        {
            if (item.Id.IsNullOrEmpty() && item.Url.IsNullOrEmpty() && item.Remarks.IsNullOrEmpty())
            {
                continue;
            }

            var profiles = item.Id.IsNullOrEmpty()
                ? null
                : await AppManager.Instance.ProfileItems(item.Id);
            cards.Add(new SubscriptionCard
            {
                Source = item,
                Title = item.Remarks.IsNullOrEmpty() ? item.Url : item.Remarks,
                Url = item.Url ?? string.Empty,
                Meta = ShellStrings.FormatUpdated(item.UpdateTime),
                CountText = ShellStrings.NodeCount(profiles?.Count ?? 0),
            });
        }

        lstSubs.ItemsSource = cards;
        txtEmpty.IsVisible = cards.Count == 0;
    }
}
