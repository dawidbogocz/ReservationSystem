using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarReservation.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MileageAndFuelLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FuelLevel",
                table: "Car",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Mileage",
                table: "Car",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "59fb49fe-15c0-41b8-926b-ac8b8e45fd11", "83e2b07a-9989-4e59-b6b4-622338588b71" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "35b91c34-83cd-4083-aed3-edf32f49e0b3", "b33e708f-c1ee-4fff-84c8-e6ccd1d4934f" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "b7399ffa-da51-4bc5-94ec-0ddbfa493243", "9b355654-aeed-422f-b8fa-eb41a5ac908d" });

            migrationBuilder.UpdateData(
                table: "Car",
                keyColumn: "NumberPlate",
                keyValue: "ABC123",
                columns: new[] { "FuelLevel", "Mileage" },
                values: new object[] { 100, 0 });

            migrationBuilder.UpdateData(
                table: "Car",
                keyColumn: "NumberPlate",
                keyValue: "DEF456",
                columns: new[] { "FuelLevel", "Mileage" },
                values: new object[] { 100, 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FuelLevel",
                table: "Car");

            migrationBuilder.DropColumn(
                name: "Mileage",
                table: "Car");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "57b0f2b5-28fa-4ef8-b66a-042e3da15114", "0ce0640a-9bd0-41b0-b1b2-4832c6b7af53" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "1b9395a2-f618-49a7-ab07-bebcc2fff7e4", "35c47e00-f17b-487f-b4a0-cdd8de1d09a1" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "1d66b3ef-78f2-4115-8c3d-5fefd589e78d", "4e2ecfd6-bf67-4b10-a178-88714f1d872b" });
        }
    }
}
