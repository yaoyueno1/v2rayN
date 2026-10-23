using System.Collections.Specialized;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using v2rayN.Desktop.Common;

namespace v2rayN.Desktop.Views;

public sealed class RoutingRuleCard : ReactiveObject
{
    private bool _enabled;
    private string _outboundLabel;

    public required string Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Pattern { get; init; } = string.Empty;
    public string OutboundKey { get; private set; } = Global.ProxyTag;

    public IReadOnlyList<string> OutboundChoices { get; } = ChoiceLabels;

    private static readonly string[] ChoiceLabels =
    [
        ShellStrings.OutboundProxy,
        ShellStrings.OutboundDirect,
        ShellStrings.OutboundBlock,
    ];

    public RoutingRuleCard()
    {
        _outboundLabel = ShellStrings.OutboundProxy;
    }

    public bool Enabled
    {
        get => _enabled;
        set => this.RaiseAndSetIfChanged(ref _enabled, value);
    }

    public string OutboundLabel
    {
        get => _outboundLabel;
        set
        {
            var key = KeyOf(value);
            if (key == OutboundKey && value == _outboundLabel)
            {
                return;
            }

            OutboundKey = key;
            this.RaiseAndSetIfChanged(ref _outboundLabel, LabelOf(key));
        }
    }

    public void SetOutbound(string? tag)
    {
        OutboundKey = KeyOf(tag);
        _outboundLabel = LabelOf(OutboundKey);
    }

    public static string KeyOf(string? label)
    {
        if (string.Equals(label, Global.DirectTag, StringComparison.OrdinalIgnoreCase)
            || label == ShellStrings.OutboundDirect)
        {
            return Global.DirectTag;
        }

        if (string.Equals(label, Global.BlockTag, StringComparison.OrdinalIgnoreCase)
            || label == ShellStrings.OutboundBlock)
        {
            return Global.BlockTag;
        }

        return Global.ProxyTag;
    }

    public string Label =>
        string.IsNullOrWhiteSpace(Pattern) || Pattern == Title ? Title : $"{Title}  {Pattern}";

    private static string LabelOf(string tag)
    {
        if (tag == Global.DirectTag)
        {
            return ShellStrings.OutboundDirect;
        }

        if (tag == Global.BlockTag)
        {
            return ShellStrings.OutboundBlock;
        }

        return ShellStrings.OutboundProxy;
    }
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
        btnAddFirst.Click += async (_, _) => await AddRuleAsync();
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
            button.Classes.Add("tab");
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
            SetEmpty(true);
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
                Enabled = rule.Enabled,
            };
            card.SetOutbound(rule.OutboundTag);
            cards.Add(card);
            WatchCard(card);
        }

        _loadingCards = true;
        lstRules.ItemsSource = cards;
        SetEmpty(cards.Count == 0);
        _loadingCards = false;
        await Task.CompletedTask;
    }

    private void WatchCard(RoutingRuleCard card)
    {
        var enabled = card.WhenAnyValue(x => x.Enabled)
            .Skip(1)
            .SubscribeAsync(async value =>
            {
                if (_loadingCards || _rules is null)
                {
                    return;
                }

                if (await _rules.SetRuleEnabledAsync(card.Id, value))
                {
                    StatusBarViewModel.Instance.ReloadRequested.Publish();
                }
            });
        var outbound = card.WhenAnyValue(x => x.OutboundLabel)
            .Skip(1)
            .SubscribeAsync(async _ =>
            {
                if (_loadingCards || _rules is null)
                {
                    return;
                }

                if (await _rules.SetRuleOutboundAsync(card.Id, card.OutboundKey))
                {
                    StatusBarViewModel.Instance.ReloadRequested.Publish();
                }
            });
        _enabledSubscriptions[card.Id] = new ActionDisposable(() =>
        {
            enabled.Dispose();
            outbound.Dispose();
        });
    }

    public void OpenFirstOutbound()
    {
        var combo = this.GetVisualDescendants().OfType<ComboBox>().FirstOrDefault(item => item.Classes.Contains("outbound"));
        if (combo is not null)
        {
            combo.IsDropDownOpen = true;
        }
    }

    public void CloseOutboundDropDown()
    {
        foreach (var combo in this.GetVisualDescendants().OfType<ComboBox>())
        {
            combo.IsDropDownOpen = false;
        }
    }

    private void SetEmpty(bool empty)
    {
        txtEmpty.IsVisible = empty;
        btnAddFirst.IsVisible = empty;
    }

    private void ClearEnabledWatch()
    {
        foreach (var sub in _enabledSubscriptions.Values)
        {
            sub.Dispose();
        }

        _enabledSubscriptions.Clear();
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
