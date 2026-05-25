using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarReservation.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FeedbackDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PickupFeedbackDate",
                table: "Reservation",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnFeedbackDate",
                table: "Reservation",
                type: "datetime2",
                nullable: true);

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

            migrationBuilder.UpdateData(
                table: "Reservation",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PickupFeedbackDate", "ReturnFeedbackDate" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Reservation",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "PickupFeedbackDate", "ReturnFeedbackDate" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Reservation",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "PickupFeedbackDate", "ReturnFeedbackDate" },
                values: new object[] { null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PickupFeedbackDate",
                table: "Reservation");

            migrationBuilder.DropColumn(
                name: "ReturnFeedbackDate",
                table: "Reservation");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "a0183101-8c13-4429-81a5-c03fc8de30d7", "2c90291f-a2a8-4633-9678-3a59968a8983" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "4f198bc9-3226-4136-bfc1-7228c355da27", "893e04f2-fcae-49d6-921b-86eaf5118cbc" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "90c19fc6-d977-48c6-a985-c7f2e2bde6c0", "29469d12-cf7d-46e1-ba6b-fea5c1a3d91c" });
        }
    }
}
