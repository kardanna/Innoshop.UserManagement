using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UserManagement.Persistence.Migrations
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
                name: "SigningKeys");

            migrationBuilder.EnsureSchema(
                name: "Authentication");

            migrationBuilder.CreateTable(
                name: "LoginAttempt",
                schema: "Identity",
                columns: table => new
                {
                    Email = table.Column<string>(type: "text", nullable: false),
                    AttemtedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeviceFingerprint = table.Column<string>(type: "text", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                    table.UniqueConstraint("AK_Role_Name", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "RsaKey",
                schema: "SigningKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicKeyPem = table.Column<string>(type: "text", nullable: false),
                    PrivateKeyPem = table.Column<string>(type: "text", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RsaKey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LastName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletionRequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailVerificationAttempt",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VerificationCode = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PreviousEmail = table.Column<string>(type: "text", nullable: true),
                    AttemptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsSucceeded = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SucceededAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailVerificationAttempt", x => x.Id);
                    table.UniqueConstraint("AK_EmailVerificationAttempt_VerificationCode", x => x.VerificationCode);
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptCode = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsSucceeded = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SucceededAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordRestoreAttempt", x => x.Id);
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
                    AccessTokenId = table.Column<Guid>(type: "uuid", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AccessTokenExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RefreshToken = table.Column<string>(type: "text", nullable: false),
                    DeviceFingerprint = table.Column<string>(type: "text", nullable: false)
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
                name: "UserDeactivation",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeactivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeactivationRequesterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Commentary = table.Column<string>(type: "text", nullable: false),
                    ReactivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReactivationRequesterId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDeactivation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDeactivation_User_DeactivationRequesterId",
                        column: x => x.DeactivationRequesterId,
                        principalSchema: "Identity",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserDeactivation_User_ReactivationRequesterId",
                        column: x => x.ReactivationRequesterId,
                        principalSchema: "Identity",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserDeactivation_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                schema: "Identity",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRole_Role_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "Identity",
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRole_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "Identity",
                table: "Role",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Administrator" },
                    { 2, "Customer" }
                });

            migrationBuilder.InsertData(
                schema: "Identity",
                table: "User",
                columns: new[] { "Id", "CreatedAt", "DateOfBirth", "DeletionRequestedAt", "Email", "FirstName", "IsEmailVerified", "LastModifiedAt", "LastName", "PasswordHash" },
                values: new object[,]
                {
                    { new Guid("160be924-907f-4d70-d15c-08de2383d454"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateOnly(2000, 1, 1), null, "ivan.ivanov@gmail.com", "Ivan", true, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ivanov", "AQAAAAIAAYagAAAAEDUID6axCz6cvyUWqrPGPCrA+Mm5w8K+1vSgeMrXoqk+NjrjeiCIS9IevKEbet2QdQ==" },
                    { new Guid("30fc2d9e-3bb0-4bdc-d15b-08de2383d454"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateOnly(2000, 1, 1), null, "admin@innoshop.by", "Admin", true, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Admin", "AQAAAAIAAYagAAAAEBZ2EtG4oB80p/B/1tWjr27MgHcqtVLPyaf7a/wnQsC7/rzf0J2fVO1jMhrGPy5vQw==" }
                });

            migrationBuilder.InsertData(
                schema: "Identity",
                table: "UserRole",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { 2, new Guid("160be924-907f-4d70-d15c-08de2383d454") },
                    { 1, new Guid("30fc2d9e-3bb0-4bdc-d15b-08de2383d454") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerificationAttempt_UserId",
                schema: "Identity",
                table: "EmailVerificationAttempt",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordRestoreAttempt_AttemptCode",
                schema: "Identity",
                table: "PasswordRestoreAttempt",
                column: "AttemptCode");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordRestoreAttempt_UserId",
                schema: "Identity",
                table: "PasswordRestoreAttempt",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenRecord_UserId",
                schema: "Authentication",
                table: "TokenRecord",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                schema: "Identity",
                table: "User",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_UserDeactivation_DeactivationRequesterId",
                schema: "Identity",
                table: "UserDeactivation",
                column: "DeactivationRequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDeactivation_ReactivationRequesterId",
                schema: "Identity",
                table: "UserDeactivation",
                column: "ReactivationRequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDeactivation_UserId",
                schema: "Identity",
                table: "UserDeactivation",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleId",
                schema: "Identity",
                table: "UserRole",
                column: "RoleId");
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
                name: "RsaKey",
                schema: "SigningKeys");

            migrationBuilder.DropTable(
                name: "TokenRecord",
                schema: "Authentication");

            migrationBuilder.DropTable(
                name: "UserDeactivation",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "UserRole",
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
