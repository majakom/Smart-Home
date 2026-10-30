using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmartHome.Domain.Devices;
using SmartHome.Domain.Sensors;
using SerialNumberValue = SmartHome.Domain.Shared.SerialNumber;

namespace SmartHome.Infrastructure.Persistence.Configurations;

public sealed class SensorConfiguration : IEntityTypeConfiguration<Sensor>
{
    public void Configure(EntityTypeBuilder<Sensor> builder)
    {
        builder.ToTable("sensors");

        builder.HasKey(sensor => sensor.Id);

        builder.Property(sensor => sensor.Id)
            .HasConversion(id => id.Value, value => new SensorId(value))
            .ValueGeneratedNever();

        builder.Property(sensor => sensor.DeviceId)
            .HasConversion(id => id.Value, value => new DeviceId(value))
            .IsRequired();

        builder.HasOne<Device>()
            .WithMany()
            .HasForeignKey(sensor => sensor.DeviceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(sensor => sensor.Category)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(sensor => sensor.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(sensor => sensor.Model)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(sensor => sensor.SerialNumber)
            .HasConversion(
                new ValueConverter<SerialNumberValue?, string?>(
                    serialNumber => serialNumber == null
                        ? null
                        : serialNumber.Value,
                    value => SerialNumberValue.Parse(value)))
            .HasMaxLength(100);

        builder.Property(sensor => sensor.RegisteredAtUtc)
            .IsRequired();

        builder.Property(sensor => sensor.IsEnabled)
            .IsRequired();
    }
}
