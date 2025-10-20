using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CdcBridge.Host.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BufferedChangeEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TrackingInstanceName = table.Column<string>(type: "TEXT", nullable: false),
                    RowLabel = table.Column<string>(type: "TEXT", nullable: false),
                    Change = table.Column<string>(type: "TEXT", nullable: false),
                    BufferedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BufferedChangeEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrackingInstanceStates",
                columns: table => new
                {
                    TrackingInstanceName = table.Column<string>(type: "TEXT", nullable: false),
                    LastProcessedRowLabel = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackingInstanceStates", x => x.TrackingInstanceName);
                });

            migrationBuilder.CreateTable(
                name: "ReceiverDeliveryStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BufferedChangeEventId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReceiverName = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    AttemptCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LastAttemptAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ErrorDescription = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiverDeliveryStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceiverDeliveryStatuses_BufferedChangeEvents_BufferedChangeEventId",
                        column: x => x.BufferedChangeEventId,
                        principalTable: "BufferedChangeEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReceiverDeliveryStatuses_BufferedChangeEventId_ReceiverName",
                table: "ReceiverDeliveryStatuses",
                columns: new[] { "BufferedChangeEventId", "ReceiverName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReceiverDeliveryStatuses");

            migrationBuilder.DropTable(
                name: "TrackingInstanceStates");

            migrationBuilder.DropTable(
                name: "BufferedChangeEvents");
        }
    }
}
