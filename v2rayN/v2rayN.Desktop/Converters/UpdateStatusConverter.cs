using System.Text.RegularExpressions;
using Avalonia.Data.Converters;
using v2rayN.Desktop.Common;

namespace v2rayN.Desktop.Converters;

/// <summary>
/// Maps a core-update remark onto the paper status line.
/// ConverterParameter new|latest|checking|plain returns visibility; otherwise the display text.
/// </summary>
public class UpdateStatusConverter : IValueConverter
{
    private static readonly Regex VersionToken = new(@"v?\d+\.\d+(?:\.\d+)*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var remarks = value as string ?? string.Empty;
        var kind = Classify(remarks);
        var mode = parameter as string;
        if (mode is "new" or "latest" or "checking" or "plain")
        {
            return kind == mode;
        }

        return Display(remarks, kind);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    private static string Classify(string remarks)
    {
        if (remarks is "..." or "…" or "."
            || remarks.Contains("检查中", StringComparison.Ordinal)
            || remarks.Contains("檢查中", StringComparison.Ordinal)
            || remarks.Contains("Checking", StringComparison.OrdinalIgnoreCase))
        {
            return "checking";
        }

        if (remarks.Contains("有新版本", StringComparison.Ordinal)
            || remarks.Contains("new version", StringComparison.OrdinalIgnoreCase)
            || remarks.Contains("nouvelle version", StringComparison.OrdinalIgnoreCase)
            || remarks.Contains("новая версия", StringComparison.OrdinalIgnoreCase))
        {
            return "new";
        }

        if (remarks.Contains("已是最新", StringComparison.Ordinal)
            || remarks.Contains("up to date", StringComparison.OrdinalIgnoreCase)
            || remarks.Contains("à jour", StringComparison.OrdinalIgnoreCase)
            || remarks.Contains("последн", StringComparison.OrdinalIgnoreCase))
        {
            return "latest";
        }

        return "plain";
    }

    private static string Display(string remarks, string kind)
    {
        switch (kind)
        {
            case "checking":
                return ShellStrings.UpdateChecking;
            case "new":
            {
                var version = AfterColon(remarks);
                return version.Length == 0 ? ShellStrings.UpdateAvailable(string.Empty).Trim() : ShellStrings.UpdateAvailable(version);
            }
            case "latest":
            {
                var version = LastVersion(remarks);
                return version.Length == 0 ? ShellStrings.UpdateLatest(string.Empty).Trim() : ShellStrings.UpdateLatest(version);
            }
            default:
                return remarks;
        }
    }

    private static string AfterColon(string remarks)
    {
        var idx = remarks.LastIndexOf('：');
        if (idx < 0)
        {
            idx = remarks.LastIndexOf(':');
        }

        if (idx < 0 || idx >= remarks.Length - 1)
        {
            return string.Empty;
        }

        return remarks[(idx + 1)..].Trim().TrimEnd('。', '.', ' ');
    }

    private static string LastVersion(string remarks)
    {
        var matches = VersionToken.Matches(remarks);
        if (matches.Count == 0)
        {
            return string.Empty;
        }

        var token = matches[^1].Value;
        if (token.Length == 0)
        {
            return string.Empty;
        }

        return token[0] is 'v' or 'V' ? token : "v" + token;
    }
}
