using Microsoft.Management.Infrastructure;
using MinimalDriveApp.Models;

namespace MinimalDriveApp.Services;

public class DriveDetectionService : IDriveDetectionService
{
    public IReadOnlyList<DriveInfo> GetConnectedDrives()
    {
        var result = new List<DriveInfo>();

        // null = local machine via DCOM; avoids WinRM/WS-Man dependency
        using var session = CimSession.Create(null);

        var physicalDrives = session
            .QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_DiskDrive")
            .ToList();

        foreach (var disk in physicalDrives)
        {
            var serial = disk.CimInstanceProperties["SerialNumber"]?.Value?.ToString()?.Trim() ?? string.Empty;
            var interfaceType = disk.CimInstanceProperties["InterfaceType"]?.Value?.ToString() ?? string.Empty;
            var health = disk.CimInstanceProperties["Status"]?.Value?.ToString() ?? string.Empty;
            var physicalCapacity = Convert.ToInt64(disk.CimInstanceProperties["Size"]?.Value ?? 0L);
            var mediaType = disk.CimInstanceProperties["MediaType"]?.Value?.ToString() ?? string.Empty;

            var logicalDisks = GetDriveLetters(session, disk).ToList();

            if (logicalDisks.Count > 0)
            {
                foreach (var letter in logicalDisks)
                {
                    result.Add(new DriveInfo
                    {
                        SerialNumber = serial,
                        DriveLetter = letter.DriveLetter,
                        VolumeName = letter.VolumeName,
                        DriveType = letter.DriveType,
                        FileSystem = letter.FileSystem,
                        Capacity = letter.Capacity,
                        FreeSpace = letter.FreeSpace,
                        UsedSpace = letter.Capacity - letter.FreeSpace,
                        HealthStatus = health,
                        ConnectionType = MapConnectionType(interfaceType)
                    });
                }
            }
            else
            {
                // No logical disk found — drive may be encrypted/locked or unformatted.
                // Show a placeholder row so the user knows the physical disk is present.
                result.Add(new DriveInfo
                {
                    SerialNumber = serial,
                    DriveLetter = string.Empty,
                    VolumeName = string.Empty,
                    DriveType = MapDriveTypeFromMedia(mediaType, interfaceType),
                    FileSystem = "Encrypted",
                    Capacity = physicalCapacity,
                    FreeSpace = 0,
                    UsedSpace = 0,
                    HealthStatus = health,
                    ConnectionType = MapConnectionType(interfaceType)
                });
            }
        }

        return result;
    }

    private static IEnumerable<LogicalDiskData> GetDriveLetters(CimSession session, CimInstance disk)
    {
        // EnumerateAssociatedInstances is the correct MI API for association traversal
        // QueryInstances with ASSOCIATORS OF WQL does not work reliably with CimSession
        var partitions = session.EnumerateAssociatedInstances(
            @"root\cimv2",
            disk,
            "Win32_DiskDriveToDiskPartition",
            "Win32_DiskPartition",
            "Antecedent",
            "Dependent");

        foreach (var partition in partitions)
        {
            var logicals = session.EnumerateAssociatedInstances(
                @"root\cimv2",
                partition,
                "Win32_LogicalDiskToPartition",
                "Win32_LogicalDisk",
                "Antecedent",
                "Dependent");

            foreach (var logical in logicals)
            {
                yield return new LogicalDiskData
                {
                    DriveLetter = logical.CimInstanceProperties["DeviceID"]?.Value?.ToString() ?? string.Empty,
                    VolumeName = logical.CimInstanceProperties["VolumeName"]?.Value?.ToString() ?? string.Empty,
                    DriveType = MapDriveType(logical.CimInstanceProperties["DriveType"]?.Value),
                    FileSystem = logical.CimInstanceProperties["FileSystem"]?.Value?.ToString() ?? string.Empty,
                    Capacity = Convert.ToInt64(logical.CimInstanceProperties["Size"]?.Value ?? 0L),
                    FreeSpace = Convert.ToInt64(logical.CimInstanceProperties["FreeSpace"]?.Value ?? 0L)
                };
            }
        }
    }

    internal static string MapDriveType(object? value) => value switch
    {
        2u or 2 => "Removable",
        3u or 3 => "Fixed",
        4u or 4 => "Network",
        5u or 5 => "CD-ROM",
        _ => "Unknown"
    };

    // Fallback for drives with no logical disk — derive type from physical WMI properties
    internal static string MapDriveTypeFromMedia(string mediaType, string interfaceType)
    {
        var iface = interfaceType.ToUpperInvariant();
        if (iface == "USB") return "Removable";
        if (iface is "SCSI" or "IDE") return "Fixed";
        if (iface.Contains("NVME")) return "Fixed";

        var media = mediaType.ToUpperInvariant();
        if (media.Contains("REMOVABLE")) return "Removable";
        if (media.Contains("FIXED")) return "Fixed";

        return "Unknown";
    }

    internal static string MapConnectionType(string interfaceType) => interfaceType.ToUpperInvariant() switch
    {
        "USB" => "USB",
        "SCSI" => "SATA",
        "IDE" => "SATA",
        "NVME" => "NVMe",
        var s when s.Contains("NVME") => "NVMe",
        _ => interfaceType
    };

    private sealed class LogicalDiskData
    {
        public string DriveLetter { get; init; } = string.Empty;
        public string VolumeName { get; init; } = string.Empty;
        public string DriveType { get; init; } = string.Empty;
        public string FileSystem { get; init; } = string.Empty;
        public long Capacity { get; init; }
        public long FreeSpace { get; init; }
    }
}
