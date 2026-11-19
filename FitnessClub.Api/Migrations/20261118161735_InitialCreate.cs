using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FitnessClub.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Memberships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Price = table.Column<double>(type: "REAL", nullable: false),
                    DurationDays = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Memberships", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Workouts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TrainerId = table.Column<int>(type: "INTEGER", nullable: false),
                    DayOfWeek = table.Column<int>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    DurationMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    MaxParticipants = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workouts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MembershipWorkouts",
                columns: table => new
                {
                    MembershipId = table.Column<int>(type: "INTEGER", nullable: false),
                    WorkoutId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipWorkouts", x => new { x.MembershipId, x.WorkoutId });
                    table.ForeignKey(
                        name: "FK_MembershipWorkouts_Memberships_MembershipId",
                        column: x => x.MembershipId,
                        principalTable: "Memberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MembershipWorkouts_Workouts_WorkoutId",
                        column: x => x.WorkoutId,
                        principalTable: "Workouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Memberships",
                columns: new[] { "Id", "Description", "DurationDays", "IsActive", "Name", "Price" },
                values: new object[,]
                {
                    { 1, "Одне відвідування тренажерного залу", 1, true, "Разовий візит", 200.0 },
                    { 2, "Необмежене відвідування залу та всіх групових тренувань", 30, true, "Місячний безліміт", 1200.0 },
                    { 3, "Відвідування з 7:00 до 12:00 і ранкові групові тренування", 30, true, "Ранковий", 800.0 },
                    { 4, "Безліміт + 4 персональні тренування на місяць", 365, false, "Річний преміум", 12000.0 }
                });

            migrationBuilder.InsertData(
                table: "Workouts",
                columns: new[] { "Id", "DayOfWeek", "Description", "DurationMinutes", "Level", "MaxParticipants", "Name", "StartTime", "TrainerId" },
                values: new object[,]
                {
                    { 1, 1, "Хатха-йога для розтяжки та розслаблення", 60, "Beginner", 15, "Йога", new TimeOnly(18, 0, 0), 1 },
                    { 2, 2, "Кругове тренування з власною вагою", 45, "Intermediate", 12, "Функціональний тренінг", new TimeOnly(19, 0, 0), 2 },
                    { 3, 3, "Зміцнення м'язів кору", 50, "Beginner", 10, "Пілатес", new TimeOnly(9, 0, 0), 1 },
                    { 4, 5, "Високоінтенсивне тренування", 60, "Advanced", 8, "Кросфіт", new TimeOnly(20, 0, 0), 3 }
                });

            migrationBuilder.InsertData(
                table: "MembershipWorkouts",
                columns: new[] { "MembershipId", "WorkoutId" },
                values: new object[,]
                {
                    { 2, 1 },
                    { 2, 2 },
                    { 2, 3 },
                    { 2, 4 },
                    { 3, 3 },
                    { 4, 1 },
                    { 4, 2 },
                    { 4, 3 },
                    { 4, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_Name",
                table: "Memberships",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipWorkouts_WorkoutId",
                table: "MembershipWorkouts",
                column: "WorkoutId");

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_DayOfWeek",
                table: "Workouts",
                column: "DayOfWeek");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MembershipWorkouts");

            migrationBuilder.DropTable(
                name: "Memberships");

            migrationBuilder.DropTable(
                name: "Workouts");
        }
    }
}
