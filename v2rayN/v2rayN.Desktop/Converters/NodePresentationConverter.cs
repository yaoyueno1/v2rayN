using Avalonia.Data.Converters;

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

public class DelayTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var text = value?.ToString();
        if (text.IsNullOrEmpty())
        {
            return "—";
        }

        if (int.TryParse(text, out var delay))
        {
            return delay > 0 ? $"{delay} ms" : "—";
        }

        return text;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}
