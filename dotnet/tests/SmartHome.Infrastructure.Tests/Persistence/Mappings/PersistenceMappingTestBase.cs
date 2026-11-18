
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmartHome.Infrastructure.Persistence;

namespace SmartHome.Infrastructure.Tests.Persistence.Mappings;

public abstract class PersistenceMappingTestBase
{
    private readonly IModel _model;

    protected PersistenceMappingTestBase()
    {
        DbContextOptions<SmartHomeDbContext> options =
            new DbContextOptionsBuilder<SmartHomeDbContext>()
                .UseNpgsql(
                    "Host=localhost;Database=smarthome_model_tests;Username=test;Password=test"
                )
                .Options;

        using SmartHomeDbContext dbContext = new(options);

        _model = dbContext.Model;
    }

    protected IEntityType GetEntityType<TEntity>()
    {
        return _model.FindEntityType(typeof(TEntity))
            ?? throw new InvalidOperationException(
                $"Entity type {typeof(TEntity).Name} was not found in the EF model."
            );
    }

    protected IProperty GetProperty<TEntity>(string propertyName)
    {
        return GetEntityType<TEntity>().FindProperty(propertyName)
            ?? throw new InvalidOperationException(
                $"Property {propertyName} was not found on {typeof(TEntity).Name}."
            );
    }

    protected IForeignKey GetForeignKey<TEntity>(string propertyName)
    {
        return GetEntityType<TEntity>().GetForeignKeys()
            .Single(foreignKey =>
                foreignKey.Properties.Any(property =>
                    property.Name == propertyName
                )
            );
    }

    protected void AssertProviderType<TEntity>(
        string propertyName,
        Type expectedProviderType)
    {
        IProperty property = GetProperty<TEntity>(propertyName);

        ValueConverter converter = property.GetTypeMapping().Converter
            ?? throw new InvalidOperationException(
                $"Property {propertyName} on {typeof(TEntity).Name} has no value converter."
            );

        Assert.Equal(expectedProviderType, converter.ProviderClrType);
    }

    protected void AssertPrimaryKey<TEntity>(string propertyName)
    {
        IKey primaryKey = Assert.Single(
            GetEntityType<TEntity>().GetKeys(),
            key => key.IsPrimaryKey()
        );

        Assert.Equal(
            new[] { propertyName },
            primaryKey.Properties.Select(property => property.Name)
        );
    }
}
