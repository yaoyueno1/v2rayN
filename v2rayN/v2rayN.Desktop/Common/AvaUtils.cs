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

    public static WindowIcon GetAppIcon(ESysProxyType sysProxyType)
    {
        var index = (int)sysProxyType + 1;
        if (index is < 1 or > 4)
        {
            index = 1;
        }

        // A loose ico keeps every Windows icon size. A bitmap loaded from the stream and then disposed
        // leaves the tray with a blank icon, which is what happened after the shell refreshed TrayIcon.
        var fileName = Utils.GetPath($"NotifyIcon{index}.ico");
        if (File.Exists(fileName))
        {
            return new WindowIcon(fileName);
        }

        var uri = new Uri($"{Global.AvaAssets}NotifyIcon{index}.ico");
        var stream = AssetLoader.Open(uri);
        return new WindowIcon(stream);
    }
}
