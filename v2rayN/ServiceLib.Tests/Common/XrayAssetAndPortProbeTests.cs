using System.Globalization;

namespace ServiceLib.Tests.Common;

public class XrayAssetAndPortProbeTests
{
    [Test]
    public async Task MissingExeGeo_IsCopiedBesideXray()
    {
        var root = Path.Combine(Path.GetTempPath(), "xray-geo-" + Guid.NewGuid().ToString("n"));
        var bin = Path.Combine(root, "bin");
        var exeDir = Path.Combine(root, "bin", "xray");
        Directory.CreateDirectory(bin);
        Directory.CreateDirectory(exeDir);
        await File.WriteAllTextAsync(Path.Combine(bin, "geoip.dat"), "geoip");
        await File.WriteAllTextAsync(Path.Combine(bin, "geosite.dat"), "geosite");

        var assetDir = Utils.AlignXrayAssetDirectory(Path.Combine(exeDir, "xray.exe"), bin);
        var geosite = await File.ReadAllTextAsync(Path.Combine(exeDir, "geosite.dat"));
        var geoip = await File.ReadAllTextAsync(Path.Combine(exeDir, "geoip.dat"));

        await assetDir.Should().BeEqualTo(exeDir);
        await geosite.Should().BeEqualTo("geosite");
        await geoip.Should().BeEqualTo("geoip");
    }

    [Test]
    public async Task ExeOnlyGeo_IsMirroredBackToBin()
    {
        var root = Path.Combine(Path.GetTempPath(), "xray-geo-" + Guid.NewGuid().ToString("n"));
        var bin = Path.Combine(root, "bin");
        var exeDir = Path.Combine(root, "bin", "xray");
        Directory.CreateDirectory(exeDir);
        await File.WriteAllTextAsync(Path.Combine(exeDir, "geoip.dat"), "geoip");
        await File.WriteAllTextAsync(Path.Combine(exeDir, "geosite.dat"), "geosite");

        var assetDir = Utils.AlignXrayAssetDirectory(Path.Combine(exeDir, "xray.exe"), bin);
        var geosite = await File.ReadAllTextAsync(Path.Combine(bin, "geosite.dat"));

        await assetDir.Should().BeEqualTo(exeDir);
        await geosite.Should().BeEqualTo("geosite");
    }

    [Test]
    public async Task NewerBinGeo_ReplacesExeCopy()
    {
        var root = Path.Combine(Path.GetTempPath(), "xray-geo-" + Guid.NewGuid().ToString("n"));
        var bin = Path.Combine(root, "bin");
        var exeDir = Path.Combine(root, "bin", "xray");
        Directory.CreateDirectory(exeDir);
        var exeGeo = Path.Combine(exeDir, "geosite.dat");
        var binGeo = Path.Combine(bin, "geosite.dat");
        Directory.CreateDirectory(bin);
        await File.WriteAllTextAsync(exeGeo, "old");
        await File.WriteAllTextAsync(Path.Combine(exeDir, "geoip.dat"), "geoip");
        await File.WriteAllTextAsync(binGeo, "new-geosite");
        await File.WriteAllTextAsync(Path.Combine(bin, "geoip.dat"), "geoip");
        File.SetLastWriteTimeUtc(exeGeo, DateTime.UtcNow.AddMinutes(-5));
        File.SetLastWriteTimeUtc(binGeo, DateTime.UtcNow);

        Utils.AlignXrayAssetDirectory(Path.Combine(exeDir, "xray.exe"), bin);
        var geosite = await File.ReadAllTextAsync(exeGeo);

        await geosite.Should().BeEqualTo("new-geosite");
    }

    [Test]
    public async Task ExcludedRange_MatchesConfiguredPort()
    {
        const string output = """
            协议 tcp 端口排除范围

            开始端口    结束端口
            ----------    --------
                 10790       10889
                 50000       50059
            """;

        var hit = Utils.FindExcludedPortRange(10808, output);
        var miss = Utils.FindExcludedPortRange(20808, output);

        await hit.Should().BeEqualTo(new Utils.PortRange(10790, 10889));
        await miss.Should().BeNull();
    }

    [Test]
    public async Task UnboundPortMessage_KeepsTheConfiguredPort()
    {
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("zh-Hans");
            var text = Utils.DescribeUnboundLocalPort(10808, new Utils.PortRange(10790, 10889));
            await text.Should().Contain("10808");
            await text.Should().Contain("10790");
            await text.Should().Contain("10889");
            await text.Should().Contain("配置未修改");
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Test]
    public async Task BindProbe_DoesNotReserveThePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var occupied = ((IPEndPoint)listener.LocalEndpoint).Port;
        var reserved = 0;
        try
        {
            await Utils.CanBindLoopbackPort(occupied).Should().BeFalse();
            listener.Stop();
            await Utils.CanBindLoopbackPort(occupied).Should().BeTrue();
            reserved = Utils.ReserveLoopbackPort(occupied);
            await reserved.Should().BeEqualTo(occupied);
        }
        finally
        {
            try
            {
                listener.Stop();
            }
            catch
            {
                // already stopped
            }

            Utils.ReleaseLoopbackPort(reserved);
        }
    }
}
