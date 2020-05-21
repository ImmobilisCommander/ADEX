using Microsoft.EntityFrameworkCore.Migrations;

namespace Adex.Model.Migrations
{
    public partial class RenameAndAddColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Designation",
                table: "Companies",
                newName: "Name");

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Companies",
                maxLength: 200,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Companies");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Companies",
                newName: "Designation");
        }
    }
}
