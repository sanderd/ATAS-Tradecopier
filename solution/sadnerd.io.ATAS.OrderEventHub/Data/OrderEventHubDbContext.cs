using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using sadnerd.io.ATAS.OrderEventHub.Data.Models;
using sadnerd.io.ATAS.OrderEventHub.Identity;

namespace sadnerd.io.ATAS.OrderEventHub.Data;

public class OrderEventHubDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public DbSet<AtasAccount> AtasAccounts { get; set; }
    public DbSet<ProjectXAccount> ProjectXAccounts { get; set; }
    public DbSet<ProjectXApiCredential> ProjectXApiCredentials { get; set; }
    public DbSet<CopyStrategy> CopyStrategies { get; set; }

    public string DbPath { get; }

    public OrderEventHubDbContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "sadnerd.tradecopy.db");
    }

    public OrderEventHubDbContext(DbContextOptions<OrderEventHubDbContext> options) : base(options)
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "sadnerd.tradecopy.db");
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
            options.UseSqlite($"Data Source={DbPath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure ProjectXAccount -> ApiCredential relationship
        modelBuilder.Entity<ProjectXAccount>()
            .HasOne(p => p.ApiCredential)
            .WithMany(a => a.ProjectXAccounts)
            .HasForeignKey(p => p.ApiCredentialId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure unique constraint on Vendor + DisplayName for API credentials
        modelBuilder.Entity<ProjectXApiCredential>()
            .HasIndex(a => new { a.Vendor, a.DisplayName })
            .IsUnique();

        // Configure Identity table names to avoid conflicts
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
        });

        modelBuilder.Entity<ApplicationRole>(entity =>
        {
            entity.ToTable("Roles");
        });
    }
}
