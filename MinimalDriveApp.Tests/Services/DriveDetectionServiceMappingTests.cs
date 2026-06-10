using MinimalDriveApp.Services;

namespace MinimalDriveApp.Tests.Services;

public class DriveDetectionServiceMappingTests
{
    [Theory]
    [InlineData(2u, "Fixed")]
    [InlineData(3u, "Removable")]
    [InlineData(4u, "Network")]
    [InlineData(5u, "CD-ROM")]
    [InlineData(99u, "Unknown")]
    [InlineData(null, "Unknown")]
    public void MapDriveType_ReturnsExpectedLabel(object? wmiValue, string expected)
    {
        Assert.Equal(expected, DriveDetectionService.MapDriveType(wmiValue));
    }

    [Theory]
    [InlineData("USB", "USB")]
    [InlineData("usb", "USB")]
    [InlineData("SCSI", "SATA")]
    [InlineData("IDE", "SATA")]
    [InlineData("NVME", "NVMe")]
    [InlineData("NVMe", "NVMe")]
    [InlineData("Unknown", "Unknown")]
    public void MapConnectionType_ReturnsExpectedLabel(string interfaceType, string expected)
    {
        Assert.Equal(expected, DriveDetectionService.MapConnectionType(interfaceType));
    }
}
