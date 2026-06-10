using System.ComponentModel.DataAnnotations;

namespace MinimalDriveApp.Models;

public class KnownDrive
{
    [Key]
    public string SerialNumber { get; set; } = string.Empty;

    public string? UserName { get; set; }

    public DateTime FirstSeen { get; set; }

    public DateTime LastSeen { get; set; }

    public DateTime? LastUpdated { get; set; }

    public DateTime? LastBackedUp { get; set; }
}
