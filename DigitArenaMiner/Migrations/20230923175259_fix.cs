using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitArenaMiner.Migrations
{
    /// <inheritdoc />
    public partial class fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MessageReactionCounts",
                table: "MessageReactionCounts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MessageReactionCounts",
                table: "MessageReactionCounts",
                columns: new[] { "IdMessage", "EmoteIdentifier" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MessageReactionCounts",
                table: "MessageReactionCounts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MessageReactionCounts",
                table: "MessageReactionCounts",
                column: "IdMessage");
        }
    }
}
