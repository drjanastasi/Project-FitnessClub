using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessClub.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutClub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClubId",
                table: "Workouts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Workouts",
                keyColumn: "Id",
                keyValue: 1,
                column: "ClubId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Workouts",
                keyColumn: "Id",
                keyValue: 2,
                column: "ClubId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Workouts",
                keyColumn: "Id",
                keyValue: 3,
                column: "ClubId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Workouts",
                keyColumn: "Id",
                keyValue: 4,
                column: "ClubId",
                value: 2);

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_ClubId",
                table: "Workouts",
                column: "ClubId");

            migrationBuilder.AddForeignKey(
                name: "FK_Workouts_Clubs_ClubId",
                table: "Workouts",
                column: "ClubId",
                principalTable: "Clubs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workouts_Clubs_ClubId",
                table: "Workouts");

            migrationBuilder.DropIndex(
                name: "IX_Workouts_ClubId",
                table: "Workouts");

            migrationBuilder.DropColumn(
                name: "ClubId",
                table: "Workouts");
        }
    }
}
