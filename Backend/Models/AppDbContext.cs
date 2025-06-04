using System;

using Microsoft.EntityFrameworkCore;

namespace Jour2.Models;

public class AppDbContext : DbContext
{
    public DbSet<Entry> Entries { get; set; } = null!;
    private readonly string _root;

    public AppDbContext() 
    {
        _root = AppDomain.CurrentDomain.BaseDirectory;
    }
    public AppDbContext(string root) 
    {
        _root = root;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={_root}\\app.db");
    }
}
