using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class AcheivementUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AchievementCode",
                table: "CompletedAchievement",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_CompletedAchievement_AchievementCode_GameWorldId",
                table: "CompletedAchievement",
                columns: new[] { "AchievementCode", "GameWorldId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompletedAchievement_AchievementCode_GameWorldId",
                table: "CompletedAchievement");

            migrationBuilder.AlterColumn<string>(
                name: "AchievementCode",
                table: "CompletedAchievement",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
