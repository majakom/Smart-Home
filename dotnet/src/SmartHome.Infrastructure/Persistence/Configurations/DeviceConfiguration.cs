using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartHome.Domain.Devices;

namespace SmartHome.Infrastructure.Persistence.Configurations;

public sealed class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("devices");

        builder.HasKey(device => device.Id);

        builder.Property(device => device.Id)
            .HasConversion(id => id.Value, value => new DeviceId(value))
            .ValueGeneratedNever();

        builder.Property(device => device.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(device => device.Model)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(device => device.RegisteredAtUtc)
            .IsRequired();

        builder.Property(device => device.LastSeenAtUtc);

        builder.Property(device => device.IsEnabled)
            .IsRequired();
    }
}
