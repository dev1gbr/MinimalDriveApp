using MinimalDriveApp.Models;

namespace MinimalDriveApp.Data;

public interface IDriveRepository
{
    KnownDrive? GetBySerial(string serialNumber);
    void Upsert(string serialNumber);
}
