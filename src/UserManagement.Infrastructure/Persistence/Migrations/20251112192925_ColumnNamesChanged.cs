using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ColumnNamesChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_EmailVerificationAttempt_AttemptCode",
                schema: "Identity",
                table: "EmailVerificationAttempt");

            migrationBuilder.RenameColumn(
                name: "OldEmail",
                schema: "Identity",
                table: "EmailVerificationAttempt",
                newName: "PreviousEmail");

            migrationBuilder.RenameColumn(
                name: "NewEmail",
                schema: "Identity",
                table: "EmailVerificationAttempt",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "AttemptCode",
                schema: "Identity",
                table: "EmailVerificationAttempt",
                newName: "VerificationCode");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_EmailVerificationAttempt_VerificationCode",
                schema: "Identity",
                table: "EmailVerificationAttempt",
                column: "VerificationCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_EmailVerificationAttempt_VerificationCode",
                schema: "Identity",
                table: "EmailVerificationAttempt");

            migrationBuilder.RenameColumn(
                name: "VerificationCode",
                schema: "Identity",
                table: "EmailVerificationAttempt",
                newName: "AttemptCode");

            migrationBuilder.RenameColumn(
                name: "PreviousEmail",
                schema: "Identity",
                table: "EmailVerificationAttempt",
                newName: "OldEmail");

            migrationBuilder.RenameColumn(
                name: "Email",
                schema: "Identity",
                table: "EmailVerificationAttempt",
                newName: "NewEmail");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_EmailVerificationAttempt_AttemptCode",
                schema: "Identity",
                table: "EmailVerificationAttempt",
                column: "AttemptCode");
        }
    }
}
