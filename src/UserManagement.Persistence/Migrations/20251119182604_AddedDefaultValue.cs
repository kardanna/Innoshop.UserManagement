using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedDefaultValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_PasswordRestoreAttempt_AttemptCode",
                schema: "Identity",
                table: "PasswordRestoreAttempt");

            migrationBuilder.AlterColumn<bool>(
                name: "IsSucceeded",
                schema: "Identity",
                table: "PasswordRestoreAttempt",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                schema: "Identity",
                table: "User",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordRestoreAttempt_AttemptCode",
                schema: "Identity",
                table: "PasswordRestoreAttempt",
                column: "AttemptCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_Email",
                schema: "Identity",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_PasswordRestoreAttempt_AttemptCode",
                schema: "Identity",
                table: "PasswordRestoreAttempt");

            migrationBuilder.AlterColumn<bool>(
                name: "IsSucceeded",
                schema: "Identity",
                table: "PasswordRestoreAttempt",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_PasswordRestoreAttempt_AttemptCode",
                schema: "Identity",
                table: "PasswordRestoreAttempt",
                column: "AttemptCode");
        }
    }
}
