using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class Initialize5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompletedAchievement_GameWorlds_GameWorldDataId",
                table: "CompletedAchievement");

            migrationBuilder.RenameColumn(
                name: "GameWorldDataId",
                table: "CompletedAchievement",
                newName: "GameWorldId");

            migrationBuilder.RenameIndex(
                name: "IX_CompletedAchievement_GameWorldDataId",
                table: "CompletedAchievement",
                newName: "IX_CompletedAchievement_GameWorldId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompletedAchievement_GameWorlds_GameWorldId",
                table: "CompletedAchievement",
                column: "GameWorldId",
                principalTable: "GameWorlds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompletedAchievement_GameWorlds_GameWorldId",
                table: "CompletedAchievement");

            migrationBuilder.RenameColumn(
                name: "GameWorldId",
                table: "CompletedAchievement",
                newName: "GameWorldDataId");

            migrationBuilder.RenameIndex(
                name: "IX_CompletedAchievement_GameWorldId",
                table: "CompletedAchievement",
                newName: "IX_CompletedAchievement_GameWorldDataId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompletedAchievement_GameWorlds_GameWorldDataId",
                table: "CompletedAchievement",
                column: "GameWorldDataId",
                principalTable: "GameWorlds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
