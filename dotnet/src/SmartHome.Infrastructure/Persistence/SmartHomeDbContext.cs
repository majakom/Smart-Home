using Microsoft.EntityFrameworkCore;
using SmartHome.Domain.Devices;
using SmartHome.Domain.Sensors;
using SmartHome.Domain.Measurements;

namespace SmartHome.Infrastructure.Persistence;

public sealed class SmartHomeDbContext : DbContext
{
    public SmartHomeDbContext(
        DbContextOptions<SmartHomeDbContext> options)
        : base(options)
    {
    }

    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Sensor> Sensors => Set<Sensor>();
    public DbSet<ClimateMeasurement> ClimateMeasurements => Set<ClimateMeasurement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Discover IEntityTypeConfiguration mappings from this Infrastructure assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SmartHomeDbContext).Assembly);
    }
}
