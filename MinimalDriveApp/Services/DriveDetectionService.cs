using Microsoft.Management.Infrastructure;
using MinimalDriveApp.Models;

namespace MinimalDriveApp.Services;

public class DriveDetectionService : IDriveDetectionService
{
    public IReadOnlyList<DriveInfo> GetConnectedDrives()
    {
        var result = new List<DriveInfo>();

        using var session = CimSession.Create("localhost");

        var physicalDrives = session
            .QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_DiskDrive")
            .ToList();

        foreach (var disk in physicalDrives)
        {
            var serial = disk.CimInstanceProperties["SerialNumber"]?.Value?.ToString()?.Trim() ?? string.Empty;
            var interfaceType = disk.CimInstanceProperties["InterfaceType"]?.Value?.ToString() ?? string.Empty;
            var health = disk.CimInstanceProperties["Status"]?.Value?.ToString() ?? string.Empty;

            foreach (var letter in GetDriveLetters(session, disk))
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

        return result;
    }

    private static IEnumerable<LogicalDiskData> GetDriveLetters(CimSession session, CimInstance disk)
    {
        var diskQuery = $"ASSOCIATORS OF {{Win32_DiskDrive.DeviceID='{EscapeWql(disk.CimInstanceProperties["DeviceID"]?.Value?.ToString() ?? "")}'}} WHERE AssocClass=Win32_DiskDriveToDiskPartition";

        foreach (var partition in session.QueryInstances(@"root\cimv2", "WQL", diskQuery))
        {
            var partQuery = $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{EscapeWql(partition.CimInstanceProperties["DeviceID"]?.Value?.ToString() ?? "")}'}} WHERE AssocClass=Win32_LogicalDiskToPartition";

            foreach (var logical in session.QueryInstances(@"root\cimv2", "WQL", partQuery))
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

    private static string MapDriveType(object? value) => value switch
    {
        2u or 2 => "Fixed",
        3u or 3 => "Removable",  // WMI uses 2=Fixed, 3=Removable — spec labels differ from WMI numeric values
        4u or 4 => "Network",
        5u or 5 => "CD-ROM",
        _ => "Unknown"
    };

    private static string MapConnectionType(string interfaceType) => interfaceType.ToUpperInvariant() switch
    {
        "USB" => "USB",
        "SCSI" => "SATA",
        "IDE" => "SATA",
        "NVME" => "NVMe",
        var s when s.Contains("NVME") => "NVMe",
        _ => interfaceType
    };

    private static string EscapeWql(string value) => value.Replace("\\", "\\\\");

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
