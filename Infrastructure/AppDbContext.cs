using Microsoft.EntityFrameworkCore;
using pyjump.Entities;

namespace pyjump.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public DbSet<WhitelistEntry> Whitelist { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=data.sqlite");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WhitelistEntry>()
                .HasKey(e => e.Id);
            modelBuilder.Entity<WhitelistEntry>()
                .Property(e => e.ResourceKey)
                .HasDefaultValue(string.Empty);
            modelBuilder.Entity<WhitelistEntry>()
                .Property(e => e.Name)
                .IsRequired();
            modelBuilder.Entity<WhitelistEntry>()
                .Property(e => e.Url)
                .IsRequired();
            modelBuilder.Entity<WhitelistEntry>()
                .Property(e => e.LastChecked);
            modelBuilder.Entity<WhitelistEntry>()
                .Property(e => e.Type)
                .HasDefaultValue(string.Empty);
        }
    }
}
