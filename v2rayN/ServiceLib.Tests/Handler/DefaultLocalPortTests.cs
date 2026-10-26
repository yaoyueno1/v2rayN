namespace ServiceLib.Tests.Handler;

public class DefaultLocalPortTests
{
    [Test]
    public async Task NewInstall_DefaultsLocalPortTo20808()
    {
        var config = new Config();

        ConfigHandler.EnsureDefaultInbound(config);

        await config.Inbound.Should().NotBeNull();
        await config.Inbound[0].LocalPort.Should().BeEqualTo(20808);
        await Global.DefaultLocalPort.Should().BeEqualTo(20808);
    }

    [Test]
    public async Task ExistingLocalPort_IsNotReplaced()
    {
        var config = new Config
        {
            Inbound =
            [
                new InItem
                {
                    Protocol = nameof(EInboundProtocol.socks),
                    LocalPort = 10808,
                },
            ],
        };

        ConfigHandler.EnsureDefaultInbound(config);

        await config.Inbound[0].LocalPort.Should().BeEqualTo(10808);
    }
}
