using Microsoft.EntityFrameworkCore.Migrations;

namespace GameMaker.Migrations
{
    public partial class GameDBMigration6 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlayerScoreUncommited",
                table: "Player",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlayerScoreUncommited",
                table: "Player");
        }
    }
}
