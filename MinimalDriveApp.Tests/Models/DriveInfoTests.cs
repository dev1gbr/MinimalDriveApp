using MinimalDriveApp.Models;

namespace MinimalDriveApp.Tests.Models;

public class DriveInfoTests
{
    [Fact]
    public void IsOutdatedBackup_IsTrue_WhenLastUpdatedNewerThanLastBackedUp()
    {
        var drive = new DriveInfo
        {
            LastUpdated = new DateTime(2026, 1, 2),
            LastBackedUp = new DateTime(2026, 1, 1)
        };

        Assert.True(drive.IsOutdatedBackup);
    }

    [Fact]
    public void IsOutdatedBackup_IsFalse_WhenLastBackedUpNewerThanLastUpdated()
    {
        var drive = new DriveInfo
        {
            LastUpdated = new DateTime(2026, 1, 1),
            LastBackedUp = new DateTime(2026, 1, 2)
        };

        Assert.False(drive.IsOutdatedBackup);
    }

    [Fact]
    public void IsOutdatedBackup_IsFalse_WhenEitherDateIsNull()
    {
        Assert.False(new DriveInfo { LastUpdated = DateTime.Now, LastBackedUp = null }.IsOutdatedBackup);
        Assert.False(new DriveInfo { LastUpdated = null, LastBackedUp = DateTime.Now }.IsOutdatedBackup);
        Assert.False(new DriveInfo { LastUpdated = null, LastBackedUp = null }.IsOutdatedBackup);
    }

    [Theory]
    [InlineData("Degraded", true)]
    [InlineData("Unknown", true)]
    [InlineData("Pred Fail", true)]
    [InlineData("Error", true)]
    [InlineData("Starting", true)]
    [InlineData("Stopping", true)]
    [InlineData("OK", false)]
    [InlineData("", false)]
    public void IsHealthWarning_ReturnsExpected_ForKnownStatuses(string status, bool expected)
    {
        var drive = new DriveInfo { HealthStatus = status };
        Assert.Equal(expected, drive.IsHealthWarning);
    }

    [Fact]
    public void UsedSpace_IsCapacityMinusFreeSpace()
    {
        var drive = new DriveInfo { Capacity = 1_000_000, FreeSpace = 400_000, UsedSpace = 600_000 };
        Assert.Equal(drive.Capacity - drive.FreeSpace, drive.UsedSpace);
    }
}
