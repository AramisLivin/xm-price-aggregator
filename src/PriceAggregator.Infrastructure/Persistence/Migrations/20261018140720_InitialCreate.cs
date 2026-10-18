using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriceAggregator.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AggregatedPrices",
                columns: table => new
                {
                    Instrument = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Timestamp = table.Column<long>(type: "INTEGER", nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", nullable: false),
                    Sources = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AggregatedPrices", x => new { x.Instrument, x.Timestamp });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AggregatedPrices");
        }
    }
}
