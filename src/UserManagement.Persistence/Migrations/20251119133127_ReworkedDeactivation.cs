using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReworkedDeactivation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeactivationRequestedAt",
                schema: "Identity",
                table: "User");

            migrationBuilder.DropColumn(
                name: "IsDeactivated",
                schema: "Identity",
                table: "User");

            migrationBuilder.CreateTable(
                name: "UserDeactivation",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeactivatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeactivationRequesterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Commentary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReactivatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReactivationRequesterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDeactivation",
                schema: "Identity");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivationRequestedAt",
                schema: "Identity",
                table: "User",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeactivated",
                schema: "Identity",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                schema: "Identity",
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("160be924-907f-4d70-d15c-08de2383d454"),
                column: "DeactivationRequestedAt",
                value: null);

            migrationBuilder.UpdateData(
                schema: "Identity",
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("30fc2d9e-3bb0-4bdc-d15b-08de2383d454"),
                column: "DeactivationRequestedAt",
                value: null);
        }
    }
}
