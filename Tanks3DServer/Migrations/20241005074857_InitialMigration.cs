using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tanks3DServer.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AuthCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WalletAddres = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dumpprivkey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InventoryJSon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProccesedTransactionsJSon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
