using System;
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
            migrationBuilder.DropColumn(
                name: "PublishedDate",
                schema: "posonl",
                table: "LocalizationEntries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedDate",
                schema: "posonl",
                table: "LocalizationEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 2L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 3L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 4L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 5L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 6L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 7L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 8L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 9L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 10L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 11L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 12L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 13L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 14L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 15L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 16L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 17L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 18L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 19L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 20L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 21L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 22L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 23L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 24L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 25L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 26L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 27L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 28L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 29L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 30L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 31L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 32L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 33L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 34L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 35L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 36L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 37L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 38L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 39L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 40L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 41L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 42L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 43L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 44L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 45L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 46L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 47L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 48L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 49L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 50L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 51L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 52L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 53L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 54L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 55L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 56L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 57L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 58L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 59L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 60L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 61L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 62L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 63L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 64L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 65L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 66L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 67L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 68L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 69L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 70L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 71L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 72L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 73L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 74L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 75L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 76L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 77L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 78L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 79L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 80L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 81L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 82L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 83L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 84L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 85L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 86L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 87L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 88L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 89L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 90L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 91L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 92L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 93L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 94L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 95L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 96L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 97L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 98L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 99L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 100L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1001L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1002L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1003L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1004L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1005L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1006L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1007L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1008L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1009L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1010L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1011L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1012L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1013L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1014L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1015L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1016L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1017L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1018L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1019L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1020L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1021L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1022L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1023L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1024L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1025L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1026L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1027L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1028L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1029L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1030L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1031L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1032L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1033L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1034L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1035L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1036L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1037L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1038L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1039L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1040L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1041L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1042L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1043L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1044L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1045L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1046L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1047L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1048L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1049L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1050L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1051L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1052L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1053L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1054L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 1055L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 15001L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 15002L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 15003L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 15004L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 15005L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 15006L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 15007L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 15008L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 15009L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "posonl",
                table: "LocalizationEntries",
                keyColumn: "Id",
                keyValue: 15010L,
                column: "PublishedDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
