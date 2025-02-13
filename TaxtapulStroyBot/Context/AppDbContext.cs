using Microsoft.EntityFrameworkCore;
using TaxtapulStroyBot.Entities;

public class AppDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=stroyBot.db");
    }
}
