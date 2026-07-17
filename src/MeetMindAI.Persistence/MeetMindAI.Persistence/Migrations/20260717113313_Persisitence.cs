using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetMindAI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Persisitence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MeetingSummaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MeetingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    Provider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PromptVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GeneratedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRegenerated = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingSummaries_Meeting_MeetingId",
                        column: x => x.MeetingId,
                        principalTable: "Meeting",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeetingSummaries_MeetingId",
                table: "MeetingSummaries",
                column: "MeetingId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeetingSummaries");
        }
    }
}
