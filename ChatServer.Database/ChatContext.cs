using ChatServer.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Database
{
    public class ChatContext : DbContext
    {
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<MessageEntity> Messages { get; set; }
        public DbSet<FileEntity> Files { get; set; }

        public ChatContext() { }

        public ChatContext(DbContextOptions<ChatContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured) return;
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
                ?? Environment.GetEnvironmentVariable("ConnectionStrings:Default");
            if (connectionString != null)
                optionsBuilder.UseNpgsql(connectionString).UseLazyLoadingProxies();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UserEntity>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(x => x.Id).HasName("userPk");
                entity.HasIndex(x => x.FullName).IsUnique();
                entity.Property(e => e.FullName)
                    .HasColumnName("FullName")
                    .HasMaxLength(255)
                    .IsRequired();
                entity.Property(e => e.PasswordHash)
                    .HasColumnName("PasswordHash")
                    .HasMaxLength(255);
            });
            modelBuilder.Entity<FileEntity>(entity =>
            {
                entity.ToTable("files");
                entity.HasKey(x => x.Id).HasName("filePk");
                entity.Property(e => e.FileName).HasMaxLength(512).IsRequired();
                entity.Property(e => e.MimeType).HasMaxLength(128);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.UploadedAt).HasColumnName("UploadedAt");
                entity.HasOne(x => x.Sender)
                    .WithMany()
                    .HasForeignKey(x => x.SenderId)
                    .HasConstraintName("fileSenderFK");
                entity.HasOne(x => x.Recipient)
                    .WithMany()
                    .HasForeignKey(x => x.RecipientId)
                    .HasConstraintName("fileRecipientFK");
            });
            modelBuilder.Entity<MessageEntity>(entity =>
            {
                entity.ToTable("messages");
                entity.HasKey(x => x.MessageId).HasName("messagePk");
                entity.Property(e => e.Text).HasColumnName("messageText");
                entity.Property(e => e.DateSend).HasColumnName("messageData");
                entity.Property(e => e.IsSent).HasColumnName("is_sent");
                entity.Property(e => e.MessageId).HasColumnName("id");
                entity.HasOne(x => x.UserTo)
                    .WithMany(m => m.MessagesTo)
                    .HasForeignKey(x => x.UserToId)
                    .HasConstraintName("messageToUserFK");
                entity.HasOne(x => x.UserFrom)
                    .WithMany(m => m.MessagesFrom)
                    .HasForeignKey(x => x.UserFromId)
                    .HasConstraintName("messageFromUserFK");
                entity.HasOne(x => x.File)
                    .WithMany()
                    .HasForeignKey(x => x.FileId)
                    .HasConstraintName("messageFileFK");
            });
        }
    }
}
