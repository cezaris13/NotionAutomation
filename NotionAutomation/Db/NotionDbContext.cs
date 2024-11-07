using Microsoft.EntityFrameworkCore;
using NotionAutomation.Objects;

namespace NotionAutomation.Db;

public class NotionDbContext(DbContextOptions<NotionDbContext> options) : DbContext(options) {
    public DbSet<NotionDatabaseRule> NotionDatabaseRules { get; set; }
}