namespace MinimalDriveApp.Models;

public class DriveInfo
{
    public string SerialNumber { get; set; } = string.Empty;
    public string DriveLetter { get; set; } = string.Empty;
    public string VolumeName { get; set; } = string.Empty;
    public string DriveType { get; set; } = string.Empty;
    public string FileSystem { get; set; } = string.Empty;
    public long Capacity { get; set; }
    public long UsedSpace { get; set; }
    public long FreeSpace { get; set; }
    public string HealthStatus { get; set; } = string.Empty;
    public string ConnectionType { get; set; } = string.Empty;

    public DateTime? LastUpdated { get; set; }
    public DateTime? LastBackedUp { get; set; }

    public DriveStatus Status { get; set; }

    public bool IsOutdatedBackup =>
        LastUpdated.HasValue && LastBackedUp.HasValue && LastUpdated > LastBackedUp;

    public bool IsHealthWarning =>
        HealthStatus is "Degraded" or "Unknown" or "Pred Fail" or "Error" or "Starting" or "Stopping";
}
