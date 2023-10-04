using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitArenaMiner.Migrations
{
    /// <inheritdoc />
    public partial class indexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_MessageReactionCounts_IdSender_EmoteIdentifier",
                table: "MessageReactionCounts",
                columns: new[] { "IdSender", "EmoteIdentifier" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MessageReactionCounts_IdSender_EmoteIdentifier",
                table: "MessageReactionCounts");
        }
    }
}
