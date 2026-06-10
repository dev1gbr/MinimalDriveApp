using Microsoft.EntityFrameworkCore;
using MinimalDriveApp.Models;

namespace MinimalDriveApp.Data;

public class MinimalDriveAppDbContext : DbContext
{
    public MinimalDriveAppDbContext(DbContextOptions<MinimalDriveAppDbContext> options) : base(options) { }

    public DbSet<KnownDrive> KnownDrives => Set<KnownDrive>();
}
