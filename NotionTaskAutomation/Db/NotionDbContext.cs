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
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "notionrules.db");
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={DbPath}");
    }
}