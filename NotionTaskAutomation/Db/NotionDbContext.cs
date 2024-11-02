using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using NotionTaskAutomation.Objects;

namespace NotionTaskAutomation.Db;

public class NotionDbContext : DbContext
{
    public DbSet<NotionDatabaseRule> NotionDatabaseRules { get; }

    public string DbPath { get; }

    public NotionDbContext()
    {
        const Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "notionrules.db");
    }
    
    public NotionDbContext(DbContextOptions<NotionDbContext> options)
        : base(options)
    {
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={DbPath}");
    }
}