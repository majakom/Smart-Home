using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartHome.Domain.Measurements;
using SmartHome.Domain.Sensors;

namespace SmartHome.Infrastructure.Persistence.Configurations;

public sealed class ClimateMeasurementConfiguration : IEntityTypeConfiguration<ClimateMeasurement>
{
    public void Configure(EntityTypeBuilder<ClimateMeasurement> builder)
    {
        builder.ToTable("climate_measurements");

        builder.HasKey(measurement => measurement.Id);

        builder.Property(measurement => measurement.Id)
            .HasConversion(id => id.Value, value => new ClimateMeasurementId(value))
            .ValueGeneratedNever();

        builder.Property(measurement => measurement.SourceMessageId)
            .HasConversion(id => id.Value, value => new SourceMessageId(value))
            .IsRequired();

        builder.Property(measurement => measurement.SensorId)
            .HasConversion(id => id.Value, value => new SensorId(value))
            .IsRequired();

        builder.HasOne<Sensor>()
            .WithMany()
            .HasForeignKey(measurement => measurement.SensorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(measurement => measurement.MeasuredAtUtc)
            .IsRequired();

        builder.Property(measurement => measurement.ReceivedAtUtc)
            .IsRequired();

        builder.Property(measurement => measurement.TemperatureCelsius)
            .HasPrecision(6, 2);

        builder.Property(measurement => measurement.RelativeHumidityPercent)
            .HasPrecision(5, 2);

        builder.HasIndex(measurement => new
        {
            measurement.SensorId,
            measurement.MeasuredAtUtc
        });

        // Prevent replays of the same sensor message from creating duplicate measurements.
        builder.HasIndex(measurement => new
        {
            measurement.SensorId,
            measurement.SourceMessageId
        })
        .IsUnique();
    }
}
