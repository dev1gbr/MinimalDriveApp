using MinimalDriveApp.Services;

namespace MinimalDriveApp.Tests.Services;

public class DriveDetectionServiceMappingTests
{
    [Theory]
    [InlineData(2u, "Removable")]
    [InlineData(3u, "Fixed")]
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

    [Theory]
    [InlineData("USB", "", "Removable")]
    [InlineData("usb", "", "Removable")]
    [InlineData("SCSI", "", "Fixed")]
    [InlineData("IDE", "", "Fixed")]
    [InlineData("NVME", "", "Fixed")]
    [InlineData("", "Removable Media", "Removable")]
    [InlineData("", "Fixed hard disk media", "Fixed")]
    [InlineData("", "", "Unknown")]
    public void MapDriveTypeFromMedia_ReturnsExpectedLabel(string interfaceType, string mediaType, string expected)
    {
        Assert.Equal(expected, DriveDetectionService.MapDriveTypeFromMedia(mediaType, interfaceType));
    }
}
