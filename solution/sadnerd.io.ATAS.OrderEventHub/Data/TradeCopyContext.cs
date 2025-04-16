using Microsoft.EntityFrameworkCore;
using sadnerd.io.ATAS.OrderEventHub.Data.Models;

namespace sadnerd.io.ATAS.OrderEventHub.Data;

public class TradeCopyContext : DbContext
{
    public DbSet<AtasAccount> AtasAccounts { get; set; }
    public DbSet<TopstepAccount> TopstepAccount { get; set; }
    public DbSet<CopyStrategy> CopyStrategies { get; set; }

    public string DbPath { get; }

    public TradeCopyContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "sadnerd.tradecopy.db");
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}
