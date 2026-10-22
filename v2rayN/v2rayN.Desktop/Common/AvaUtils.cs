using Avalonia.Input.Platform;

namespace v2rayN.Desktop.Common;

internal class AvaUtils
{
    public static async Task<string?> GetClipboardData(Window owner)
    {
        try
        {
            var clipboard = TopLevel.GetTopLevel(owner)?.Clipboard;
            if (clipboard == null)
            {
                return null;
            }

            return await clipboard.TryGetTextAsync();
        }
        catch
        {
            return null;
        }
    }

    public static async Task SetClipboardData(Visual? visual, string strData)
    {
        try
        {
            var clipboard = TopLevel.GetTopLevel(visual)?.Clipboard;
            if (clipboard == null)
            {
                return;
            }

            await clipboard.SetTextAsync(strData);
        }
        catch
        {
        }
    }

    private static readonly WindowIcon?[] TrayIcons = new WindowIcon?[5];

    public static WindowIcon GetAppIcon(ESysProxyType sysProxyType)
    {
        var index = (int)sysProxyType + 1;
        if (index is < 1 or > 4)
        {
            index = 1;
        }

        return TrayIcons[index] ??= LoadTrayIcon(index);
    }

    public static void ApplyTrayIcon(ESysProxyType sysProxyType)
    {
        var icon = GetAppIcon(sysProxyType);
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        if (desktop.MainWindow is { } window)
        {
            window.Icon = icon;
        }

        var icons = TrayIcon.GetIcons(Application.Current);
        if (icons is { Count: > 0 })
        {
            icons[0].Icon = icon;
            icons[0].IsVisible = true;
        }
    }

    private static WindowIcon LoadTrayIcon(int index)
    {
        var name = $"NotifyIcon{index}.ico";
        foreach (var path in TrayIconPaths(name))
        {
            if (File.Exists(path))
            {
                // File-based WindowIcon keeps the Win32 HICON alive. Do not dispose it while the tray exists.
                return new WindowIcon(path);
            }
        }

        var uri = new Uri($"{Global.AvaAssets}{name}");
        using var asset = AssetLoader.Open(uri);
        var copy = new MemoryStream();
        asset.CopyTo(copy);
        copy.Position = 0;
        return new WindowIcon(copy);
    }

    private static IEnumerable<string> TrayIconPaths(string name)
    {
        if (!string.IsNullOrEmpty(AppContext.BaseDirectory))
        {
            yield return Path.Combine(AppContext.BaseDirectory, name);
        }

        var processDir = Path.GetDirectoryName(Environment.ProcessPath);
        if (!string.IsNullOrEmpty(processDir))
        {
            yield return Path.Combine(processDir, name);
        }

        yield return Utils.GetPath(name);
    }
}
