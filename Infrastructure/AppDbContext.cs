using Microsoft.EntityFrameworkCore;
using pyjump.Entities;

namespace pyjump.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public DbSet<WhitelistEntry> Whitelist { get; set; }
        public DbSet<FileEntry> Files { get; set; }
        public DbSet<OwnerIdentity> OwnerIdentities { get; set; }
        public DbSet<LNKIdentity> LNKIdentities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=data.sqlite").UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WhitelistEntry>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.ResourceKey)
                    .HasDefaultValue(string.Empty);
                x.Property(e => e.DriveId)
                    .HasDefaultValue(string.Empty);
                x.Property(e => e.Name)
                    .IsRequired();
                x.Property(e => e.Url)
                    .IsRequired();
                x.Property(e => e.LastChecked);
                x.Property(e => e.Type)
                    .HasDefaultValue(string.Empty);
            });

            modelBuilder.Entity<FileEntry>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.ResourceKey)
                    .HasDefaultValue(string.Empty);
                x.Property(e => e.DriveId)
                    .HasDefaultValue(string.Empty);
                x.Property(e => e.Url)
                    .IsRequired();
                x.Property(e => e.Name)
                    .IsRequired();
                x.Property(e => e.LastModified);
                x.Property(e => e.Owner)
                    .HasDefaultValue(string.Empty);
                x.Property(e => e.FolderId);
                x.Property(e => e.FolderName)
                    .IsRequired();
                x.Property(e => e.FolderUrl)
                    .IsRequired();
                x.Property(e => e.Type)
                    .IsRequired();
                x.Property(e => e.FilterIgnored)
                    .HasDefaultValue(false);

                x.HasOne<WhitelistEntry>()
                    .WithMany()
                    .HasForeignKey(e => e.FolderId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<OwnerIdentity>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Name)
                    .IsRequired();
            });

            modelBuilder.Entity<LNKIdentity>(x =>
            {
                x.HasKey(e => new { e.Identity1, e.Identity2 });
                x.Property(e => e.Identity1)
                    .IsRequired();
                x.Property(e => e.Identity2)
                    .IsRequired();

                x.HasOne<OwnerIdentity>()
                    .WithMany()
                    .HasForeignKey(e => e.Identity1)
                    .OnDelete(DeleteBehavior.Cascade);

                x.HasOne<OwnerIdentity>()
                    .WithMany()
                    .HasForeignKey(e => e.Identity2)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
