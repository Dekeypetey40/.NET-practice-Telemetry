using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Telemetry.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixAlarmInstrumentIdRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alarms_Instruments_InstrumentId",
                table: "Alarms");

            migrationBuilder.AddForeignKey(
                name: "FK_Alarms_Instruments_InstrumentId",
                table: "Alarms",
                column: "InstrumentId",
                principalTable: "Instruments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alarms_Instruments_InstrumentId",
                table: "Alarms");

            migrationBuilder.AddForeignKey(
                name: "FK_Alarms_Instruments_InstrumentId",
                table: "Alarms",
                column: "InstrumentId",
                principalTable: "Instruments",
                principalColumn: "Id");
        }
    }
}
