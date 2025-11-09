using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PickleBallBooking.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeIntoPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeOnly>(
                name: "EndTime",
                table: "Pricing",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "StartTime",
                table: "Pricing",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.CreateIndex(
                name: "IX_Pricing_DayOfWeek_StartTime",
                table: "Pricing",
                columns: new[] { "DayOfWeek", "StartTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pricing_DayOfWeek_StartTime",
                table: "Pricing");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Pricing");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Pricing");
        }
    }
}
