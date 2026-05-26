using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservationApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AdditionalFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DrivableComment",
                table: "Fault",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDrivable",
                table: "Fault",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "Service",
                table: "Car",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "87c5457e-f6a7-41e4-8a11-0b8613d5717c", "2ce1a1e8-7610-43cd-ae04-24d5b63af84a" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "e9ab4b6a-ede8-4f3e-a97b-36367a47c317", "28a7a395-5a22-422b-ae42-a7f3684d5afc" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "52cfa1de-bb50-4226-8ec6-cb928d140eb4", "df311450-139e-43e7-8f27-1b01177930de" });

            migrationBuilder.UpdateData(
                table: "Car",
                keyColumn: "NumberPlate",
                keyValue: "ABC123",
                column: "Service",
                value: new DateOnly(1, 1, 1));

            migrationBuilder.UpdateData(
                table: "Car",
                keyColumn: "NumberPlate",
                keyValue: "DEF456",
                column: "Service",
                value: new DateOnly(1, 1, 1));

            migrationBuilder.UpdateData(
                table: "Fault",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DrivableComment", "IsDrivable" },
                values: new object[] { null, true });

            migrationBuilder.UpdateData(
                table: "Fault",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DrivableComment", "IsDrivable" },
                values: new object[] { null, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DrivableComment",
                table: "Fault");

            migrationBuilder.DropColumn(
                name: "IsDrivable",
                table: "Fault");

            migrationBuilder.DropColumn(
                name: "Service",
                table: "Car");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "63492dcf-2a58-41fe-ab47-f90801eaf57a", "86bf19c4-c841-4188-a0df-26d92cc35882" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "456768c0-43c3-4ce6-b500-166fddeca9a3", "ea98abe5-2c16-4d76-bb28-543eee9e50bf" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "2a3bca45-5469-4473-b5d7-12024e10383c", "02f7ac92-3e9b-4f4d-b7dc-252941eabf6b" });
        }
    }
}
