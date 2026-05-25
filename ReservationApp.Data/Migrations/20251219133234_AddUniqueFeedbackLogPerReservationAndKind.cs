using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReservationApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueFeedbackLogPerReservationAndKind : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FeedbackLogs_ReservationId",
                table: "FeedbackLogs");

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

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackLogs_ReservationId_Kind",
                table: "FeedbackLogs",
                columns: new[] { "ReservationId", "Kind" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FeedbackLogs_ReservationId_Kind",
                table: "FeedbackLogs");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "6b946012-54fa-4814-a65c-454480a2ee3a", "f5abe40d-cb4e-4681-907b-633d165aad31" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "ee50819c-ee7d-4a14-98fa-0faa381273e7", "ba4854c5-14e5-4b7d-a8e9-e2aa406e5bdd" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "70236d11-b75d-405a-94e1-b054838191fc", "fd636751-d184-4264-a5fd-5b4779125d80" });

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackLogs_ReservationId",
                table: "FeedbackLogs",
                column: "ReservationId");
        }
    }
}
