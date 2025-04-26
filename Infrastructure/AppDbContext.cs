using Microsoft.EntityFrameworkCore;
using pyjump.Entities;

namespace pyjump.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public DbSet<WhitelistEntry> Whitelist { get; set; }
        public DbSet<FileEntry> Files { get; set; }
        public DbSet<SimilarSet> SimilarSets { get; set; }
        public DbSet<LNKSimilarSetFile> LNKSimilarSetFiles { get; set; }

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

                x.HasOne<WhitelistEntry>()
                    .WithMany()
                    .HasForeignKey(e => e.FolderId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<SimilarSet>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id)
                    .ValueGeneratedOnAdd();
                x.Property(e => e.OwnerFileEntryId)
                    .IsRequired();
                x.HasOne<FileEntry>()
                    .WithMany()
                    .HasForeignKey(e => e.OwnerFileEntryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LNKSimilarSetFile>(x =>
            {
                x.HasKey(e => new { e.SimilarSetId, e.FileEntryId });
                x.Property(e => e.SimilarSetId)
                    .IsRequired();
                x.Property(e => e.FileEntryId)
                    .IsRequired();

                x.HasOne<SimilarSet>()
                    .WithMany()
                    .HasForeignKey(e => e.SimilarSetId)
                    .OnDelete(DeleteBehavior.Cascade);

                x.HasOne<FileEntry>()
                    .WithMany()
                    .HasForeignKey(e => e.FileEntryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
