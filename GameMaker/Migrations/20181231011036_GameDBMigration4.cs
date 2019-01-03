using Microsoft.EntityFrameworkCore.Migrations;

namespace GameMaker.Migrations
{
    public partial class GameDBMigration4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlayerAlias",
                table: "Player",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlayerAlias",
                table: "Player");
        }
    }
}
