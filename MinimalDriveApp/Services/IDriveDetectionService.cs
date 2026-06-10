using MinimalDriveApp.Models;

namespace MinimalDriveApp.Services;

public interface IDriveDetectionService
{
    IReadOnlyList<DriveInfo> GetConnectedDrives();
}
