using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adex.Data.Model.Migrations
{
    /// <inheritdoc />
    public partial class FinancialLinkTypeTotals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinancialLinkTypeTotals",
                columns: table => new
                {
                    Type = table.Column<string>(type: "text", nullable: false),
                    Count = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialLinkTypeTotals", x => x.Type);
                });

            migrationBuilder.Sql("""
                INSERT INTO "FinancialLinkTypeTotals" ("Type", "Count", "Amount")
                SELECT COALESCE("DeclarationType", 'Non renseigné'), COUNT(*), COALESCE(SUM("Amount"), 0)
                FROM "FinancialLinks"
                GROUP BY 1
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinancialLinkTypeTotals");
        }
    }
}
