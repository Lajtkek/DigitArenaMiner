using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitArenaMiner.Migrations
{
    /// <inheritdoc />
    public partial class userActionCounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserActionCounts",
                columns: table => new
                {
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ActionName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActionCounts", x => new { x.UserId, x.ActionName });
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserActionCounts_UserId_ActionName",
                table: "UserActionCounts",
                columns: new[] { "UserId", "ActionName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserActionCounts");
        }
    }
}
