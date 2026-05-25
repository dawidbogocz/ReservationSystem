using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservationApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddFixDateFault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FixDate",
                table: "Fault",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "7982fbe5-3615-48eb-9e73-390393d3f76a", "1f9a3122-02a7-4adc-8f53-4c49fff2a450" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "77eb4841-9dfb-453b-b666-a3d25f4f7035", "4fe6e493-14ab-481c-999f-fa1028d1ed4b" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "ad365ab1-d41c-4826-94d8-fb61308c0f97", "5501fa75-0975-4c63-8de5-9fe3231eca38" });

            migrationBuilder.UpdateData(
                table: "Fault",
                keyColumn: "Id",
                keyValue: 1,
                column: "FixDate",
                value: null);

            migrationBuilder.UpdateData(
                table: "Fault",
                keyColumn: "Id",
                keyValue: 2,
                column: "FixDate",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FixDate",
                table: "Fault");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "f9868363-68be-48d2-b11a-5aca53e23ee4", "0553f2c6-aa3b-42e1-bcdd-d51bd6347c99" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "01f7d0ec-61fc-43e2-b29c-ef0f1798b372", "7beebe74-bbc3-46a7-a39b-0f4b3cc98efb" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "4ed57059-7a96-4198-9023-b16567903434", "17bd812f-548e-4580-8074-3e0faa085f90" });
        }
    }
}
