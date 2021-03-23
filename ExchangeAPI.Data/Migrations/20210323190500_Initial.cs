using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ExchangeAPI.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rates",
                columns: table => new
                {
                    Date = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    Base = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rates", x => new { x.Date, x.Base, x.Currency });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rates_Currency",
                table: "Rates",
                column: "Currency");

            migrationBuilder.CreateIndex(
                name: "IX_Rates_Date_Currency",
                table: "Rates",
                columns: new[] { "Date", "Currency" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Rates");
        }
    }
}
