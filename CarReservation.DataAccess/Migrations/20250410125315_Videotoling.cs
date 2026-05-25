using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarReservation.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Videotoling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasVideotolling",
                table: "Car",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "7e47e28b-be72-4c9a-a8bc-882b97850070", "9982438d-9f21-4ed2-a87a-4d49a2e1c2d5" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "601f6f0b-0cca-4e13-ad99-ce022a74635d", "5d6dfe64-51bc-4341-b41a-80d65201397a" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "f6c1a1c4-2b5f-4234-b9e8-8557f66ac87c", "053706fe-8c11-4803-a5da-9767b818085b" });

            migrationBuilder.UpdateData(
                table: "Car",
                keyColumn: "NumberPlate",
                keyValue: "ABC123",
                column: "HasVideotolling",
                value: false);

            migrationBuilder.UpdateData(
                table: "Car",
                keyColumn: "NumberPlate",
                keyValue: "DEF456",
                column: "HasVideotolling",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasVideotolling",
                table: "Car");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "3fba1b0a-4205-44f6-a7a3-1a45555b990c", "22b0375f-e258-4359-bc4a-b1067d61c7ff" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "3e9f1501-0ae0-4d88-bf52-861970129cc0", "831cfc52-495e-41a4-90b8-fdb0375c78fd" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "3decdd1b-2a5b-4460-b125-99a7b5e1524c", "58f7a3a7-516b-47ff-a8e3-55c69df5be05" });
        }
    }
}
