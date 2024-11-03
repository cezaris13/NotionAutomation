using Microsoft.EntityFrameworkCore;
using NotionTaskAutomation.Objects;

namespace NotionTaskAutomation.Db;

public class NotionDbContext(DbContextOptions<NotionDbContext> options) : DbContext(options)
{
    public DbSet<NotionDatabaseRule> NotionDatabaseRules { get; set; }
}