using Microsoft.EntityFrameworkCore;
using NotificationGateway.Domain.Entities;

namespace NotificationGateway.Infrastructure.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }

        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedNever();

                entity.Property(e => e.Recipient)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.Subject)
                    .HasMaxLength(1000);

                entity.Property(e => e.Body)
                    .IsRequired();

                entity.Property(e => e.IdempotencyKey)
                    .HasMaxLength(256);

                entity.Property(e => e.ErrorMessage)
                    .HasMaxLength(2000);

                entity.Property(e => e.Status)
                    .HasConversion<string>();

                entity.Property(e => e.Channel)
                    .HasConversion<string>();

                entity.Property(e => e.MessageType)
                    .HasConversion<string>();

                entity.HasIndex(e => e.IdempotencyKey)
                    .IsUnique()
                    .HasFilter("[IdempotencyKey] IS NOT NULL");

                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
            });
        }
    }
}
