using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitArenaMiner.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArchivedMessages",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivedMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MessageReactionCounts",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    IdMessage = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    IdSender = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    EmoteIdentifier = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageReactionCounts", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchivedMessages");

            migrationBuilder.DropTable(
                name: "MessageReactionCounts");
        }
    }
}
