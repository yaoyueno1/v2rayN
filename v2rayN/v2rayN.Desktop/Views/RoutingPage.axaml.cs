using System.Collections.Specialized;
using Avalonia.Controls.Primitives;
using v2rayN.Desktop.Common;

namespace v2rayN.Desktop.Views;

public sealed class RoutingRuleCard : ReactiveObject
{
    private bool _enabled;

    public required string Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Pattern { get; init; } = string.Empty;
    public string Outbound { get; init; } = Global.ProxyTag;

    public bool Enabled
    {
        get => _enabled;
        set => this.RaiseAndSetIfChanged(ref _enabled, value);
    }

    public bool IsProxy => string.Equals(Outbound, Global.ProxyTag, StringComparison.OrdinalIgnoreCase);
    public bool IsDirect => string.Equals(Outbound, Global.DirectTag, StringComparison.OrdinalIgnoreCase);
    public bool IsBlock => string.Equals(Outbound, Global.BlockTag, StringComparison.OrdinalIgnoreCase);
}

public partial class RoutingPage : ReactiveUserControl<StatusBarViewModel>
{
    private readonly Dictionary<string, IDisposable> _enabledSubscriptions = new();
    private RoutingRuleSettingViewModel? _rules;
    private bool _syncingModes;
    private bool _loadingCards;

    public RoutingPage()
    {
        InitializeComponent();

        if (!Design.IsDesignMode)
        {
            ViewModel = StatusBarViewModel.Instance;
        }

        btnAdd.Click += async (_, _) => await AddRuleAsync();
        btnAdvanced.Click += (_, _) =>
        {
            if (TopLevel.GetTopLevel(this) is not Window { DataContext: MainWindowViewModel main })
            {
                return;
            }

            main.RoutingSettingCmd.Execute(RxVoid.Default)
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Subscribe(async _ => await LoadRulesAsync(ViewModel?.SelectedRouting));
        };

        this.WhenActivated(disposables =>
        {
            if (ViewModel is null)
            {
                return;
            }

            NotifyCollectionChangedEventHandler onRoutingsChanged = (_, _) => RebuildModes();
            ViewModel.RoutingItems.CollectionChanged += onRoutingsChanged;
            new ActionDisposable(() => ViewModel.RoutingItems.CollectionChanged -= onRoutingsChanged).DisposeWith(disposables);

            this.WhenAnyValue(x => x.ViewModel!.SelectedRouting)
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .SubscribeAsync(async routing => await LoadRulesAsync(routing))
                .DisposeWith(disposables);

            RebuildModes();
        });
    }

    private void RebuildModes()
    {
        if (ViewModel is null)
        {
            return;
        }

        _syncingModes = true;
        modeBar.Children.Clear();
        foreach (var item in ViewModel.RoutingItems)
        {
            var button = new ToggleButton
            {
                Content = ShellStrings.ShortRoutingName(item.Remarks),
                Tag = item,
                IsChecked = item.Id == ViewModel.SelectedRouting?.Id,
                MinWidth = 72,
            };
            button.Classes.Add("segment");
            button.IsCheckedChanged += ModeChanged;
            ToolTip.SetTip(button, item.Remarks);
            modeBar.Children.Add(button);
        }

        _syncingModes = false;
    }

    private void ModeChanged(object? sender, RoutedEventArgs e)
    {
        if (_syncingModes || ViewModel is null || sender is not ToggleButton { IsChecked: true, Tag: RoutingItem item })
        {
            return;
        }

        if (item.Id == ViewModel.SelectedRouting?.Id)
        {
            return;
        }

        ViewModel.SelectedRouting = item;
    }

    private async Task LoadRulesAsync(RoutingItem? routing)
    {
        RebuildModes();
        ClearEnabledWatch();
        if (routing is null || routing.Id.IsNullOrEmpty())
        {
            _rules = null;
            lstRules.ItemsSource = null;
            txtEmpty.IsVisible = true;
            return;
        }

        _rules = new RoutingRuleSettingViewModel(routing);
        var cards = new List<RoutingRuleCard>();
        var rules = JsonUtils.Deserialize<List<RulesItem>>(routing.RuleSet) ?? [];
        foreach (var rule in rules)
        {
            if (rule.Id.IsNullOrEmpty())
            {
                continue;
            }

            var (title, pattern) = Describe(rule);
            var card = new RoutingRuleCard
            {
                Id = rule.Id,
                Title = title,
                Pattern = pattern,
                Outbound = rule.OutboundTag ?? Global.ProxyTag,
                Enabled = rule.Enabled,
            };
            cards.Add(card);
            WatchEnabled(card);
        }

        _loadingCards = true;
        lstRules.ItemsSource = cards;
        txtEmpty.IsVisible = cards.Count == 0;
        _loadingCards = false;
        await Task.CompletedTask;
    }

    private void WatchEnabled(RoutingRuleCard card)
    {
        var sub = card.WhenAnyValue(x => x.Enabled)
            .Skip(1)
            .SubscribeAsync(async enabled =>
            {
                if (_loadingCards || _rules is null)
                {
                    return;
                }

                if (await _rules.SetRuleEnabledAsync(card.Id, enabled))
                {
                    StatusBarViewModel.Instance.ReloadRequested.Publish();
                }
            });
        _enabledSubscriptions[card.Id] = sub;
    }

    private void ClearEnabledWatch()
    {
        foreach (var sub in _enabledSubscriptions.Values)
        {
            sub.Dispose();
        }

        _enabledSubscriptions.Clear();
    }

    private async void Outbound_Click(object? sender, RoutedEventArgs e)
    {
        if (_rules is null || sender is not Button { DataContext: RoutingRuleCard card, Tag: string tag })
        {
            return;
        }

        if (await _rules.SetRuleOutboundAsync(card.Id, tag))
        {
            StatusBarViewModel.Instance.ReloadRequested.Publish();
            await LoadRulesAsync(ViewModel?.SelectedRouting);
        }
    }

    private async Task AddRuleAsync()
    {
        if (_rules is null)
        {
            return;
        }

        var before = _rules.RulesItems.Count;
        await _rules.RuleEditAsync(true);
        if (_rules.RulesItems.Count == before)
        {
            return;
        }

        if (await _rules.SaveRulesInPlaceAsync())
        {
            StatusBarViewModel.Instance.ReloadRequested.Publish();
            await LoadRulesAsync(ViewModel?.SelectedRouting);
        }
    }

    private static (string Title, string Pattern) Describe(RulesItem rule)
    {
        if (rule.Domain is { Count: > 0 })
        {
            return (ShellStrings.MatchDomain, string.Join(", ", rule.Domain));
        }

        if (rule.Ip is { Count: > 0 })
        {
            return (ShellStrings.MatchIp, string.Join(", ", rule.Ip));
        }

        if (rule.Process is { Count: > 0 })
        {
            return (ShellStrings.MatchProcess, string.Join(", ", rule.Process));
        }

        var pattern = rule.Remarks.IsNullOrEmpty() ? rule.OutboundTag ?? string.Empty : rule.Remarks;
        return (ShellStrings.MatchRule, pattern ?? string.Empty);
    }
}
