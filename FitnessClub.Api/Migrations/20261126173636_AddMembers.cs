using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FitnessClub.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    MembershipId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Members_Memberships_MembershipId",
                        column: x => x.MembershipId,
                        principalTable: "Memberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Members",
                columns: new[] { "Id", "BirthDate", "Email", "FirstName", "IsActive", "LastName", "MembershipId", "Phone" },
                values: new object[,]
                {
                    { 1, new DateOnly(1995, 4, 12), "ivanenko@gmail.com", "Олександр", true, "Іваненко", 1, "+380501112233" },
                    { 2, new DateOnly(2000, 8, 25), "petrenko@gmail.com", "Марія", true, "Петренко", 2, "+380671234567" },
                    { 3, new DateOnly(1988, 2, 5), "kovalenko@gmail.com", "Андрій", false, "Коваленко", 3, "+380931234567" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Members_Email",
                table: "Members",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_MembershipId",
                table: "Members",
                column: "MembershipId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Members_MemberId",
                table: "Bookings",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Members_MemberId",
                table: "Bookings");

            migrationBuilder.DropTable(
                name: "Members");
        }
    }
}
