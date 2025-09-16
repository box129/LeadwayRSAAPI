using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leadway_RSA_API.Migrations
{
    /// <inheritdoc />
    public partial class RefinedApplicantmodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HashedPassword",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Applicants");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HashedPassword",
                table: "Applicants",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Applicants",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
