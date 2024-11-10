using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotionAutomation.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotionDatabaseRules",
                columns: table => new
                {
                    RuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    DatabaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartingState = table.Column<string>(type: "text", nullable: true),
                    EndingState = table.Column<string>(type: "text", nullable: true),
                    OnDay = table.Column<string>(type: "text", nullable: true),
                    DayOffset = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotionDatabaseRules", x => x.RuleId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotionDatabaseRules");
        }
    }
}
