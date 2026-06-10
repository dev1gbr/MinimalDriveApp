using Microsoft.EntityFrameworkCore;
using MinimalDriveApp.Data;

namespace MinimalDriveApp.IntegrationTests.Data;

public class DriveRepositoryIntegrationTests : IDisposable
{
    private readonly ArchiveStackDbContext _db;
    private readonly DriveRepository _repo;

    public DriveRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ArchiveStackDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        _db = new ArchiveStackDbContext(options);
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();
        _repo = new DriveRepository(_db);
    }

    public void Dispose()
    {
        _db.Database.CloseConnection();
        _db.Dispose();
    }

    [Fact]
    public void Upsert_PersistsRecord_InRealSqlite()
    {
        _repo.Upsert("SN-REAL-001");

        var result = _repo.GetBySerial("SN-REAL-001");

        Assert.NotNull(result);
        Assert.Equal("SN-REAL-001", result.SerialNumber);
        Assert.Null(result.UserName);
        Assert.Null(result.LastUpdated);
        Assert.Null(result.LastBackedUp);
    }

    [Fact]
    public void Upsert_PreservesFirstSeen_OnSubsequentCalls()
    {
        _repo.Upsert("SN-REAL-002");
        var first = _repo.GetBySerial("SN-REAL-002")!.FirstSeen;

        _repo.Upsert("SN-REAL-002");
        var updated = _repo.GetBySerial("SN-REAL-002")!;

        Assert.Equal(first, updated.FirstSeen);
    }

    [Fact]
    public void GetBySerial_ReturnsNull_ForUnknownSerial()
    {
        var result = _repo.GetBySerial("DOES-NOT-EXIST");
        Assert.Null(result);
    }
}
