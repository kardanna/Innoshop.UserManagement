using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TableRefactoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiresAt",
                schema: "Authentication",
                table: "TokenRecord");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                schema: "RsaKey",
                table: "Signing");

            migrationBuilder.DropColumn(
                name: "SigningExpiresAt",
                schema: "RsaKey",
                table: "Signing");

            migrationBuilder.AddColumn<string>(
                name: "DeviceFingerprint",
                schema: "Identity",
                table: "LoginAttempt",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsSucceeded",
                schema: "Identity",
                table: "EmailVerificationAttempt",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceFingerprint",
                schema: "Identity",
                table: "LoginAttempt");

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiresAt",
                schema: "Authentication",
                table: "TokenRecord",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                schema: "RsaKey",
                table: "Signing",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "SigningExpiresAt",
                schema: "RsaKey",
                table: "Signing",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<bool>(
                name: "IsSucceeded",
                schema: "Identity",
                table: "EmailVerificationAttempt",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);
        }
    }
}
