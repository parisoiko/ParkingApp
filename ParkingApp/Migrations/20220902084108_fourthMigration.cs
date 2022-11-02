using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingApp.Migrations
{
    public partial class fourthMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "ParkingVehicle",
                newName: "FullName");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "ParkingActivity",
                newName: "FullName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "ParkingVehicle",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "ParkingActivity",
                newName: "Name");
        }
    }
}
