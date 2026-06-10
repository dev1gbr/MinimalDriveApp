using Microsoft.EntityFrameworkCore;
using MinimalDriveApp.Models;

namespace MinimalDriveApp.Data;

public class ArchiveStackDbContext : DbContext
{
    public ArchiveStackDbContext(DbContextOptions<ArchiveStackDbContext> options) : base(options) { }

    public DbSet<KnownDrive> KnownDrives => Set<KnownDrive>();
}
