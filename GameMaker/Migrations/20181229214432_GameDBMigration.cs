using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GameMaker.Migrations
{
    public partial class GameDBMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    SessonId = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    NumberOfPlayers = table.Column<int>(nullable: false),
                    Gamestate = table.Column<int>(nullable: false),
                    TurnPlayerId = table.Column<Guid>(nullable: false),
                    MaxAllowedPlayer = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.SessonId);
                });

            migrationBuilder.CreateTable(
                name: "Player",
                columns: table => new
                {
                    PlayerName = table.Column<string>(nullable: true),
                    PlayerId = table.Column<Guid>(nullable: false),
                    PlayerScore = table.Column<int>(nullable: false),
                    PlayerState = table.Column<int>(nullable: false),
                    GameSessonId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Player", x => x.PlayerId);
                    table.ForeignKey(
                        name: "FK_Player_Games_GameSessonId",
                        column: x => x.GameSessonId,
                        principalTable: "Games",
                        principalColumn: "SessonId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Player_GameSessonId",
                table: "Player",
                column: "GameSessonId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Player");

            migrationBuilder.DropTable(
                name: "Games");
        }
    }
}
