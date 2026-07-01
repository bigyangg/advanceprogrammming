using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CEMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImagePathProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Venues",
                type: "TEXT",
                maxLength: 255,
                nullable: true,
                defaultValue: "/images/venues/img1_auditorium.png");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Events",
                type: "TEXT",
                maxLength: 255,
                nullable: true,
                defaultValue: "/images/events/img1_workshop.png");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Activities",
                type: "TEXT",
                maxLength: 255,
                nullable: true,
                defaultValue: "/images/activities/img1_talk.png");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Activities");
        }
    }
}
