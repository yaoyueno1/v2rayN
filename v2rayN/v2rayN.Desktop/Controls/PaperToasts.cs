using System.Text.RegularExpressions;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace v2rayN.Desktop.Controls;

/// <summary>
/// In-window paper toasts. Replaces the system notification window.
/// </summary>
public static class PaperToasts
{
    private static readonly Regex DelaySuffix = new(
        @"^(?<title>.*\S)\s+(?<ms>\d+\s*ms)\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static void Clear(Panel host)
    {
        host.Children.Clear();
    }

    public static void Show(Panel host, string? content, bool pin = false)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        var (title, subtitle, node) = Split(content);
        var card = Build(title, subtitle, node);
        host.Children.Add(card);
        while (host.Children.Count > 3)
        {
            host.Children.RemoveAt(0);
        }

        if (pin)
        {
            return;
        }

        var entry = new ToastLife(card, node ? TimeSpan.FromSeconds(4) : TimeSpan.FromSeconds(2.5));
        card.PointerEntered += (_, _) => entry.Pause();
        card.PointerExited += (_, _) => entry.Resume();
        entry.Elapsed += (_, _) =>
        {
            if (host.Children.Contains(card))
            {
                host.Children.Remove(card);
            }
        };
        var close = card.GetVisualDescendants().OfType<Button>().FirstOrDefault();
        if (close is not null)
        {
            close.Click += (_, _) =>
            {
                entry.Stop();
                if (host.Children.Contains(card))
                {
                    host.Children.Remove(card);
                }
            };
        }

        entry.Start();
    }

    private static (string Title, string? Subtitle, bool Node) Split(string content)
    {
        var text = content.Trim().Replace("\r\n", "\n");
        string? subtitle = null;
        var lines = text.Split('\n', 2, StringSplitOptions.TrimEntries);
        if (lines.Length == 2 && lines[1].Length > 0)
        {
            text = lines[0];
            subtitle = lines[1];
        }
        else
        {
            var match = DelaySuffix.Match(text);
            if (match.Success)
            {
                text = match.Groups["title"].Value;
                subtitle = match.Groups["ms"].Value;
            }
        }

        var node = text.StartsWith('[') && text.Contains(']');
        return (text, subtitle, node);
    }

    private static Border Build(string title, string? subtitle, bool node)
    {
        var bar = new Border { Width = node ? 3 : 2 };
        bar.Classes.Add("toast-bar");

        var titleBlock = new TextBlock { Text = title };
        titleBlock.Classes.Add("toast-title");

        var body = new StackPanel { Spacing = 2, Margin = new Thickness(12, 12, 32, 12) };
        body.Children.Add(titleBlock);
        if (!string.IsNullOrWhiteSpace(subtitle))
        {
            var sub = new TextBlock { Text = subtitle };
            sub.Classes.Add("toast-sub");
            body.Children.Add(sub);
        }

        var row = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*") };
        Grid.SetColumn(bar, 0);
        Grid.SetColumn(body, 1);
        row.Children.Add(bar);
        row.Children.Add(body);

        var close = new Button
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 6, 6, 0),
            Content = new PathIcon
            {
                Width = 12,
                Height = 12,
                Data = CloseIcon(),
            },
        };
        close.Classes.Add("toast-close");

        var layer = new Grid();
        layer.Children.Add(row);
        layer.Children.Add(close);

        var card = new Border { Child = layer, MaxWidth = 320 };
        card.Classes.Add("paper-toast");
        if (!node)
        {
            card.Classes.Add("status");
        }

        return card;
    }

    private static Geometry? CloseIcon()
    {
        if (Application.Current?.TryGetResource("SemiIconClose", null, out var value) == true && value is Geometry geometry)
        {
            return geometry;
        }

        return null;
    }

    private sealed class ToastLife
    {
        private readonly DispatcherTimer _timer = new();
        private TimeSpan _remaining;
        private DateTime _started;
        private bool _paused;

        public ToastLife(Control card, TimeSpan duration)
        {
            _remaining = duration;
            _timer.Tick += (_, _) =>
            {
                _timer.Stop();
                Elapsed?.Invoke(card, EventArgs.Empty);
            };
        }

        public event EventHandler? Elapsed;

        public void Start()
        {
            _paused = false;
            _started = DateTime.UtcNow;
            _timer.Interval = _remaining;
            _timer.Start();
        }

        public void Pause()
        {
            if (_paused || !_timer.IsEnabled)
            {
                return;
            }

            _timer.Stop();
            var left = _remaining - (DateTime.UtcNow - _started);
            _remaining = left < TimeSpan.FromMilliseconds(400) ? TimeSpan.FromMilliseconds(400) : left;
            _paused = true;
        }

        public void Resume()
        {
            if (!_paused)
            {
                return;
            }

            Start();
        }

        public void Stop()
        {
            _timer.Stop();
            _paused = true;
        }
    }
}
