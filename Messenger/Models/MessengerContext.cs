namespace Messenger.Models
{
    public partial class MessengerContext : DbContext
    {
        private readonly IConfiguration _configuration;

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

        public virtual DbSet<FileName> FileNames { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseMySql("server=database;port=3306;database=messenger;uid=root;", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.27-mysql"))
                    .UseLazyLoadingProxies();
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
                    .HasMaxLength(16)
                    .HasColumnName("id")
                    .IsFixedLength()
                    .HasConversion<byte[]>();

                entity.Property(e => e.ImageSrc)
                    .HasMaxLength(100)
                    .HasColumnName("image_src")
                    .HasDefaultValueSql("'default-profile-icon-16.png'");

                entity.Property(e => e.Password)
                    .HasMaxLength(64)
                    .HasColumnName("password")
                    .IsFixedLength();

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(15)
                    .HasColumnName("phone_number");

                entity.Property(e => e.Nickname)
                    .HasMaxLength(60)
                    .HasColumnName("nickname");

                entity.Property(e => e.Salt)
                    .HasMaxLength(10)
                    .HasColumnName("salt");

                entity.Property(e => e.Status)
                    .HasMaxLength(250)
                    .HasColumnName("status");

                entity.Property(e => e.Username)
                    .HasMaxLength(45)
                    .HasColumnName("username");
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("messages");

                entity.HasIndex(e => e.UserFrom, "id_idx");

                entity.HasIndex(e => e.UserTo, "to_fk_idx");

                entity.HasIndex(e => e.FileID, "file_fk_idx");

                entity.Property(e => e.Id)
                    .HasMaxLength(16)
                    .HasColumnName("id")
                    .IsFixedLength()
                    .HasConversion<byte[]>();

                entity.Property(e => e.Content)
                    .HasColumnType("text")
                    .HasColumnName("content");

                entity.Property(e => e.DateSent)
                    .HasColumnType("datetime")
                    .HasColumnName("date_sent");

                entity.Property(e => e.IsDelivered).HasColumnName("is_delivered");

                entity.Property(e => e.MessageType)
                    .HasColumnName("message_type")
                    .HasConversion<int>();

                entity.Property(e => e.FileID)
                    .HasColumnName("file_id")
                    .HasMaxLength(16)
                    .IsFixedLength()
                    .HasConversion<byte[]>();

                entity.Property(e => e.UserFrom)
                    .HasMaxLength(16)
                    .HasColumnName("user_from")
                    .IsFixedLength()
                    .HasConversion<byte[]>();

                entity.Property(e => e.UserTo)
                    .HasMaxLength(16)
                    .HasColumnName("user_to")
                    .IsFixedLength()
                    .HasConversion<byte[]>();

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

                entity.HasOne(d => d.FileNavigation)
                    .WithMany(p => p.MessageFileIDNavigations)
                    .HasForeignKey(d => d.FileID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("file_fk");
            });

            modelBuilder.Entity<FileName>(entity =>
            {
                entity.ToTable("file_names");

                entity.Property(e => e.ID)
                    .HasMaxLength(16)
                    .HasColumnName("id")
                    .IsFixedLength()
                    .HasConversion<byte[]>();

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .HasColumnName("name");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
