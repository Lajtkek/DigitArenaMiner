using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitArenaMiner.Migrations
{
    /// <inheritdoc />
    public partial class init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MessageReactionCounts",
                table: "MessageReactionCounts");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "MessageReactionCounts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MessageReactionCounts",
                table: "MessageReactionCounts",
                column: "IdMessage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MessageReactionCounts",
                table: "MessageReactionCounts");

            migrationBuilder.AddColumn<decimal>(
                name: "Id",
                table: "MessageReactionCounts",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MessageReactionCounts",
                table: "MessageReactionCounts",
                column: "Id");
        }
    }
}
