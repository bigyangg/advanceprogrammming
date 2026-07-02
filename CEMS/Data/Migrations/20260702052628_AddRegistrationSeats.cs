using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CEMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrationSeats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Seats",
                table: "Registrations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.UpdateData(
                table: "Registrations",
                keyColumn: "RegistrationId",
                keyValue: 1,
                column: "Seats",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Registrations",
                keyColumn: "RegistrationId",
                keyValue: 2,
                column: "Seats",
                value: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Seats",
                table: "Registrations");
        }
    }
}
