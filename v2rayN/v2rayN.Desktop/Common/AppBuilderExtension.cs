namespace v2rayN.Desktop.Common;

public static class AppBuilderExtension
{
    /// <summary>
    /// UI face is embedded MiSans (Xiaomi HyperOS). One family covers Latin and Chinese.
    /// System CJK faces stay behind it for glyphs the embedded cut does not include.
    /// </summary>
    private static readonly string EmbeddedUiFamily =
        Global.AvaAssets + "Fonts#MiSans";

    private static readonly string DefaultFontFamilyName = EmbeddedUiFamily;

    public static AppBuilder WithFontByDefault(this AppBuilder appBuilder)
    {
        var fallbacks = new List<FontFallback>
        {
            new() { FontFamily = new FontFamily("PingFang SC") },
            new() { FontFamily = new FontFamily("Microsoft YaHei UI") },
            new() { FontFamily = new FontFamily("Microsoft YaHei") },
        };

        if (OperatingSystem.IsWindows())
        {
            AddFontFallback(fallbacks, "Segoe UI Emoji");
            AddFontFallback(fallbacks, "Segoe UI Symbol");
        }
        else if (OperatingSystem.IsMacOS())
        {
            AddFontFallback(fallbacks, "Apple Color Emoji");
            AddFontFallback(fallbacks, "Apple Symbols");
        }
        else if (OperatingSystem.IsLinux())
        {
            AddFontFallback(fallbacks, "Noto Color Emoji");
            AddFontFallback(fallbacks, "Noto Sans Symbols");
        }

        return appBuilder.With(new FontManagerOptions
        {
            DefaultFamilyName = DefaultFontFamilyName,
            FontFallbacks = fallbacks.ToArray()
        });
    }

    private static void AddFontFallback(List<FontFallback> fallbacks, string fontFamilyName)
    {
        fallbacks.Add(new FontFallback
        {
            FontFamily = new FontFamily(fontFamilyName)
        });
    }
}
