using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservationApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class CancelationEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "3e40146e-9224-4b07-91ab-5f8501133cf7", "2979a37e-d32a-44b2-b348-927b923f4146" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "f1b89359-d306-4fff-898b-94e1cf3d78a2", "649cedbf-25fe-45b8-8e7c-80fef6926552" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "7db9ecc8-dd7b-4a98-9630-6b0bf0296c53", "ea355d5d-92d6-4969-bcd3-e374b0fea801" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "6f84a335-9f4c-4598-ab02-31eda2926ee7", "7d87aef0-09d0-4b4b-847d-734965453dde" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "8a3d9179-b63d-4834-8036-051810da5631", "3ff1e0b5-aa75-4995-8d2d-1b136dfdc2cd" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "604cd9ea-5478-4acc-ab1e-a21d3e7deac4", "8c196c7d-bd30-4995-9a3a-4eb00a9e22a8" });
        }
    }
}
