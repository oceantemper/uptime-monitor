using UptimeMonitor.Domain;

namespace UptimeMonitor.Domain.Tests;

public class CheckOutcomeTests
{
    [Fact]
    public void Values_DeclaredOutcomes_AreSucceededAndFailed()
    {
        Assert.Equal(
            new[] { CheckOutcome.Succeeded, CheckOutcome.Failed },
            Enum.GetValues<CheckOutcome>());
    }
}