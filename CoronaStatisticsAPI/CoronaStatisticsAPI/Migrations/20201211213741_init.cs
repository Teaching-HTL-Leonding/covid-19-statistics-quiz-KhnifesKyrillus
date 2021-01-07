using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CoronaStatisticsAPI.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "FederalStates",
                table => new
                {
                    Id = table.Column<int>("int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>("nvarchar(max)", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_FederalStates", x => x.Id); });

            migrationBuilder.CreateTable(
                "Districts",
                table => new
                {
                    Id = table.Column<int>("int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StateId = table.Column<int>("int", nullable: false),
                    Code = table.Column<int>("int", nullable: false),
                    Name = table.Column<string>("nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Id);
                    table.ForeignKey(
                        "FK_Districts_FederalStates_StateId",
                        x => x.StateId,
                        "FederalStates",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "CovidCases",
                table => new
                {
                    Id = table.Column<int>("int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>("datetime2", nullable: false),
                    DistrictId = table.Column<int>("int", nullable: false),
                    Population = table.Column<int>("int", nullable: false),
                    Cases = table.Column<int>("int", nullable: false),
                    Deaths = table.Column<int>("int", nullable: false),
                    SevenDaysIncidents = table.Column<int>("int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CovidCases", x => x.Id);
                    table.ForeignKey(
                        "FK_CovidCases_Districts_DistrictId",
                        x => x.DistrictId,
                        "Districts",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                "IX_CovidCases_DistrictId",
                "CovidCases",
                "DistrictId");

            migrationBuilder.CreateIndex(
                "IX_Districts_StateId",
                "Districts",
                "StateId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "CovidCases");

            migrationBuilder.DropTable(
                "Districts");

            migrationBuilder.DropTable(
                "FederalStates");
        }
    }
}