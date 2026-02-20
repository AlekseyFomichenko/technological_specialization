using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data
{
    internal class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Session> Sessions => Set<Session>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<FileMetadata> FileMetadata => Set<FileMetadata>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Login).IsUnique();
                entity.Property(e => e.Login).HasMaxLength(100);
                entity.Property(e => e.PasswordHash).HasMaxLength(256);
            });

            modelBuilder.Entity<Session>(entity =>
            {
                entity.ToTable("sessions");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Token);
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.Token).HasMaxLength(64);
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("messages");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ReceiverId);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.ReceiverId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.Property(e => e.Content).HasMaxLength(10000);
            });

            modelBuilder.Entity<FileMetadata>(entity =>
            {
                entity.ToTable("files");
                entity.HasKey(e => e.Id);
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.ReceiverId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.Property(e => e.FileName).HasMaxLength(255);
                entity.Property(e => e.FilePath).HasMaxLength(1024);
            });
        }
    }
}
