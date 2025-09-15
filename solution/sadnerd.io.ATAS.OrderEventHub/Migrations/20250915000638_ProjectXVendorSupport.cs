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
            // Step 1: Create the new ProjectXAccounts table
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

            // Step 2: Migrate data from TopstepAccount to ProjectXAccounts
            migrationBuilder.Sql(
                @"INSERT INTO ProjectXAccounts (ProjectXAccountId, Vendor)
                  SELECT TopstepAccountId, 1 AS Vendor -- Default to TopstepX
                  FROM TopstepAccount"
            );

            // Step 3: Update CopyStrategies to reference ProjectXAccounts
            migrationBuilder.RenameColumn(
                name: "TopstepAccountId",
                table: "CopyStrategies",
                newName: "ProjectXAccountId"
            );

            migrationBuilder.RenameColumn(
                name: "TopstepContract",
                table: "CopyStrategies",
                newName: "ProjectXContract"
            );

            migrationBuilder.RenameIndex(
                name: "IX_CopyStrategies_TopstepAccountId",
                table: "CopyStrategies",
                newName: "IX_CopyStrategies_ProjectXAccountId"
            );

            // Step 4: Add foreign key constraint to ProjectXAccounts
            migrationBuilder.AddForeignKey(
                name: "FK_CopyStrategies_ProjectXAccounts_ProjectXAccountId",
                table: "CopyStrategies",
                column: "ProjectXAccountId",
                principalTable: "ProjectXAccounts",
                principalColumn: "ProjectXAccountId",
                onDelete: ReferentialAction.Cascade
            );

            // Step 5: Drop the old TopstepAccount table
            migrationBuilder.DropTable(
                name: "TopstepAccount"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Step 1: Recreate the TopstepAccount table
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

            // Step 2: Migrate data back from ProjectXAccounts to TopstepAccount
            migrationBuilder.Sql(@"INSERT INTO TopstepAccount (TopstepAccountId)
                  SELECT ProjectXAccountId
                  FROM ProjectXAccounts
                  WHERE Vendor = 1 -- Only migrate TopstepX accounts back");

            // Step 3: Revert CopyStrategies to reference TopstepAccount
            migrationBuilder.RenameColumn(
                name: "ProjectXAccountId",
                table: "CopyStrategies",
                newName: "TopstepAccountId"
            );

            migrationBuilder.RenameColumn(
                name: "ProjectXContract",
                table: "CopyStrategies",
                newName: "TopstepContract"
            );

            migrationBuilder.RenameIndex(
                name: "IX_CopyStrategies_ProjectXAccountId",
                table: "CopyStrategies",
                newName: "IX_CopyStrategies_TopstepAccountId"
            );

            // Step 4: Add foreign key constraint back to TopstepAccount
            migrationBuilder.AddForeignKey(
                name: "FK_CopyStrategies_TopstepAccount_TopstepAccountId",
                table: "CopyStrategies",
                column: "TopstepAccountId",
                principalTable: "TopstepAccount",
                principalColumn: "TopstepAccountId",
                onDelete: ReferentialAction.Cascade
            );

            // Step 5: Drop the ProjectXAccounts table
            migrationBuilder.DropTable(
                name: "ProjectXAccounts"
            );
        }
    }
}
