using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using CdcBridge.Core.Models;
using CdcBridge.Persistence.Models;

namespace CdcBridge.Persistence;

public class CdcBridgeDbContext : DbContext
{
    public DbSet<BufferedChangeEvent> BufferedChangeEvents { get; set; }
    public DbSet<ReceiverDeliveryStatus> ReceiverDeliveryStatuses { get; set; }
    public DbSet<TrackingInstanceState> TrackingInstanceStates { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }

    public CdcBridgeDbContext(DbContextOptions<CdcBridgeDbContext> options) : base(options)
    {
        //Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var jsonOptions = new JsonSerializerOptions();

        // Учим EF Core, как хранить TrackedChange в виде JSON-колонки
        modelBuilder.Entity<BufferedChangeEvent>()
            .Property(e => e.Change)
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonOptions),
                v => JsonSerializer.Deserialize<TrackedChange>(v, jsonOptions)!);

        modelBuilder.Entity<BufferedChangeEvent>()
            .HasMany(e => e.DeliveryStatuses)
            .WithOne(s => s.BufferedChangeEvent)
            .HasForeignKey(s => s.BufferedChangeEventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ReceiverDeliveryStatus>()
            .HasIndex(s => new { s.BufferedChangeEventId, s.ReceiverName })
            .IsUnique();

        // Конфигурация API ключей
        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Key).IsUnique();
            entity.Property(e => e.Key).HasMaxLength(64).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Owner).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Permission).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
        });
    }
}