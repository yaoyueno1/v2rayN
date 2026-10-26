namespace ServiceLib.Tests.Handler;

public class CloseToTrayTests
{
    [Test]
    public async Task NewConfig_ClosesTheMainWindowToTheTray()
    {
        await new UIItem().ShouldCloseMainWindowToTray().Should().BeTrue();
    }

    [Test]
    public async Task ExistingConfigWithoutTheChoice_StillClosesToTheTray()
    {
        var ui = JsonUtils.Deserialize<UIItem>("""{"Hide2TrayWhenClose":false}""");

        await ui!.ShouldCloseMainWindowToTray().Should().BeTrue();
        await ui.CloseMainWindowToTray.Should().BeNull();
    }

    [Test]
    public async Task ExplicitExit_KeepsClosingTheProcess()
    {
        var ui = new UIItem { CloseMainWindowToTray = false };

        await ui.ShouldCloseMainWindowToTray().Should().BeFalse();
    }
}
