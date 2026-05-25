using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarReservation.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FaultsAndcleanliness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCarDirtyAtPickup",
                table: "Reservation",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCarDirtyAtReturn",
                table: "Reservation",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickupFaults",
                table: "Reservation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnFaults",
                table: "Reservation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDirty",
                table: "Car",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "e4672bcb-355d-43ec-b30d-65c2b84d8f2a", "de22588b-91f1-4b0b-8906-c8f2f99fbf82" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "6239d74b-3794-4f01-844b-8d4d7d779c45", "bd3615c8-1d85-486a-b6db-87f044278263" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "2257b5d6-4936-49fd-b759-bc02b2b72a31", "1c67ae0b-5872-45d2-bd41-4ee37738482a" });

            migrationBuilder.UpdateData(
                table: "Car",
                keyColumn: "NumberPlate",
                keyValue: "ABC123",
                column: "IsDirty",
                value: false);

            migrationBuilder.UpdateData(
                table: "Car",
                keyColumn: "NumberPlate",
                keyValue: "DEF456",
                column: "IsDirty",
                value: false);

            migrationBuilder.UpdateData(
                table: "Fault",
                keyColumn: "Id",
                keyValue: 1,
                column: "DateReported",
                value: new DateTime(2025, 3, 6, 12, 34, 12, 8, DateTimeKind.Local).AddTicks(6751));

            migrationBuilder.UpdateData(
                table: "Fault",
                keyColumn: "Id",
                keyValue: 2,
                column: "DateReported",
                value: new DateTime(2025, 3, 6, 12, 34, 12, 8, DateTimeKind.Local).AddTicks(6756));

            migrationBuilder.UpdateData(
                table: "Reservation",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "IsCarDirtyAtPickup", "IsCarDirtyAtReturn", "PickupFaults", "ReturnFaults" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Reservation",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "IsCarDirtyAtPickup", "IsCarDirtyAtReturn", "PickupFaults", "ReturnFaults" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Reservation",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "IsCarDirtyAtPickup", "IsCarDirtyAtReturn", "PickupFaults", "ReturnFaults" },
                values: new object[] { null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCarDirtyAtPickup",
                table: "Reservation");

            migrationBuilder.DropColumn(
                name: "IsCarDirtyAtReturn",
                table: "Reservation");

            migrationBuilder.DropColumn(
                name: "PickupFaults",
                table: "Reservation");

            migrationBuilder.DropColumn(
                name: "ReturnFaults",
                table: "Reservation");

            migrationBuilder.DropColumn(
                name: "IsDirty",
                table: "Car");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "3a95b05f-21f8-4f9f-aa17-7ad774fabad3", "a1a75bf5-75de-4087-9e78-d7cc3d2e2bbf" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "8b1a2c14-6ba6-4d0d-8f8e-fe60408290d7", "8d1e113a-90a9-4202-aa80-1b8d25f26a83" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "a904d83b-06e8-4811-96d6-204a038eea46", "0b33eac8-2e43-4bf4-afcb-75bfb23474fb" });

            migrationBuilder.UpdateData(
                table: "Fault",
                keyColumn: "Id",
                keyValue: 1,
                column: "DateReported",
                value: new DateTime(2025, 3, 5, 15, 5, 31, 150, DateTimeKind.Local).AddTicks(4843));

            migrationBuilder.UpdateData(
                table: "Fault",
                keyColumn: "Id",
                keyValue: 2,
                column: "DateReported",
                value: new DateTime(2025, 3, 5, 15, 5, 31, 150, DateTimeKind.Local).AddTicks(4849));
        }
    }
}
