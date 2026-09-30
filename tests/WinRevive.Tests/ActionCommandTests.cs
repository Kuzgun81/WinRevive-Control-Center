using WinRevive.ViewModels;
using Xunit;

namespace WinRevive.Tests;

public sealed class ActionCommandTests
{
    [Fact]
    public void Execute_RunsProvidedAction()
    {
        var executed = false;
        var command = new ActionCommand(() => executed = true);

        command.Execute(null);

        Assert.True(executed);
    }
}
