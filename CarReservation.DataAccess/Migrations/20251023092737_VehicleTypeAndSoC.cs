using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarReservation.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class VehicleTypeAndSoC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop FKs first
            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_Car_CarNumberPlate",
                table: "Reservation");

            migrationBuilder.DropForeignKey(
                name: "FK_Fault_Car_CarNumberPlate",
                table: "Fault");

            // Alter columns (now safe)
            migrationBuilder.AlterColumn<string>(
                name: "CarNumberPlate",
                table: "Reservation",
                type: "nvarchar(30)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)");

            migrationBuilder.AlterColumn<string>(
                name: "CarNumberPlate",
                table: "Fault",
                type: "nvarchar(30)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)");

            migrationBuilder.AlterColumn<string>(
                name: "NumberPlate",
                table: "Car",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            // Add VehicleType column
            migrationBuilder.AddColumn<int>(
                name: "VehicleType",
                table: "Car",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Recreate FKs
            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_Car_CarNumberPlate",
                table: "Reservation",
                column: "CarNumberPlate",
                principalTable: "Car",
                principalColumn: "NumberPlate",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Fault_Car_CarNumberPlate",
                table: "Fault",
                column: "CarNumberPlate",
                principalTable: "Car",
                principalColumn: "NumberPlate",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "f8f5369b-5950-487a-81f5-dd4a4bda5c52", "ce1846c2-3ea7-4fcb-b71e-88ffb1403338" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "5f171e11-1dff-49bf-8614-6468f99f5ebd", "e38b307b-3871-41c8-bd60-392514960c98" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "0ec8449f-c818-4ec1-8564-0babb0fac7da", "ed29889e-49f9-4524-8e18-aea9a2fe170c" });

            migrationBuilder.UpdateData(
                table: "Car",
                keyColumn: "NumberPlate",
                keyValue: "ABC123",
                column: "VehicleType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Car",
                keyColumn: "NumberPlate",
                keyValue: "DEF456",
                column: "VehicleType",
                value: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            // Drop new FKs first
            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_Car_CarNumberPlate",
                table: "Reservation");

            migrationBuilder.DropForeignKey(
                name: "FK_Fault_Car_CarNumberPlate",
                table: "Fault");

            // Drop VehicleType
            migrationBuilder.DropColumn(
                name: "VehicleType",
                table: "Car");

            // Revert column sizes
            migrationBuilder.AlterColumn<string>(
                name: "CarNumberPlate",
                table: "Reservation",
                type: "nvarchar(10)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)");

            migrationBuilder.AlterColumn<string>(
                name: "CarNumberPlate",
                table: "Fault",
                type: "nvarchar(10)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)");

            migrationBuilder.AlterColumn<string>(
                name: "NumberPlate",
                table: "Car",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            // Recreate original FKs
            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_Car_CarNumberPlate",
                table: "Reservation",
                column: "CarNumberPlate",
                principalTable: "Car",
                principalColumn: "NumberPlate",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Fault_Car_CarNumberPlate",
                table: "Fault",
                column: "CarNumberPlate",
                principalTable: "Car",
                principalColumn: "NumberPlate",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "be69cbea-169e-475c-8256-da1dfc4c01a1", "6421d8de-e580-4ba1-b814-dd018b8474b4" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "fe0ad373-5c0d-4a3c-b6da-e0dbfa0ebead", "5d7b2066-1168-4521-8eb8-21f7bf0cb6a1" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "361acb69-25a2-4ff9-977e-1203fcadba6f", "1397092a-63d9-4fa9-ae54-4a6d4db7c631" });
        }
    }
}
