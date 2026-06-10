using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MinimalDriveApp.Data;

public class ArchiveStackDbContextFactory : IDesignTimeDbContextFactory<ArchiveStackDbContext>
{
    public ArchiveStackDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ArchiveStackDbContext>()
            .UseSqlite("Data Source=archivestack.db")
            .Options;
        return new ArchiveStackDbContext(options);
    }
}
