using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatServer.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Database
{
    public class ChatContext : DbContext
    {
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<MessageEntity> Messages { get; set; }
        public ChatContext()
        {

        }
        public ChatContext(DbContextOptions<ChatContext> dbc) : base(dbc)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=UDPChat;Username=udpuser;Password=qwerty")
                .UseLazyLoadingProxies();
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
            });
            modelBuilder.Entity<MessageEntity>(entity =>
            {
                entity.ToTable("messages");
                entity.HasKey(x => x.MessageId).HasName("messagePk");

                entity.Property(e => e.Text)
                .HasColumnName("messageText");
                entity.Property(e => e.DateSend)
                .HasColumnName("messageData");
                entity.Property(e => e.IsSent)
                .HasColumnName("is_sent");
                entity.Property(e => e.MessageId)
                .HasColumnName("id");

                entity.HasOne(x => x.UserTo)
                .WithMany(m => m.MessagesTo)
                .HasForeignKey(x => x.UserToId)
                .HasConstraintName("messageToUserFK");
                entity.HasOne(x => x.UserFrom)
                .WithMany(m => m.MessagesFrom)
                .HasForeignKey(x => x.UserFromId)
                .HasConstraintName("messageFromUserFK");
            });
        }

    }
}
