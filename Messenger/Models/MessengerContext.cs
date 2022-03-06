using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Messenger.Models
{
    public partial class MessengerContext : DbContext
    {
        private IConfiguration _configuration;

        public MessengerContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public MessengerContext(DbContextOptions<MessengerContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        public virtual DbSet<AuthUser> AuthUsers { get; set; } = null!;
        public virtual DbSet<Message> Messages { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connection = _configuration["Messenger:DatabaseConnectionString"];

                optionsBuilder
                    .UseLazyLoadingProxies()
                    .UseMySql(connection, ServerVersion.Parse("8.0.27-mysql"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("utf8mb4_0900_ai_ci")
                .HasCharSet("utf8mb4");

            modelBuilder.Entity<AuthUser>(entity =>
            {
                entity.ToTable("auth_user");

                entity.Property(e => e.Id)
                    .HasConversion<byte[]>()
                    .HasColumnName("id")
                    .IsFixedLength();

                entity.Property(e => e.ImageSrc)
                    .HasMaxLength(100)
                    .HasColumnName("image_src")
                    .HasDefaultValueSql("'/Images/default-profile-icon-16.png'");

                entity.Property(e => e.Password)
                    .HasMaxLength(32)
                    .HasColumnName("password")
                    .IsFixedLength();

                entity.Property(e => e.Username)
                    .HasMaxLength(45)
                    .HasColumnName("username");
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("messages");

                entity.HasIndex(e => e.UserFrom, "id_idx");

                entity.HasIndex(e => e.UserTo, "to_fk_idx");

                entity.Property(e => e.Id)
                    .HasConversion<byte[]>()
                    .HasColumnName("id")
                    .IsFixedLength();

                entity.Property(e => e.Content)
                    .HasColumnType("text")
                    .HasColumnName("content");

                entity.Property(e => e.MessageType)
                    .HasConversion<int>()
                    .HasColumnName("message_type");

                entity.Property(e => e.UserFrom)
                    .HasConversion<byte[]>()
                    .HasColumnName("user_from")
                    .IsFixedLength();

                entity.Property(e => e.UserTo)
                    .HasConversion<byte[]>()
                    .HasColumnName("user_to")
                    .IsFixedLength();

                entity.Property(e => e.IsDelivered)
                    .HasColumnType("tinyint")
                    .HasColumnName("is_delivered");

                entity.HasOne(d => d.UserFromNavigation)
                    .WithMany(p => p.MessageUserFromNavigations)
                    .HasForeignKey(d => d.UserFrom)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("from_fk");

                entity.HasOne(d => d.UserToNavigation)
                    .WithMany(p => p.MessageUserToNavigations)
                    .HasForeignKey(d => d.UserTo)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("to_fk");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
