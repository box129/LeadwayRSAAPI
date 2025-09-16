using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Leadway_RSA_API.Migrations
{
    /// <inheritdoc />
    public partial class FinalSchemaUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "AssetType",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "Assets");

            migrationBuilder.RenameColumn(
                name: "BankName",
                table: "Assets",
                newName: "SalaryBankName");

            migrationBuilder.RenameColumn(
                name: "AccountType",
                table: "Assets",
                newName: "SalaryAccountNumber");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "PaymentTransactions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "PaymentTransactions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.Sql("ALTER TABLE \"Identifications\" ALTER COLUMN \"IdentificationType\" TYPE integer USING CASE \"IdentificationType\" WHEN 'Passport' THEN 0 WHEN 'DriversLicense' THEN 1 WHEN 'VotersCard' THEN 2 WHEN 'BVN' THEN 3 ELSE 0 END;");

            migrationBuilder.AlterColumn<string>(
                name: "DocumentNumber",
                table: "Identifications",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Identifications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "Identifications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Guardians",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "Guardians",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Relationship",
                table: "Guardians",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("ALTER TABLE \"Executors\" ALTER COLUMN \"ExecutorType\" TYPE integer USING CASE \"ExecutorType\" WHEN 'Individual' THEN 0 WHEN 'Company' THEN 1 ELSE 0 END;");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Executors",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "Executors",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "Executors",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Executors",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "BeneficiaryAssetAllocations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "BeneficiaryAssetAllocations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Beneficiaries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Beneficiaries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "Beneficiaries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Assets",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "Assets",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

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

            migrationBuilder.CreateTable(
                name: "PersonalDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicantId = table.Column<int>(type: "integer", nullable: false),
                    PlaceOfBirth = table.Column<string>(type: "text", nullable: false),
                    Religion = table.Column<string>(type: "text", nullable: false),
                    Gender = table.Column<string>(type: "text", nullable: false),
                    HomeAddress = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    PassportPhotoPath = table.Column<string>(type: "text", nullable: true),
                    SignaturePath = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalDetails_Applicants_ApplicantId",
                        column: x => x.ApplicantId,
                        principalTable: "Applicants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Token = table.Column<string>(type: "text", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApplicantId = table.Column<int>(type: "integer", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Applicants_ApplicantId",
                        column: x => x.ApplicantId,
                        principalTable: "Applicants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistrationKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "text", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApplicantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrationKeys_Applicants_ApplicantId",
                        column: x => x.ApplicantId,
                        principalTable: "Applicants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonalDetails_ApplicantId",
                table: "PersonalDetails",
                column: "ApplicantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ApplicantId",
                table: "RefreshTokens",
                column: "ApplicantId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationKeys_ApplicantId",
                table: "RegistrationKeys",
                column: "ApplicantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationKeys_Key",
                table: "RegistrationKeys",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonalDetails");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "RegistrationKeys");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Identifications");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "Identifications");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Guardians");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "Guardians");

            migrationBuilder.DropColumn(
                name: "Relationship",
                table: "Guardians");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Executors");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "Executors");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "Executors");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Executors");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "BeneficiaryAssetAllocations");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "BeneficiaryAssetAllocations");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "HashedPassword",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Applicants");

            migrationBuilder.RenameColumn(
                name: "SalaryBankName",
                table: "Assets",
                newName: "BankName");

            migrationBuilder.RenameColumn(
                name: "SalaryAccountNumber",
                table: "Assets",
                newName: "AccountType");

            migrationBuilder.AlterColumn<string>(
                name: "IdentificationType",
                table: "Identifications",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "DocumentNumber",
                table: "Identifications",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExecutorType",
                table: "Executors",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "Assets",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssetType",
                table: "Assets",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Value",
                table: "Assets",
                type: "numeric",
                nullable: true);
        }
    }
}
