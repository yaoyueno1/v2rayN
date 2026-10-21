namespace v2rayN.Desktop.Common;

public static class AppBuilderExtension
{
    /// <summary>
    /// Latin uses Segoe UI. Chinese prefers Microsoft YaHei UI, then PingFang, then the
    /// OFL Noto Sans SC already shipped with the app. No commercial font files.
    /// </summary>
    private static readonly string EmbeddedCjkFamily =
        Global.AvaAssets + "Fonts#Noto Sans SC";

    private static readonly string DefaultFontFamilyName =
        "Segoe UI, Microsoft YaHei UI, Microsoft YaHei, PingFang SC, " + EmbeddedCjkFamily;

    public static AppBuilder WithFontByDefault(this AppBuilder appBuilder)
    {
        var fallbacks = new List<FontFallback>
        {
            new() { FontFamily = new FontFamily("Microsoft YaHei UI") },
            new() { FontFamily = new FontFamily("Microsoft YaHei") },
            new() { FontFamily = new FontFamily("PingFang SC") },
            new() { FontFamily = new FontFamily(EmbeddedCjkFamily) },
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
