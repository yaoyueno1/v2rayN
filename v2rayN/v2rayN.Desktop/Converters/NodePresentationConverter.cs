using Avalonia.Data;
using Avalonia.Data.Converters;
using v2rayN.Desktop.Common;

namespace v2rayN.Desktop.Converters;

public class RemarksToFlagConverter : IValueConverter
{
    private static readonly (string Key, string Flag)[] Flags =
    [
        ("香港", "🇭🇰"),
        ("hong kong", "🇭🇰"),
        ("台湾", "🇹🇼"),
        ("台灣", "🇹🇼"),
        ("taiwan", "🇹🇼"),
        ("日本", "🇯🇵"),
        ("japan", "🇯🇵"),
        ("新加坡", "🇸🇬"),
        ("singapore", "🇸🇬"),
        ("美国", "🇺🇸"),
        ("美國", "🇺🇸"),
        ("united states", "🇺🇸"),
        ("usa", "🇺🇸"),
        ("韩国", "🇰🇷"),
        ("韓國", "🇰🇷"),
        ("korea", "🇰🇷"),
        ("德国", "🇩🇪"),
        ("德國", "🇩🇪"),
        ("germany", "🇩🇪"),
        ("英国", "🇬🇧"),
        ("英國", "🇬🇧"),
        ("united kingdom", "🇬🇧"),
        ("澳大利亚", "🇦🇺"),
        ("澳洲", "🇦🇺"),
        ("australia", "🇦🇺"),
        ("加拿大", "🇨🇦"),
        ("canada", "🇨🇦"),
        ("法国", "🇫🇷"),
        ("法國", "🇫🇷"),
        ("france", "🇫🇷"),
        ("荷兰", "🇳🇱"),
        ("荷蘭", "🇳🇱"),
        ("netherlands", "🇳🇱"),
        ("俄罗斯", "🇷🇺"),
        ("俄羅斯", "🇷🇺"),
        ("russia", "🇷🇺"),
        ("印度", "🇮🇳"),
        ("india", "🇮🇳"),
        ("土耳其", "🇹🇷"),
        ("巴西", "🇧🇷"),
        ("brazil", "🇧🇷"),
    ];

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var remarks = value?.ToString() ?? string.Empty;
        foreach (var rune in remarks.EnumerateRunes())
        {
            if (rune.Value is >= 0x1F1E6 and <= 0x1F1FF)
            {
                return ExtractFlag(remarks);
            }
        }

        var lower = remarks.ToLowerInvariant();
        foreach (var (key, flag) in Flags)
        {
            if (lower.Contains(key, StringComparison.Ordinal))
            {
                return flag;
            }
        }

        return "🌐";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;

    private static string ExtractFlag(string remarks)
    {
        var buffer = new StringBuilder();
        var started = false;
        foreach (var rune in remarks.EnumerateRunes())
        {
            var isFlag = rune.Value is >= 0x1F1E6 and <= 0x1F1FF;
            if (isFlag)
            {
                started = true;
                buffer.Append(rune);
            }
            else if (started)
            {
                break;
            }
        }

        return buffer.Length > 0 ? buffer.ToString() : "🌐";
    }
}

public enum LatencyTier
{
    Untested,
    Fast,
    Mid,
    Slow,
    Pending,
    Skipped,
}

public static class LatencyFormat
{
    public const int FastMaxMs = 100;
    public const int MidMaxMs = 300;

    public static LatencyTier Classify(object? value, out int milliseconds)
    {
        milliseconds = 0;
        if (value is null or UnsetValueType or BindingNotification)
        {
            return LatencyTier.Untested;
        }

        var text = value.ToString()?.Trim();
        if (string.IsNullOrEmpty(text) || text == "—")
        {
            return LatencyTier.Untested;
        }

        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out milliseconds)
            || int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out milliseconds))
        {
            if (milliseconds <= 0)
            {
                return LatencyTier.Untested;
            }

            if (milliseconds <= FastMaxMs)
            {
                return LatencyTier.Fast;
            }

            if (milliseconds <= MidMaxMs)
            {
                return LatencyTier.Mid;
            }

            return LatencyTier.Slow;
        }

        if (Contains(text, "跳过", "跳過", "Skip"))
        {
            return LatencyTier.Skipped;
        }

        if (Contains(text, "失败", "失敗", "fail", "error", "超时", "超時", "timeout"))
        {
            return LatencyTier.Slow;
        }

        if (Contains(text, "测试", "測試", "Testing", "test"))
        {
            return LatencyTier.Pending;
        }

        return LatencyTier.Slow;
    }

    public static string Format(object? value)
    {
        var tier = Classify(value, out var milliseconds);
        return tier switch
        {
            LatencyTier.Untested => "—",
            LatencyTier.Fast or LatencyTier.Mid => $"{milliseconds} ms",
            LatencyTier.Slow when milliseconds > 0 => $"{milliseconds} ms",
            LatencyTier.Pending => ShellStrings.LatencyTesting,
            LatencyTier.Skipped => ShellStrings.LatencySkipped,
            _ => ShellStrings.LatencyFailed,
        };
    }

    public static string BrushKey(LatencyTier tier) => tier switch
    {
        LatencyTier.Fast => "color.latency.fast",
        LatencyTier.Mid => "color.latency.mid",
        LatencyTier.Slow => "color.latency.slow",
        _ => "color.text.tertiary",
    };

    private static bool Contains(string text, params string[] needles)
    {
        foreach (var needle in needles)
        {
            if (text.Contains(needle, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}

public class DelayTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        LatencyFormat.Format(value);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

public class DelayLatencyBrushConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        var sample = values.Count > 0 ? values[0] : null;
        var tier = LatencyFormat.Classify(sample, out _);
        return ResolveBrush(LatencyFormat.BrushKey(tier));
    }

    private static IBrush ResolveBrush(string key)
    {
        var app = Application.Current;
        if (app is not null)
        {
            foreach (var theme in new ThemeVariant?[] { app.ActualThemeVariant, app.RequestedThemeVariant, ThemeVariant.Light, ThemeVariant.Dark })
            {
                if (theme is not null && app.TryGetResource(key, theme, out var value) && value is IBrush brush)
                {
                    return brush;
                }
            }
        }

        return Brushes.Gray;
    }
}
