namespace v2rayN.Desktop.Common;

/// <summary>
/// Chrome strings for the desktop shell.
/// Simplified Chinese is the default. Traditional Chinese and English follow the UI culture
/// so existing language packs are not replaced.
/// </summary>
public static class ShellStrings
{
    private static bool Zh =>
        CultureInfo.CurrentUICulture.Name.StartsWith("zh", StringComparison.OrdinalIgnoreCase);

    private static bool Hant =>
        CultureInfo.CurrentUICulture.Name.Contains("Hant", StringComparison.OrdinalIgnoreCase)
        || CultureInfo.CurrentUICulture.Name.Contains("TW", StringComparison.OrdinalIgnoreCase)
        || CultureInfo.CurrentUICulture.Name.Contains("HK", StringComparison.OrdinalIgnoreCase);

    private static string T(string hans, string hant, string en) => !Zh ? en : Hant ? hant : hans;

    public static string NavHome => T("节点", "節點", "Nodes");
    public static string NavProfiles => T("配置项", "設定項", "Profiles");
    public static string NavSubscriptions => T("订阅分组", "訂閱分組", "Subscriptions");
    public static string NavRouting => T("路由", "路由", "Routing");
    public static string NavSettings => T("设置", "設定", "Settings");
    public static string NavHelp => T("帮助", "說明", "Help");
    public static string NavAbout => T("关于", "關於", "About");
    public static string SectionActions => T("操作", "操作", "Actions");

    public static string SearchNodes => T("搜索节点...", "搜尋節點...", "Search nodes...");
    public static string SearchShortcut => OperatingSystem.IsMacOS() ? "⌘K" : "Ctrl+K";
    public static string Connect => T("连接", "連線", "Connect");
    public static string Disconnect => T("断开", "斷開", "Disconnect");
    public static string More => T("更多", "更多", "More");

    public static string NodeList => T("节点列表", "節點列表", "Nodes");
    public static string NodesAvailable(int count) => T($"{count} 个节点可用", $"{count} 個節點可用", $"{count} nodes available");
    public static string RefreshJustNow => T("刚刚刷新", "剛剛重新整理", "Just now");
    public static string NotTestedYet => T("尚未测速", "尚未測速", "Not tested yet");
    public static string LatencyFailed => T("失败", "失敗", "Failed");
    public static string LatencyTesting => T("测试中", "測試中", "Testing");
    public static string LatencySkipped => T("跳过", "跳過", "Skipped");

    public static string ConnectionStatus => T("连接状态", "連線狀態", "Status");
    public static string Connected => T("已连接", "已連線", "Connected");
    public static string Disconnected => T("未连接", "未連線", "Disconnected");
    public static string Since(string time) => T($"自 {time}", $"自 {time}", $"Since {time}");
    public static string Upload => T("上传", "上傳", "Upload");
    public static string Download => T("下载", "下載", "Download");
    public static string OutboundMode => T("出站模式", "出站模式", "Outbound");
    public static string SystemProxy => T("系统代理", "系統代理", "System proxy");
    public static string Tun => "TUN";
    public static string Pac => "PAC";
    public static string ViewDetails => T("查看连接详情", "查看連線詳情", "Connection details");
    public static string DetailsTitle => T("连接详情", "連線詳情", "Connection details");
    public static string Back => T("返回", "返回", "Back");

    public static string AddSubscription => T("添加订阅", "新增訂閱", "Add subscription");
    public static string RefreshAll => T("全部刷新", "全部重新整理", "Refresh all");
    public static string Update => T("更新", "更新", "Update");
    public static string Edit => T("编辑", "編輯", "Edit");
    public static string NodeCount(int count) => T($"{count} 个节点", $"{count} 個節點", $"{count} nodes");
    public static string NotUpdated => T("尚未更新", "尚未更新", "Not updated");
    public static string EmptySubscriptions => T("还没有订阅", "還沒有訂閱", "No subscriptions yet");

    public static string AddRule => T("添加规则", "新增規則", "Add rule");
    public static string Enabled => T("已启用", "已啟用", "Enabled");
    public static string MatchDomain => T("匹配域名", "比對網域", "Domain");
    public static string MatchIp => T("匹配 IP", "比對 IP", "IP");
    public static string MatchProcess => T("匹配进程", "比對行程", "Process");
    public static string MatchRule => T("规则", "規則", "Rule");
    public static string ModeGlobal => T("全局", "全域", "Global");
    public static string ModeRules => T("规则", "規則", "Rules");
    public static string ModeDirect => T("直连", "直連", "Direct");
    public static string ModeBlock => T("黑名单", "黑名單", "Blacklist");
    public static string OutboundProxy => T("代理", "代理", "Proxy");
    public static string OutboundDirect => T("直连", "直連", "Direct");
    public static string OutboundBlock => T("阻止", "封鎖", "Block");
    public static string EmptyRules => T("暂无规则", "暫無規則", "No rules yet");
    public static string AddFirstRule => T("添加第一条", "新增第一條", "Add the first rule");
    public static string AdvancedRouting => T("高级路由", "進階路由", "Advanced routing");

    public static string SettingsTitle => T("设置", "設定", "Settings");
    public static string SettingsCore => T("参数设置", "參數設定", "Options");
    public static string SettingsDns => T("DNS 设置", "DNS 設定", "DNS");
    public static string SettingsTemplate => T("完整配置模板", "完整設定模板", "Full config template");
    public static string SettingsHotkey => T("全局热键", "全域熱鍵", "Hotkeys");
    public static string SettingsBackup => T("备份与恢复", "備份與還原", "Backup and restore");
    public static string SettingsFolder => T("打开配置目录", "開啟設定目錄", "Open config folder");
    public static string SettingsClearStats => T("清空统计", "清空統計", "Clear statistics");
    public static string SettingsReload => T("重启服务", "重新啟動服務", "Reload service");
    public static string SettingsUpdate => T("检查更新", "檢查更新", "Check for updates");
    public static string SettingsPresetDefault => T("默认", "預設", "Default");
    public static string SettingsPresetRussia => T("俄罗斯", "俄羅斯", "Russia");
    public static string SettingsPresetIran => T("伊朗", "伊朗", "Iran");
    public static string SettingsGroupCore => T("核心与网络", "核心與網路", "Core and network");
    public static string SettingsGroupData => T("数据与维护", "資料與維護", "Data and maintenance");
    public static string SettingsGroupRegion => T("区域预设", "區域預設", "Regional presets");
    public static string ClearStatsConfirm => T(
        "确定清空全部节点统计？此操作不可恢复。",
        "確定清空全部節點統計？此操作無法還原。",
        "Clear all node statistics? This cannot be undone.");
    public static string SettingsRebootAdmin => T("以管理员重启", "以系統管理員重新啟動", "Restart as administrator");
    public static string SettingsUwp => T("UWP 回环解锁", "UWP 回環解鎖", "UWP loopback unlock");
    public static string Appearance => T("外观", "外觀", "Appearance");
    public static string LanguageLabel => T("语言", "語言", "Language");
    public static string RestartHint => T("(需重启)", "(需重新啟動)", "(restart required)");
    public static string ThemeFollowSystem => T("跟随系统", "跟隨系統", "Follow system");
    public static string ThemeLight => T("浅色", "淺色", "Light");
    public static string ThemeDark => T("深色", "深色", "Dark");
    public static string ThemeAquatic => T("水色", "水色", "Aquatic");
    public static string ThemeDesert => T("沙色", "沙色", "Desert");
    public static string ThemeDusk => T("暮色", "暮色", "Dusk");
    public static string ThemeNightSky => T("夜空", "夜空", "Night sky");
    public static string FontSizeDefault => T("默认", "預設", "Default");
    public static string ProxyConnection => T("代理连接", "代理連線", "Proxy");
    public static string TrayCopyProxy => T("复制终端代理命令", "複製終端代理命令", "Copy terminal proxy command");
    public static string TrayImport => T("剪贴板导入", "剪貼簿匯入", "Import from clipboard");
    public static string TrayUpdate => T("更新订阅", "更新訂閱", "Update subscriptions");
    public static string TrayUpdateViaProxy => T("更新订阅（通过代理）", "更新訂閱（透過代理）", "Update subscriptions via proxy");
    public static string TrayPac => T("PAC 模式", "PAC 模式", "PAC mode");
    public static string HelpTitle => T("帮助", "說明", "Help");
    public static string HelpSubtitle => T("文档与项目主页", "文件與專案主頁", "Docs and project pages");
    public static string AddProfileTitle => T("添加配置", "新增設定", "Add profile");

    public static string AboutTitle => T("关于 v2rayN", "關於 v2rayN", "About v2rayN");
    public static string AboutBody => T(
        "跨平台桌面客户端。界面是暖色纸张底、圆角内容区和左侧导航，浅色与深色都可用。",
        "跨平台桌面客戶端。介面是暖色紙張底、圓角內容區和左側導覽，淺色與深色都可用。",
        "Cross-platform desktop client. Warm paper background, rounded content, and a left sidebar, in light and dark.");

    public static string FontCredit => T(
        "界面字体 MiSans",
        "介面字型 MiSans",
        "Interface typeface: MiSans");

    public static string FormatUpdated(long unixSeconds)
    {
        if (unixSeconds <= 0)
        {
            return NotUpdated;
        }

        var when = DateTimeOffset.FromUnixTimeSeconds(unixSeconds).LocalDateTime;
        var span = DateTime.Now - when;
        if (span.TotalMinutes < 1)
        {
            return T("最后更新：刚刚", "最後更新：剛剛", "Updated just now");
        }

        if (span.TotalMinutes < 60)
        {
            var minutes = (int)span.TotalMinutes;
            return T($"最后更新：{minutes} 分钟前", $"最後更新：{minutes} 分鐘前", $"Updated {minutes} min ago");
        }

        if (span.TotalHours < 24)
        {
            var hours = (int)span.TotalHours;
            return T($"最后更新：{hours} 小时前", $"最後更新：{hours} 小時前", $"Updated {hours} h ago");
        }

        var days = Math.Max(1, (int)span.TotalDays);
        return T($"最后更新：{days} 天前", $"最後更新：{days} 天前", $"Updated {days} d ago");
    }

    public static string ShortRoutingName(string? remarks)
    {
        var text = remarks ?? string.Empty;
        if (text.Contains("全局", StringComparison.OrdinalIgnoreCase) || text.Contains("Global", StringComparison.OrdinalIgnoreCase))
        {
            return ModeGlobal;
        }

        if (text.Contains("绕过", StringComparison.Ordinal) || text.Contains("Whitelist", StringComparison.OrdinalIgnoreCase)
            || text.Contains("规则", StringComparison.Ordinal))
        {
            return ModeRules;
        }

        if (text.Contains("直连", StringComparison.Ordinal) || text.Contains("Direct", StringComparison.OrdinalIgnoreCase))
        {
            return ModeDirect;
        }

        if (text.Contains("黑名单", StringComparison.Ordinal) || text.Contains("Blacklist", StringComparison.OrdinalIgnoreCase))
        {
            return ModeBlock;
        }

        return text;
    }
}
