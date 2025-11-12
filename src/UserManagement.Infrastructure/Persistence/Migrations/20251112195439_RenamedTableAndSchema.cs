using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenamedTableAndSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Signing",
                schema: "RsaKey",
                table: "Signing");

            migrationBuilder.EnsureSchema(
                name: "SigningKeys");

            migrationBuilder.RenameTable(
                name: "Signing",
                schema: "RsaKey",
                newName: "RsaKey",
                newSchema: "SigningKeys");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RsaKey",
                schema: "SigningKeys",
                table: "RsaKey",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RsaKey",
                schema: "SigningKeys",
                table: "RsaKey");

            migrationBuilder.EnsureSchema(
                name: "RsaKey");

            migrationBuilder.RenameTable(
                name: "RsaKey",
                schema: "SigningKeys",
                newName: "Signing",
                newSchema: "RsaKey");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Signing",
                schema: "RsaKey",
                table: "Signing",
                column: "Id");
        }
    }
}
