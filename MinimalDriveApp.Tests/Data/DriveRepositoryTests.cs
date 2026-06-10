using Microsoft.EntityFrameworkCore;
using MinimalDriveApp.Data;
using MinimalDriveApp.Models;

namespace MinimalDriveApp.Tests.Data;

public class DriveRepositoryTests
{
    private static MinimalDriveAppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<MinimalDriveAppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new MinimalDriveAppDbContext(options);
    }

    [Fact]
    public void GetBySerial_ReturnsNull_WhenNotInDatabase()
    {
        using var db = CreateInMemoryContext();
        var repo = new DriveRepository(db);

        var result = repo.GetBySerial("UNKNOWN-123");

        Assert.Null(result);
    }

    [Fact]
    public void Upsert_InsertsNewRecord_WhenSerialNotKnown()
    {
        using var db = CreateInMemoryContext();
        var repo = new DriveRepository(db);

        repo.Upsert("SN-001");

        var saved = db.KnownDrives.Find("SN-001");
        Assert.NotNull(saved);
        Assert.Equal("SN-001", saved.SerialNumber);
        Assert.Null(saved.UserName);
        Assert.True(saved.FirstSeen > DateTime.MinValue);
        Assert.Equal(saved.FirstSeen, saved.LastSeen);
    }

    [Fact]
    public void Upsert_UpdatesLastSeen_WhenSerialAlreadyKnown()
    {
        using var db = CreateInMemoryContext();
        var repo = new DriveRepository(db);

        repo.Upsert("SN-002");
        var firstSeen = db.KnownDrives.Find("SN-002")!.FirstSeen;

        repo.Upsert("SN-002");

        var updated = db.KnownDrives.Find("SN-002")!;
        Assert.Equal(firstSeen, updated.FirstSeen);
        Assert.True(updated.LastSeen >= updated.FirstSeen);
    }

    [Fact]
    public void GetBySerial_ReturnsDrive_AfterUpsert()
    {
        using var db = CreateInMemoryContext();
        var repo = new DriveRepository(db);

        repo.Upsert("SN-003");
        var result = repo.GetBySerial("SN-003");

        Assert.NotNull(result);
        Assert.Equal("SN-003", result.SerialNumber);
    }
}
