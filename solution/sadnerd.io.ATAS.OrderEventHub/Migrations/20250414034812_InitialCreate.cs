using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sadnerd.io.ATAS.OrderEventHub.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AtasAccounts",
                columns: table => new
                {
                    AtasAccountId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtasAccounts", x => x.AtasAccountId);
                });

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

            migrationBuilder.CreateTable(
                name: "CopyStrategies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AtasAccountId = table.Column<string>(type: "TEXT", nullable: false),
                    TopstepAccountId = table.Column<string>(type: "TEXT", nullable: false),
                    AtasContract = table.Column<string>(type: "TEXT", nullable: false),
                    TopstepContract = table.Column<string>(type: "TEXT", nullable: false),
                    ContractMultiplier = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CopyStrategies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CopyStrategies_AtasAccounts_AtasAccountId",
                        column: x => x.AtasAccountId,
                        principalTable: "AtasAccounts",
                        principalColumn: "AtasAccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CopyStrategies_TopstepAccount_TopstepAccountId",
                        column: x => x.TopstepAccountId,
                        principalTable: "TopstepAccount",
                        principalColumn: "TopstepAccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CopyStrategies_AtasAccountId",
                table: "CopyStrategies",
                column: "AtasAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CopyStrategies_TopstepAccountId",
                table: "CopyStrategies",
                column: "TopstepAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CopyStrategies");

            migrationBuilder.DropTable(
                name: "AtasAccounts");

            migrationBuilder.DropTable(
                name: "TopstepAccount");
        }
    }
}
