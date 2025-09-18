using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sadnerd.io.ATAS.OrderEventHub.Migrations
{
    /// <inheritdoc />
    public partial class AddApiCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApiCredentialId",
                table: "ProjectXAccounts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProjectXApiCredentials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Vendor = table.Column<int>(type: "INTEGER", nullable: false),
                    ApiKey = table.Column<string>(type: "TEXT", nullable: false),
                    ApiUser = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectXApiCredentials", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectXAccounts_ApiCredentialId",
                table: "ProjectXAccounts",
                column: "ApiCredentialId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectXApiCredentials_Vendor_DisplayName",
                table: "ProjectXApiCredentials",
                columns: new[] { "Vendor", "DisplayName" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectXAccounts_ProjectXApiCredentials_ApiCredentialId",
                table: "ProjectXAccounts",
                column: "ApiCredentialId",
                principalTable: "ProjectXApiCredentials",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectXAccounts_ProjectXApiCredentials_ApiCredentialId",
                table: "ProjectXAccounts");

            migrationBuilder.DropTable(
                name: "ProjectXApiCredentials");

            migrationBuilder.DropIndex(
                name: "IX_ProjectXAccounts_ApiCredentialId",
                table: "ProjectXAccounts");

            migrationBuilder.DropColumn(
                name: "ApiCredentialId",
                table: "ProjectXAccounts");
        }
    }
}
