using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CdcBridge.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Owner = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Permission = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeys", x => x.Id);
                });

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
                    ErrorDescription = table.Column<string>(type: "TEXT", nullable: true),
                    LastDeliveryTimeMs = table.Column<long>(type: "INTEGER", nullable: true),
                    AverageDeliveryTimeMs = table.Column<double>(type: "REAL", nullable: true)
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
                name: "IX_ApiKeys_Key",
                table: "ApiKeys",
                column: "Key",
                unique: true);

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
                name: "ApiKeys");

            migrationBuilder.DropTable(
                name: "ReceiverDeliveryStatuses");

            migrationBuilder.DropTable(
                name: "TrackingInstanceStates");

            migrationBuilder.DropTable(
                name: "BufferedChangeEvents");
        }
    }
}
