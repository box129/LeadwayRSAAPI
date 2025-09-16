using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leadway_RSA_API.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicantNavigationToRegistrationKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExpirationDate",
                table: "RegistrationKeys",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "RegistrationKeys",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "RegistrationKeys");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "RegistrationKeys",
                newName: "ExpirationDate");
        }
    }
}
