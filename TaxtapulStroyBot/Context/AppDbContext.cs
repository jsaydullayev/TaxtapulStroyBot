using Microsoft.EntityFrameworkCore;
using TaxtapulStroyBot.Entities;

public class AppDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Port=2025;Username=postgres;Password=postgresql;Database=bot_database;");
    }
}
