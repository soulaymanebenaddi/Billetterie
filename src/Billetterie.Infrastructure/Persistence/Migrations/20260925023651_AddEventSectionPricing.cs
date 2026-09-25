using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billetterie.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEventSectionPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventSectionPrices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSectionPrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventSectionPrices_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventSectionPrices_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventSectionPrices_EventId_SectionId",
                table: "EventSectionPrices",
                columns: new[] { "EventId", "SectionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventSectionPrices_SectionId",
                table: "EventSectionPrices",
                column: "SectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventSectionPrices");
        }
    }
}
