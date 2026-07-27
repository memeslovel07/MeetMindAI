using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetMindAI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddActionItemSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Source",
                table: "ActionItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Source",
                table: "ActionItems");
        }
    }
}
