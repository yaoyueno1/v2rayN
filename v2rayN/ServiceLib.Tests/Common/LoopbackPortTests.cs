using System.Net;
using System.Net.Sockets;

namespace ServiceLib.Tests.Common;

public class LoopbackPortTests
{
    [Test]
    public async Task OccupiedPort_ShouldBeSkipped()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var occupied = ((IPEndPoint)listener.LocalEndpoint).Port;
        var reserved = 0;
        try
        {
            reserved = Utils.ReserveLoopbackPort(occupied);
            await (reserved > 0 && reserved != occupied).Should().BeTrue();
        }
        finally
        {
            listener.Stop();
            Utils.ReleaseLoopbackPort(reserved);
        }
    }

    [Test]
    public async Task SamePreferredPort_ShouldNotBeHandedOutTwice()
    {
        var first = Utils.ReserveLoopbackPort(28080);
        var second = Utils.ReserveLoopbackPort(28080);
        try
        {
            await (first > 0 && second > 0 && second != first).Should().BeTrue();
        }
        finally
        {
            Utils.ReleaseLoopbackPort(first);
            Utils.ReleaseLoopbackPort(second);
        }
    }

    [Test]
    public async Task ReleasedPort_CanBeReservedAgain()
    {
        var first = Utils.ReserveLoopbackPort(28180);
        Utils.ReleaseLoopbackPort(first);
        var second = Utils.ReserveLoopbackPort(first);
        try
        {
            await second.Should().BeEqualTo(first);
        }
        finally
        {
            Utils.ReleaseLoopbackPort(second);
        }
    }
}
