using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class Initialize4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompletedAchievement_GameWorlds_GameWorldDataId",
                table: "CompletedAchievement");

            migrationBuilder.AlterColumn<Guid>(
                name: "GameWorldDataId",
                table: "CompletedAchievement",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CompletedAchievement_GameWorlds_GameWorldDataId",
                table: "CompletedAchievement",
                column: "GameWorldDataId",
                principalTable: "GameWorlds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompletedAchievement_GameWorlds_GameWorldDataId",
                table: "CompletedAchievement");

            migrationBuilder.AlterColumn<Guid>(
                name: "GameWorldDataId",
                table: "CompletedAchievement",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_CompletedAchievement_GameWorlds_GameWorldDataId",
                table: "CompletedAchievement",
                column: "GameWorldDataId",
                principalTable: "GameWorlds",
                principalColumn: "Id");
        }
    }
}
