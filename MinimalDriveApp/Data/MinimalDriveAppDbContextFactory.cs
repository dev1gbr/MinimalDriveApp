using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MinimalDriveApp.Data;

public class MinimalDriveAppDbContextFactory : IDesignTimeDbContextFactory<MinimalDriveAppDbContext>
{
    public MinimalDriveAppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MinimalDriveAppDbContext>()
            .UseSqlite("Data Source=minimaldriveapp.db")
            .Options;
        return new MinimalDriveAppDbContext(options);
    }
}
