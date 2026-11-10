using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UptimeMonitor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "monitored_endpoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    CheckInterval = table.Column<TimeSpan>(type: "interval", nullable: false),
                    LastCheckedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_monitored_endpoints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "check_results",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EndpointId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Outcome = table.Column<string>(type: "text", nullable: false),
                    HttpStatusCode = table.Column<int>(type: "integer", nullable: true),
                    FailureDetail = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_check_results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_check_results_monitored_endpoints_EndpointId",
                        column: x => x.EndpointId,
                        principalTable: "monitored_endpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_check_results_EndpointId_CheckedAt",
                table: "check_results",
                columns: new[] { "EndpointId", "CheckedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_monitored_endpoints_IsEnabled",
                table: "monitored_endpoints",
                column: "IsEnabled");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "check_results");

            migrationBuilder.DropTable(
                name: "monitored_endpoints");
        }
    }
}
