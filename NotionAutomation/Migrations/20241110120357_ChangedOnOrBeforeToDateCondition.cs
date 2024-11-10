using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotionAutomation.Migrations
{
    /// <inheritdoc />
    public partial class ChangedOnOrBeforeToDateCondition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OnDay",
                table: "NotionDatabaseRules");

            migrationBuilder.AddColumn<int>(
                name: "DateCondition",
                table: "NotionDatabaseRules",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateCondition",
                table: "NotionDatabaseRules");

            migrationBuilder.AddColumn<string>(
                name: "OnDay",
                table: "NotionDatabaseRules",
                type: "text",
                nullable: true);
        }
    }
}
