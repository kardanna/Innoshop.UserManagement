using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Identity");

            migrationBuilder.EnsureSchema(
                name: "RsaKey");

            migrationBuilder.EnsureSchema(
                name: "Authentication");

            migrationBuilder.CreateTable(
                name: "Claim",
                schema: "Identity",
                columns: table => new
                {
                    Type = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Claim", x => new { x.Type, x.Value });
                });

            migrationBuilder.CreateTable(
                name: "LoginAttempt",
                schema: "Identity",
                columns: table => new
                {
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AttemtedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginAttempt", x => new { x.Email, x.AttemtedAt });
                });

            migrationBuilder.CreateTable(
                name: "Role",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                    table.UniqueConstraint("AK_Role_Name", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Signing",
                schema: "RsaKey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PublicKeyPem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrivateKeyPem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SigningExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Signing", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeactivated = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeactivationRequestedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletionRequestedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.UniqueConstraint("AK_User_Email", x => x.Email);
                });

            migrationBuilder.CreateTable(
                name: "RoleClaimUserRole",
                schema: "Identity",
                columns: table => new
                {
                    RolesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimsType = table.Column<string>(type: "nvarchar(15)", nullable: false),
                    ClaimsValue = table.Column<string>(type: "nvarchar(40)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaimUserRole", x => new { x.RolesId, x.ClaimsType, x.ClaimsValue });
                    table.ForeignKey(
                        name: "FK_RoleClaimUserRole_Claim_ClaimsType_ClaimsValue",
                        columns: x => new { x.ClaimsType, x.ClaimsValue },
                        principalSchema: "Identity",
                        principalTable: "Claim",
                        principalColumns: new[] { "Type", "Value" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleClaimUserRole_Role_RolesId",
                        column: x => x.RolesId,
                        principalSchema: "Identity",
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailVerificationAttempt",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttemptCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NewEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttemptedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSucceeded = table.Column<bool>(type: "bit", nullable: false),
                    SucceededAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailVerificationAttempt", x => x.Id);
                    table.UniqueConstraint("AK_EmailVerificationAttempt_AttemptCode", x => x.AttemptCode);
                    table.ForeignKey(
                        name: "FK_EmailVerificationAttempt_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordRestoreAttempt",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttemptCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttemptedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSucceeded = table.Column<bool>(type: "bit", nullable: false),
                    SucceededAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordRestoreAttempt", x => x.Id);
                    table.UniqueConstraint("AK_PasswordRestoreAttempt_AttemptCode", x => x.AttemptCode);
                    table.ForeignKey(
                        name: "FK_PasswordRestoreAttempt_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TokenRecord",
                schema: "Authentication",
                columns: table => new
                {
                    AccessTokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AccessTokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefreshTokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeviceFingerprint = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenRecord", x => x.AccessTokenId);
                    table.ForeignKey(
                        name: "FK_TokenRecord_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserUserRole",
                schema: "Identity",
                columns: table => new
                {
                    RolesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsersId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserUserRole", x => new { x.RolesId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_UserUserRole_Role_RolesId",
                        column: x => x.RolesId,
                        principalSchema: "Identity",
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserUserRole_User_UsersId",
                        column: x => x.UsersId,
                        principalSchema: "Identity",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "Identity",
                table: "User",
                columns: new[] { "Id", "CreatedAt", "DateOfBirth", "DeactivationRequestedAt", "DeletionRequestedAt", "Email", "FirstName", "IsEmailVerified", "LastModifiedAt", "LastName", "PasswordHash" },
                values: new object[] { new Guid("2f6ba6b8-e14d-4b05-942d-e2c1344ce708"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateOnly(1, 1, 1), null, null, "ivan.ivanov@gmail.com", "Ivan", true, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ivanov", "123456" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerificationAttempt_UserId",
                schema: "Identity",
                table: "EmailVerificationAttempt",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordRestoreAttempt_UserId",
                schema: "Identity",
                table: "PasswordRestoreAttempt",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaimUserRole_ClaimsType_ClaimsValue",
                schema: "Identity",
                table: "RoleClaimUserRole",
                columns: new[] { "ClaimsType", "ClaimsValue" });

            migrationBuilder.CreateIndex(
                name: "IX_TokenRecord_UserId",
                schema: "Authentication",
                table: "TokenRecord",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserUserRole_UsersId",
                schema: "Identity",
                table: "UserUserRole",
                column: "UsersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailVerificationAttempt",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "LoginAttempt",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "PasswordRestoreAttempt",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "RoleClaimUserRole",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "Signing",
                schema: "RsaKey");

            migrationBuilder.DropTable(
                name: "TokenRecord",
                schema: "Authentication");

            migrationBuilder.DropTable(
                name: "UserUserRole",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "Claim",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "Role",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "User",
                schema: "Identity");
        }
    }
}
