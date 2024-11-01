using System;
using Microsoft.EntityFrameworkCore;
using NotionTaskAutomation.Objects;

namespace NotionTaskAutomation.Db;

public class NotionDbContext : DbContext
{
    public DbSet<NotionPageRule> NotionPageRules { get; set; }

    public string DbPath { get; }

    public NotionDbContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "notionrules.db");
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}