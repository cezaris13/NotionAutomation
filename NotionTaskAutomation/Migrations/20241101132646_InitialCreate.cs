using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotionTaskAutomation.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotionPageRules",
                columns: table => new
                {
                    RuleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartingState = table.Column<string>(type: "TEXT", nullable: true),
                    EndingState = table.Column<string>(type: "TEXT", nullable: true),
                    OnDay = table.Column<string>(type: "TEXT", nullable: true),
                    DayOffset = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotionPageRules", x => x.RuleId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotionPageRules");
        }
    }
}
