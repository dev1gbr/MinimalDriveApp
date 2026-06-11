using MinimalDriveApp.Models;
using MinimalDriveApp.ViewModels;
using DriveInfo = MinimalDriveApp.Models.DriveInfo;

namespace MinimalDriveApp.Tests.ViewModels;

public class DriveDashboardViewModelTests
{
    private static DriveInfo MakeDrive(long capacity = 1_073_741_824L, long freeSpace = 536_870_912L) => new()
    {
        SerialNumber  = "SN001",
        DriveLetter   = "E:",
        VolumeName    = "Backup",
        DriveType     = "Removable",
        FileSystem    = "NTFS",
        ConnectionType = "USB",
        HealthStatus  = "OK",
        Capacity      = capacity,
        FreeSpace     = freeSpace,
        UsedSpace     = capacity - freeSpace,
        Status        = DriveStatus.KnownNamedDrive
    };

    // ── HasDrive ────────────────────────────────────────────────────────────

    [Fact]
    public void HasDrive_WhenDriveIsNull_ReturnsFalse()
    {
        var vm = new DriveDashboardViewModel();
        Assert.False(vm.HasDrive);
    }

    [Fact]
    public void HasDrive_WhenDriveIsSet_ReturnsTrue()
    {
        var vm = new DriveDashboardViewModel { Drive = MakeDrive() };
        Assert.True(vm.HasDrive);
    }

    // ── HeaderTitle ──────────────────────────────────────────────────────────

    [Fact]
    public void HeaderTitle_WithVolumeName_IncludesBothNameAndLetter()
    {
        var vm = new DriveDashboardViewModel { Drive = MakeDrive() };
        Assert.Equal("Backup (E:)", vm.HeaderTitle);
    }

    [Fact]
    public void HeaderTitle_WithoutVolumeName_ShowsDriveLetter()
    {
        var drive = MakeDrive();
        drive.VolumeName = string.Empty;
        var vm = new DriveDashboardViewModel { Drive = drive };
        Assert.Equal("E:", vm.HeaderTitle);
    }

    // ── StatusLabel ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData(DriveStatus.KnownNamedDrive,       "Known Named Drive")]
    [InlineData(DriveStatus.PreviouslySeenUnnamed, "Previously Seen Unnamed")]
    [InlineData(DriveStatus.BrandNewNeverSeen,     "Brand New — Never Seen")]
    public void StatusLabel_MapsEnumToFriendlyString(DriveStatus status, string expected)
    {
        var drive = MakeDrive();
        drive.Status = status;
        var vm = new DriveDashboardViewModel { Drive = drive };
        Assert.Equal(expected, vm.StatusLabel);
    }

    // ── UsedPercentFormatted ─────────────────────────────────────────────────

    [Fact]
    public void UsedPercentFormatted_HalfUsed_Returns50Percent()
    {
        var vm = new DriveDashboardViewModel { Drive = MakeDrive(1_073_741_824L, 536_870_912L) };
        Assert.Equal("50%", vm.UsedPercentFormatted);
    }

    [Fact]
    public void UsedPercentFormatted_WhenNoDrive_ReturnsDash()
    {
        var vm = new DriveDashboardViewModel();
        Assert.Equal("—", vm.UsedPercentFormatted);
    }

    [Fact]
    public void UsedPercentFormatted_ZeroCapacity_ReturnsDash()
    {
        var vm = new DriveDashboardViewModel { Drive = MakeDrive(0, 0) };
        Assert.Equal("—", vm.UsedPercentFormatted);
    }

    // ── FormatBytes ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData(1_099_511_627_776L, "1.0 TB")]
    [InlineData(1_073_741_824L,     "1.0 GB")]
    [InlineData(1_048_576L,         "1.0 MB")]
    [InlineData(1024L,              "1.0 KB")]
    [InlineData(0L,                 "0 GB")]
    public void FormatBytes_VariousSizes_CorrectUnit(long bytes, string expected)
    {
        Assert.Equal(expected, DriveDashboardViewModel.FormatBytes(bytes));
    }

    [Fact]
    public void FormatBytes_UsesInvariantCulture()
    {
        // 1.5 GB — must use '.' as decimal separator regardless of system locale
        long bytes = (long)(1.5 * 1_073_741_824);
        string result = DriveDashboardViewModel.FormatBytes(bytes);
        Assert.Contains('.', result);
    }

    // ── IsHealthOk ───────────────────────────────────────────────────────────

    [Fact]
    public void IsHealthOk_OkStatus_ReturnsTrue()
    {
        var drive = MakeDrive();
        drive.HealthStatus = "OK";
        Assert.True(new DriveDashboardViewModel { Drive = drive }.IsHealthOk);
    }

    [Theory]
    [InlineData("Degraded")]
    [InlineData("Unknown")]
    [InlineData("Pred Fail")]
    public void IsHealthOk_WarningStatus_ReturnsFalse(string status)
    {
        var drive = MakeDrive();
        drive.HealthStatus = status;
        Assert.False(new DriveDashboardViewModel { Drive = drive }.IsHealthOk);
    }

    // ── PropertyChanged ──────────────────────────────────────────────────────

    [Fact]
    public void SettingDrive_RaisesPropertyChangedForHasDrive()
    {
        var vm = new DriveDashboardViewModel();
        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.Drive = MakeDrive();

        Assert.Contains(nameof(vm.HasDrive), changed);
    }

    [Fact]
    public void SettingDriveToNull_HasDriveBecomeFalse()
    {
        var vm = new DriveDashboardViewModel { Drive = MakeDrive() };
        vm.Drive = null;
        Assert.False(vm.HasDrive);
    }
}
