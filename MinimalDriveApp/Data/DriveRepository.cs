using MinimalDriveApp.Models;

namespace MinimalDriveApp.Data;

public class DriveRepository : IDriveRepository
{
    private readonly MinimalDriveAppDbContext _db;

    public DriveRepository(MinimalDriveAppDbContext db) => _db = db;

    public KnownDrive? GetBySerial(string serialNumber)
        => _db.KnownDrives.Find(serialNumber);

    public void Upsert(string serialNumber)
    {
        var now = DateTime.UtcNow;
        var existing = _db.KnownDrives.Find(serialNumber);

        if (existing is null)
        {
            _db.KnownDrives.Add(new KnownDrive
            {
                SerialNumber = serialNumber,
                FirstSeen = now,
                LastSeen = now
            });
        }
        else
        {
            existing.LastSeen = now;
        }

        _db.SaveChanges();
    }
}
