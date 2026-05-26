using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservationApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddFixedByToFault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FixedByUserId",
                table: "Fault",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "f725d6db-9735-4758-8da1-687ba4214147", "019c63f8-8113-4d7c-bd0f-c0e461b73596" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "3f0ebc7f-4577-4822-a605-a05a486b0e70", "c1c59236-a6bb-42c5-8c39-47a7309ff60f" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "91b2bf2b-2c9e-41e4-813b-7fd482ef9dc6", "fc451295-d0e7-43e1-8b47-962819b5003f" });

            migrationBuilder.UpdateData(
                table: "Fault",
                keyColumn: "Id",
                keyValue: 1,
                column: "FixedByUserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Fault",
                keyColumn: "Id",
                keyValue: 2,
                column: "FixedByUserId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Fault_FixedByUserId",
                table: "Fault",
                column: "FixedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Fault_AspNetUsers_FixedByUserId",
                table: "Fault",
                column: "FixedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fault_AspNetUsers_FixedByUserId",
                table: "Fault");

            migrationBuilder.DropIndex(
                name: "IX_Fault_FixedByUserId",
                table: "Fault");

            migrationBuilder.DropColumn(
                name: "FixedByUserId",
                table: "Fault");

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
        }
    }
}
