using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Posonl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initial_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CountryPosCompany");

            migrationBuilder.AddColumn<long>(
                name: "CountryGroupId",
                table: "PosCompanies",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PosCompanyId",
                schema: "posonl",
                table: "Country",
                type: "bigint",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "Country",
                keyColumn: "Id",
                keyValue: 1L,
                column: "PosCompanyId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "Country",
                keyColumn: "Id",
                keyValue: 2L,
                column: "PosCompanyId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "Country",
                keyColumn: "Id",
                keyValue: 3L,
                column: "PosCompanyId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "Country",
                keyColumn: "Id",
                keyValue: 4L,
                column: "PosCompanyId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "Country",
                keyColumn: "Id",
                keyValue: 5L,
                column: "PosCompanyId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "Country",
                keyColumn: "Id",
                keyValue: 6L,
                column: "PosCompanyId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "Country",
                keyColumn: "Id",
                keyValue: 7L,
                column: "PosCompanyId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "Country",
                keyColumn: "Id",
                keyValue: 8L,
                column: "PosCompanyId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "Country",
                keyColumn: "Id",
                keyValue: 9L,
                column: "PosCompanyId",
                value: null);

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "Country",
                keyColumn: "Id",
                keyValue: 10L,
                column: "PosCompanyId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 3L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 4L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 5L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 6L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 7L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 8L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 9L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 10L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 11L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 12L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 13L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 14L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 15L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 16L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 17L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 18L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 19L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 20L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 21L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 22L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 23L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 24L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 25L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 26L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 27L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 28L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 29L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 30L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 31L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 32L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 33L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 34L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 35L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 36L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 37L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 38L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 39L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 40L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 41L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 42L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 43L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 44L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 45L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 46L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 47L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 48L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 49L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 50L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 51L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 52L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 53L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 54L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 55L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 56L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 57L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 58L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 59L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 60L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 61L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 62L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 63L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 64L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 65L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 66L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 67L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 68L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 69L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 70L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 71L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 72L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 73L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 74L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 75L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 76L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 77L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 78L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 79L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 80L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 81L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 82L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 83L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 84L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 85L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 86L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 87L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 88L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 89L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 90L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 91L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 92L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 93L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 94L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 95L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 96L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 97L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 98L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 99L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.UpdateData(
                table: "PosCompanies",
                keyColumn: "Id",
                keyValue: 100L,
                column: "CountryGroupId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_PosCompanies_CountryGroupId",
                table: "PosCompanies",
                column: "CountryGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Country_PosCompanyId",
                schema: "posonl",
                table: "Country",
                column: "PosCompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Country_PosCompanies_PosCompanyId",
                schema: "posonl",
                table: "Country",
                column: "PosCompanyId",
                principalTable: "PosCompanies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PosCompanies_CountryGroup_CountryGroupId",
                table: "PosCompanies",
                column: "CountryGroupId",
                principalSchema: "posonl",
                principalTable: "CountryGroup",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Country_PosCompanies_PosCompanyId",
                schema: "posonl",
                table: "Country");

            migrationBuilder.DropForeignKey(
                name: "FK_PosCompanies_CountryGroup_CountryGroupId",
                table: "PosCompanies");

            migrationBuilder.DropIndex(
                name: "IX_PosCompanies_CountryGroupId",
                table: "PosCompanies");

            migrationBuilder.DropIndex(
                name: "IX_Country_PosCompanyId",
                schema: "posonl",
                table: "Country");

            migrationBuilder.DropColumn(
                name: "CountryGroupId",
                table: "PosCompanies");

            migrationBuilder.DropColumn(
                name: "PosCompanyId",
                schema: "posonl",
                table: "Country");

            migrationBuilder.CreateTable(
                name: "CountryPosCompany",
                columns: table => new
                {
                    PosCompaniesId = table.Column<long>(type: "bigint", nullable: false),
                    SupportedCountriesId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountryPosCompany", x => new { x.PosCompaniesId, x.SupportedCountriesId });
                    table.ForeignKey(
                        name: "FK_CountryPosCompany_Country_SupportedCountriesId",
                        column: x => x.SupportedCountriesId,
                        principalSchema: "posonl",
                        principalTable: "Country",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CountryPosCompany_PosCompanies_PosCompaniesId",
                        column: x => x.PosCompaniesId,
                        principalTable: "PosCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CountryPosCompany_SupportedCountriesId",
                table: "CountryPosCompany",
                column: "SupportedCountriesId");
        }
    }
}
