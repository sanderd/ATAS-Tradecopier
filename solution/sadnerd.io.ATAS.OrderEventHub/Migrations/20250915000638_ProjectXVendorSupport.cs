using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sadnerd.io.ATAS.OrderEventHub.Migrations
{
    /// <inheritdoc />
    public partial class ProjectXVendorSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CopyStrategies_TopstepAccount_TopstepAccountId",
                table: "CopyStrategies");

            migrationBuilder.DropTable(
                name: "TopstepAccount");

            migrationBuilder.RenameColumn(
                name: "TopstepContract",
                table: "CopyStrategies",
                newName: "ProjectXContract");

            migrationBuilder.RenameColumn(
                name: "TopstepAccountId",
                table: "CopyStrategies",
                newName: "ProjectXAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_CopyStrategies_TopstepAccountId",
                table: "CopyStrategies",
                newName: "IX_CopyStrategies_ProjectXAccountId");

            migrationBuilder.CreateTable(
                name: "ProjectXAccounts",
                columns: table => new
                {
                    ProjectXAccountId = table.Column<string>(type: "TEXT", nullable: false),
                    Vendor = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectXAccounts", x => x.ProjectXAccountId);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_CopyStrategies_ProjectXAccounts_ProjectXAccountId",
                table: "CopyStrategies",
                column: "ProjectXAccountId",
                principalTable: "ProjectXAccounts",
                principalColumn: "ProjectXAccountId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CopyStrategies_ProjectXAccounts_ProjectXAccountId",
                table: "CopyStrategies");

            migrationBuilder.DropTable(
                name: "ProjectXAccounts");

            migrationBuilder.RenameColumn(
                name: "ProjectXContract",
                table: "CopyStrategies",
                newName: "TopstepContract");

            migrationBuilder.RenameColumn(
                name: "ProjectXAccountId",
                table: "CopyStrategies",
                newName: "TopstepAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_CopyStrategies_ProjectXAccountId",
                table: "CopyStrategies",
                newName: "IX_CopyStrategies_TopstepAccountId");

            migrationBuilder.CreateTable(
                name: "TopstepAccount",
                columns: table => new
                {
                    TopstepAccountId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopstepAccount", x => x.TopstepAccountId);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_CopyStrategies_TopstepAccount_TopstepAccountId",
                table: "CopyStrategies",
                column: "TopstepAccountId",
                principalTable: "TopstepAccount",
                principalColumn: "TopstepAccountId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
