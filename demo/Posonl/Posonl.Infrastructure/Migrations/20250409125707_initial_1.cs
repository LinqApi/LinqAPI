using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Posonl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initial_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "posonl");

            migrationBuilder.CreateTable(
                name: "CountryGroup",
                schema: "posonl",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountryGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cultures",
                schema: "posonl",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cultures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Country",
                schema: "posonl",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CountryGroupId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Country", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Country_CountryGroup_CountryGroupId",
                        column: x => x.CountryGroupId,
                        principalSchema: "posonl",
                        principalTable: "CountryGroup",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalizationEntries",
                schema: "posonl",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CultureId = table.Column<short>(type: "smallint", nullable: false),
                    LocalizationType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ViewType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetaDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetaKeywords = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PublishedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: true),
                    ContentHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Headquarters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmployeeCount = table.Column<int>(type: "int", nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FoundedYear = table.Column<int>(type: "int", nullable: true),
                    StockTicker = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PosCompanyId = table.Column<int>(type: "int", nullable: true),
                    PosCompanyDescription_FoundedYear = table.Column<int>(type: "int", nullable: true),
                    PosCompanyDescription_StockTicker = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeadquartersCountryId = table.Column<int>(type: "int", nullable: true),
                    PosCompanyDescription_PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PosCompanyDescription_Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PosServiceCategoryId = table.Column<long>(type: "bigint", nullable: true),
                    IsGlobal = table.Column<bool>(type: "bit", nullable: true),
                    IsRegional = table.Column<bool>(type: "bit", nullable: true),
                    CountryId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalizationEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalizationEntries_Country_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "posonl",
                        principalTable: "Country",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalizationEntries_Cultures_CultureId",
                        column: x => x.CultureId,
                        principalSchema: "posonl",
                        principalTable: "Cultures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LocalizationEntries_LocalizationEntries_PosServiceCategoryId",
                        column: x => x.PosServiceCategoryId,
                        principalSchema: "posonl",
                        principalTable: "LocalizationEntries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CountryGroupPosService",
                schema: "posonl",
                columns: table => new
                {
                    PosServicesId = table.Column<long>(type: "bigint", nullable: false),
                    SupportedCountryGroupsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountryGroupPosService", x => new { x.PosServicesId, x.SupportedCountryGroupsId });
                    table.ForeignKey(
                        name: "FK_CountryGroupPosService_CountryGroup_SupportedCountryGroupsId",
                        column: x => x.SupportedCountryGroupsId,
                        principalSchema: "posonl",
                        principalTable: "CountryGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CountryGroupPosService_LocalizationEntries_PosServicesId",
                        column: x => x.PosServicesId,
                        principalSchema: "posonl",
                        principalTable: "LocalizationEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CountryPosCompany",
                schema: "posonl",
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
                        name: "FK_CountryPosCompany_LocalizationEntries_PosCompaniesId",
                        column: x => x.PosCompaniesId,
                        principalSchema: "posonl",
                        principalTable: "LocalizationEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PosCommissionRate",
                schema: "posonl",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PosCompanyId = table.Column<long>(type: "bigint", nullable: false),
                    CountryId = table.Column<long>(type: "bigint", nullable: false),
                    CommissionPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosCommissionRate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PosCommissionRate_Country_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "posonl",
                        principalTable: "Country",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PosCommissionRate_LocalizationEntries_PosCompanyId",
                        column: x => x.PosCompanyId,
                        principalSchema: "posonl",
                        principalTable: "LocalizationEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PosCompanyPosService",
                schema: "posonl",
                columns: table => new
                {
                    PosCompaniesId = table.Column<long>(type: "bigint", nullable: false),
                    PosServicesId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosCompanyPosService", x => new { x.PosCompaniesId, x.PosServicesId });
                    table.ForeignKey(
                        name: "FK_PosCompanyPosService_LocalizationEntries_PosCompaniesId",
                        column: x => x.PosCompaniesId,
                        principalSchema: "posonl",
                        principalTable: "LocalizationEntries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PosCompanyPosService_LocalizationEntries_PosServicesId",
                        column: x => x.PosServicesId,
                        principalSchema: "posonl",
                        principalTable: "LocalizationEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PosCompanyRating",
                schema: "posonl",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PosCompanyId = table.Column<long>(type: "bigint", nullable: false),
                    RatingCategoryId = table.Column<long>(type: "bigint", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosCompanyRating", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PosCompanyRating_LocalizationEntries_PosCompanyId",
                        column: x => x.PosCompanyId,
                        principalSchema: "posonl",
                        principalTable: "LocalizationEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PosCompanyRating_LocalizationEntries_RatingCategoryId",
                        column: x => x.RatingCategoryId,
                        principalSchema: "posonl",
                        principalTable: "LocalizationEntries",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                schema: "posonl",
                table: "Country",
                columns: new[] { "Id", "Code", "CountryGroupId", "Currency", "LanguageCode", "Name" },
                values: new object[,]
                {
                    { 1L, "tr", null, "TRY", "tr-TR", "Türkiye" },
                    { 2L, "us", null, "USD", "en-US", "United States" },
                    { 3L, "de", null, "EUR", "de-DE", "Germany" },
                    { 4L, "nl", null, "EUR", "nl-NL", "Netherlands" },
                    { 5L, "cn", null, "CNY", "zh-CN", "China" },
                    { 6L, "jp", null, "JPY", "ja-JP", "Japan" },
                    { 7L, "in", null, "INR", "hi-IN", "India" },
                    { 8L, "uk", null, "GBP", "en-GB", "United Kingdom" },
                    { 9L, "ru", null, "RUB", "ru-RU", "Russia" },
                    { 10L, "br", null, "BRL", "pt-BR", "Brazil" }
                });

            migrationBuilder.InsertData(
                schema: "posonl",
                table: "CountryGroup",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1L, "Avrupa" },
                    { 2L, "Asya" },
                    { 3L, "Afrika" },
                    { 4L, "Kuzey Amerika" },
                    { 5L, "Güney Amerika" },
                    { 6L, "Okyanusya" },
                    { 7L, "Orta Doğu" },
                    { 8L, "Türk Devletleri" }
                });

            migrationBuilder.InsertData(
                schema: "posonl",
                table: "Cultures",
                columns: new[] { "Id", "Code", "DisplayName" },
                values: new object[,]
                {
                    { (short)1, "tr-TR", "Türkçe (Türkiye)" },
                    { (short)2, "en-US", "English (United States)" },
                    { (short)3, "de-DE", "Deutsch (Deutschland)" }
                });

            migrationBuilder.InsertData(
                schema: "posonl",
                table: "LocalizationEntries",
                columns: new[] { "Id", "Address", "CultureId", "Description", "Email", "EmployeeCount", "FoundedYear", "Headquarters", "IsPublished", "LocalizationType", "LogoUrl", "MetaDescription", "MetaKeywords", "Name", "PhoneNumber", "PublishedDate", "Slug", "StockTicker", "Title", "ViewType", "Website" },
                values: new object[,]
                {
                    { 1L, "Singapore, Singapore", (short)1, "2C2P is a Singapore-based payment platform providing payment processing solutions across Asia.", "contact@2c2p.com", 300, 2003, "Singapore, Singapore", true, null, "", "2C2P is a Singapore-based payment platform providing payment processing solutions across Asia.", null, "2C2P", "+65 3123 4567", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "2C2P", null, "2C2P", "poscompany", "https://www.2c2p.com" },
                    { 2L, "Amsterdam, Netherlands", (short)1, "Adyen is a Dutch payments company that offers a global omnichannel payment platform for businesses.", "info@adyen.com", 4000, 2006, "Amsterdam, Netherlands", true, null, "", "Adyen is a Dutch payments company that offers a global omnichannel payment platform for businesses.", null, "Adyen", "+31 20 240 1240", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Adyen", null, "Adyen", "poscompany", "https://www.adyen.com" },
                    { 3L, "San Francisco, CA, USA", (short)1, "Affirm is an American fintech company that provides buy now, pay later financing for online consumers.", "support@affirm.com", 2000, 2012, "San Francisco, USA", true, null, "", "Affirm is an American fintech company that provides buy now, pay later financing for online consumers.", null, "Affirm", "+1 415-123-4567", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Affirm", null, "Affirm", "poscompany", "https://www.affirm.com" },
                    { 4L, "Melbourne, Australia", (short)1, "Afterpay is an Australian fintech known for its buy now, pay later payment service, allowing shoppers to pay in installments.", "info@afterpay.com", 1000, 2014, "Melbourne, Australia", true, null, "", "Afterpay is an Australian fintech known for its buy now, pay later payment service, allowing shoppers to pay in installments.", null, "Afterpay", "+61 3 9100 0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Afterpay", null, "Afterpay", "poscompany", "https://www.afterpay.com" },
                    { 5L, "Hangzhou, China", (short)1, "Alipay is a Chinese online and mobile payment platform launched by Ant Group, widely used for digital payments in China.", "support@alipay.com", 10000, 2004, "Hangzhou, China", true, null, "", "Alipay is a Chinese online and mobile payment platform launched by Ant Group, widely used for digital payments in China.", null, "Alipay", "+86 571 2688 8888", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Alipay", null, "Alipay", "poscompany", "https://www.alipay.com" },
                    { 6L, "New York, NY, USA", (short)1, "American Express (Amex) is an American multinational financial services corporation best known for its credit card, charge card, and traveler's cheque businesses.", "info@americanexpress.com", 64000, 1850, "New York, USA", true, null, "", "American Express (Amex) is an American multinational financial services corporation best known for its credit card, charge card, and traveler's cheque businesses.", null, "American Express", "+1 212-640-2000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "American Express", null, "American Express", "poscompany", "https://www.americanexpress.com" },
                    { 7L, "Foster City, CA, USA", (short)1, "Authorize.Net is an American payment gateway service that enables merchants to accept credit card and electronic payments online.", "support@authorize.net", 500, 1996, "Foster City, USA", true, null, "", "Authorize.Net is an American payment gateway service that enables merchants to accept credit card and electronic payments online.", null, "Authorize.Net", "+1 888-323-4289", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Authorize.Net", null, "Authorize.Net", "poscompany", "https://www.authorize.net" },
                    { 8L, "Mumbai, India", (short)1, "BillDesk is an Indian online payment gateway platform used for bill payments and other online transactions.", "support@billdesk.com", 800, 2000, "Mumbai, India", true, null, "", "BillDesk is an Indian online payment gateway platform used for bill payments and other online transactions.", null, "BillDesk", "+91 22 7131 0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "BillDesk", null, "BillDesk", "poscompany", "https://www.billdesk.com" },
                    { 9L, "Atlanta, GA, USA", (short)1, "BitPay is an American cryptocurrency payment processor that allows merchants to accept Bitcoin and other crypto payments.", "support@bitpay.com", 100, 2011, "Atlanta, USA", true, null, "", "BitPay is an American cryptocurrency payment processor that allows merchants to accept Bitcoin and other crypto payments.", null, "BitPay", "+1 404-890-7700", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "BitPay", null, "BitPay", "poscompany", "https://www.bitpay.com" },
                    { 10L, "Waltham, MA, USA", (short)1, "BlueSnap is a US-based payment gateway that provides an all-in-one payment platform for businesses to accept payments globally.", "info@bluesnap.com", 300, 2001, "Waltham, USA", true, null, "", "BlueSnap is a US-based payment gateway that provides an all-in-one payment platform for businesses to accept payments globally.", null, "BlueSnap", "+1 866-312-7733", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "BlueSnap", null, "BlueSnap", "poscompany", "https://www.bluesnap.com" },
                    { 11L, "Chicago, IL, USA", (short)1, "Braintree is an American payments company that specializes in mobile and web payment systems for e-commerce (now a PayPal subsidiary).", "support@braintreepayments.com", 500, 2007, "Chicago, USA", true, null, "", "Braintree is an American payments company that specializes in mobile and web payment systems for e-commerce (now a PayPal subsidiary).", null, "Braintree", "+1 877-434-2894", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Braintree", null, "Braintree", "poscompany", "https://www.braintreepayments.com" },
                    { 12L, "Mumbai, India", (short)1, "CCAvenue is an Indian payment gateway platform that offers online payment processing for e-commerce businesses in India.", "support@ccavenue.com", 300, 2001, "Mumbai, India", true, null, "", "CCAvenue is an Indian payment gateway platform that offers online payment processing for e-commerce businesses in India.", null, "CCAvenue", "+91 22 6742 5555", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "CCAvenue", null, "CCAvenue", "poscompany", "https://www.ccavenue.com" },
                    { 13L, "Dallas, TX, USA", (short)1, "Chase Paymentech is an American payment processing and merchant acquiring service, part of JPMorgan Chase, serving businesses with payment solutions.", "support@chasepaymentech.com", 1000, 1985, "Dallas, USA", true, null, "", "Chase Paymentech is an American payment processing and merchant acquiring service, part of JPMorgan Chase, serving businesses with payment solutions.", null, "Chase Paymentech", "+1 800-934-7717", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Chase Paymentech", null, "Chase Paymentech", "poscompany", "https://www.chasepaymentech.com" },
                    { 14L, "London, UK", (short)1, "Checkout.com is a UK-based payment processor that provides online payment solutions for businesses worldwide.", "info@checkout.com", 1700, 2012, "London, UK", true, null, "", "Checkout.com is a UK-based payment processor that provides online payment solutions for businesses worldwide.", null, "Checkout.com", "+44 20 8068 0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Checkout.com", null, "Checkout.com", "poscompany", "https://www.checkout.com" },
                    { 15L, "San Francisco, CA, USA", (short)1, "Chipper Cash is a fintech company that offers a cross-border peer-to-peer payment service across Africa via a mobile-based platform.", "support@chippercash.com", 300, 2018, "San Francisco, USA", true, null, "", "Chipper Cash is a fintech company that offers a cross-border peer-to-peer payment service across Africa via a mobile-based platform.", null, "Chipper Cash", "+1 415-000-0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Chipper Cash", null, "Chipper Cash", "poscompany", "https://www.chippercash.com" },
                    { 16L, "Barueri, São Paulo, Brazil", (short)1, "Cielo is a Brazilian payment processing company and the largest credit and debit card payment operator in Latin America.", "atendimento@cielo.com.br", 5000, 1995, "Barueri, Brazil", true, null, "", "Cielo is a Brazilian payment processing company and the largest credit and debit card payment operator in Latin America.", null, "Cielo", "+55 11 3165-2000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Cielo", null, "Cielo", "poscompany", "https://www.cielo.com.br" },
                    { 17L, "Sunnyvale, CA, USA", (short)1, "Clover is an American point-of-sale platform offering smart payment terminals and POS software for small and medium businesses (a Fiserv company).", "sales@clover.com", 500, 2010, "Sunnyvale, USA", true, null, "", "Clover is an American point-of-sale platform offering smart payment terminals and POS software for small and medium businesses (a Fiserv company).", null, "Clover", "+1 408-123-4567", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Clover", null, "Clover", "poscompany", "https://www.clover.com" },
                    { 18L, "Foster City, CA, USA", (short)1, "CyberSource is an American payment management platform that provides credit card processing, fraud management, and payment security services (owned by Visa).", "support@cybersource.com", 1000, 1994, "Foster City, USA", true, null, "", "CyberSource is an American payment management platform that provides credit card processing, fraud management, and payment security services (owned by Visa).", null, "CyberSource", "+1 888-330-2300", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "CyberSource", null, "CyberSource", "poscompany", "https://www.cybersource.com" },
                    { 19L, "Montevideo, Uruguay", (short)1, "DLocal is a Uruguayan fintech company that specializes in cross-border payment processing for emerging markets.", "info@dlocal.com", 500, 2016, "Montevideo, Uruguay", true, null, "", "DLocal is a Uruguayan fintech company that specializes in cross-border payment processing for emerging markets.", null, "DLocal", "+598 000 0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "DLocal", null, "DLocal", "poscompany", "https://www.dlocal.com" },
                    { 20L, "North Canton, OH, USA", (short)1, "Diebold Nixdorf is an American multinational that provides ATMs, point-of-sale terminals, and integrated services for banking and retail.", "info@dieboldnixdorf.com", 22000, 1859, "North Canton, USA", true, null, "", "Diebold Nixdorf is an American multinational that provides ATMs, point-of-sale terminals, and integrated services for banking and retail.", null, "Diebold Nixdorf", "+1 330-490-4000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Diebold Nixdorf", null, "Diebold Nixdorf", "poscompany", "https://www.dieboldnixdorf.com" },
                    { 21L, "Riverwoods, IL, USA", (short)1, "Discover is an American financial services company that operates Discover Card and network, providing credit card services and payment network solutions.", "support@discover.com", 17000, 1985, "Riverwoods, USA", true, null, "", "Discover is an American financial services company that operates Discover Card and network, providing credit card services and payment network solutions.", null, "Discover", "+1 800-347-2683", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Discover", null, "Discover", "poscompany", "https://www.discover.com" },
                    { 22L, "Des Moines, IA, USA", (short)1, "Dwolla is an American fintech company that offers an API platform for bank transfers and ACH payments in the United States.", "support@dwolla.com", 200, 2008, "Des Moines, USA", true, null, "", "Dwolla is an American fintech company that offers an API platform for bank transfers and ACH payments in the United States.", null, "Dwolla", "+1 515-000-0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dwolla", null, "Dwolla", "poscompany", "https://www.dwolla.com" },
                    { 23L, "Curitiba, Brazil", (short)1, "EBANX is a Brazilian fintech company providing local payment solutions in Latin America, enabling global merchants to accept regional payment methods.", "business@ebanx.com", 1000, 2012, "Curitiba, Brazil", true, null, "", "EBANX is a Brazilian fintech company providing local payment solutions in Latin America, enabling global merchants to accept regional payment methods.", null, "EBANX", "+55 41 0000-0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "EBANX", null, "EBANX", "poscompany", "https://www.ebanx.com" },
                    { 24L, "Atlanta, GA, USA", (short)1, "Elavon is an American payment processing company (a subsidiary of U.S. Bancorp) that provides merchant acquiring services globally.", "customer@elavon.com", 4000, 1991, "Atlanta, USA", true, null, "", "Elavon is an American payment processing company (a subsidiary of U.S. Bancorp) that provides merchant acquiring services globally.", null, "Elavon", "+1 800-725-1243", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Elavon", null, "Elavon", "poscompany", "https://www.elavon.com" },
                    { 25L, "Atlanta, GA, USA", (short)1, "EVO Payments is an American payment services provider offering merchant acquiring and payment processing solutions to merchants across the world.", "info@evopayments.com", 2400, 1989, "Atlanta, USA", true, null, "", "EVO Payments is an American payment services provider offering merchant acquiring and payment processing solutions to merchants across the world.", null, "EVO Payments", "+1 770-709-3000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "EVO Payments", null, "EVO Payments", "poscompany", "https://www.evopayments.com" },
                    { 26L, "Sydney, Australia", (short)1, "Eway is an Australian online payment gateway that processes secure credit card payments for merchants, offering omnichannel payment solutions.", "support@eway.com.au", 100, 1998, "Sydney, Australia", true, null, "", "Eway is an Australian online payment gateway that processes secure credit card payments for merchants, offering omnichannel payment solutions.", null, "Eway", "+61 2 0000 0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Eway", null, "Eway", "poscompany", "https://www.eway.com.au" },
                    { 27L, "Jacksonville, FL, USA", (short)1, "FIS (Fidelity National Information Services) is an American provider of financial technology solutions for merchants, banks, and capital markets, including payment processing services.", "info@fisglobal.com", 50000, 1968, "Jacksonville, USA", true, null, "", "FIS (Fidelity National Information Services) is an American provider of financial technology solutions for merchants, banks, and capital markets, including payment processing services.", null, "FIS", "+1 904-438-6000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "FIS", null, "FIS", "poscompany", "https://www.fisglobal.com" },
                    { 28L, "Cairo, Egypt", (short)1, "Fawry is an Egyptian electronic payments network offering a variety of financial services, including bill payments and mobile wallet services.", "support@fawry.com", 1000, 2008, "Cairo, Egypt", true, null, "", "Fawry is an Egyptian electronic payments network offering a variety of financial services, including bill payments and mobile wallet services.", null, "Fawry", "+20 2 0000 0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Fawry", null, "Fawry", "poscompany", "https://www.fawry.com" },
                    { 29L, "Brookfield, WI, USA", (short)1, "Fiserv is an American global fintech and payments company that provides payment processing, financial technology, and merchant services (merged with First Data).", "info@fiserv.com", 44000, 1984, "Brookfield, USA", true, null, "", "Fiserv is an American global fintech and payments company that provides payment processing, financial technology, and merchant services (merged with First Data).", null, "Fiserv", "+1 262-879-5000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Fiserv", null, "Fiserv", "poscompany", "https://www.fiserv.com" },
                    { 30L, "San Francisco, CA, USA", (short)1, "Flutterwave is a payments technology company founded in Nigeria (with HQ in the US) that enables businesses in Africa to accept and process payments across the globe.", "support@flutterwave.com", 400, 2016, "San Francisco, USA", true, null, "", "Flutterwave is a payments technology company founded in Nigeria (with HQ in the US) that enables businesses in Africa to accept and process payments across the globe.", null, "Flutterwave", "+1 415-000-0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Flutterwave", null, "Flutterwave", "poscompany", "https://www.flutterwave.com" },
                    { 31L, "Taguig, Philippines", (short)1, "GCash is a Philippine mobile wallet and digital payment service that allows users to pay bills, transfer money, and make purchases via their mobile phones.", "support@gcash.com", 800, 2004, "Taguig, Philippines", true, null, "", "GCash is a Philippine mobile wallet and digital payment service that allows users to pay bills, transfer money, and make purchases via their mobile phones.", null, "GCash", "+63 2 0000 0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "GCash", null, "GCash", "poscompany", "https://www.gcash.com" },
                    { 32L, "Atlanta, GA, USA", (short)1, "Global Payments Inc. is an American payment technology company offering payment processing and software solutions for merchants, issuers, and consumers worldwide.", "info@globalpay.com", 27000, 2000, "Atlanta, USA", true, null, "", "Global Payments Inc. is an American payment technology company offering payment processing and software solutions for merchants, issuers, and consumers worldwide.", null, "Global Payments", "+1 770-829-8000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Global Payments", null, "Global Payments", "poscompany", "https://www.globalpayments.com" },
                    { 33L, "London, UK", (short)1, "GoCardless is a British fintech company that specializes in recurring payments and direct debit solutions for businesses.", "help@gocardless.com", 500, 2011, "London, UK", true, null, "", "GoCardless is a British fintech company that specializes in recurring payments and direct debit solutions for businesses.", null, "GoCardless", "+44 20 7183 8674", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "GoCardless", null, "GoCardless", "poscompany", "https://www.gocardless.com" },
                    { 34L, "Princeton, NJ, USA", (short)1, "Heartland Payment Systems is an American payment processing company that provides merchant services and payment technology solutions (now part of Global Payments).", "support@heartland.us", 4000, 1997, "Princeton, USA", true, null, "", "Heartland Payment Systems is an American payment processing company that provides merchant services and payment technology solutions (now part of Global Payments).", null, "Heartland Payment Systems", "+1 888-798-3131", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Heartland Payment Systems", null, "Heartland Payment Systems", "poscompany", "https://www.heartland.us" },
                    { 35L, "Paris, France", (short)1, "Ingenico is a French payment technology company known for its point-of-sale payment terminals and secure electronic transaction solutions.", "sales@ingenico.com", 8000, 1980, "Paris, France", true, null, "", "Ingenico is a French payment technology company known for its point-of-sale payment terminals and secure electronic transaction solutions.", null, "Ingenico", "+33 1 00000000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ingenico", null, "Ingenico", "poscompany", "https://www.ingenico.com" },
                    { 36L, "Istanbul, Turkey", (short)1, "Ininal is a Turkish fintech company offering a prepaid card and digital wallet platform, enabling users to make cashless payments.", "info@ininal.com", 150, 2012, "Istanbul, Turkey", true, null, "", "Ininal is a Turkish fintech company offering a prepaid card and digital wallet platform, enabling users to make cashless payments.", null, "Ininal", "+90 212 000 0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ininal", null, "Ininal", "poscompany", "https://www.ininal.com" },
                    { 37L, "Lagos, Nigeria", (short)1, "Interswitch is a Nigerian digital payment processing company providing integrated payment and transaction switching services across Africa.", "info@interswitchng.com", 1000, 2002, "Lagos, Nigeria", true, null, "", "Interswitch is a Nigerian digital payment processing company providing integrated payment and transaction switching services across Africa.", null, "Interswitch", "+234 1 000 0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Interswitch", null, "Interswitch", "poscompany", "https://www.interswitchgroup.com" },
                    { 38L, "Istanbul, Turkey", (short)1, "iyzico is a Turkish fintech company that provides a secure payment gateway and digital wallet solutions for online businesses and marketplaces.", "destek@iyzico.com", 350, 2013, "Istanbul, Turkey", true, null, "", "iyzico is a Turkish fintech company that provides a secure payment gateway and digital wallet solutions for online businesses and marketplaces.", null, "iyzico", "+90 212 909 6972", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "iyzico", null, "iyzico", "poscompany", "https://www.iyzico.com" },
                    { 39L, "Stockholm, Sweden", (short)1, "iZettle is a Swedish fintech company that provides mobile point-of-sale solutions and card readers for small businesses (acquired by PayPal, now operating as Zettle).", "support@izettle.com", 600, 2010, "Stockholm, Sweden", true, null, "", "iZettle is a Swedish fintech company that provides mobile point-of-sale solutions and card readers for small businesses (acquired by PayPal, now operating as Zettle).", null, "iZettle", "+46 8 0000 0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "iZettle", null, "iZettle", "poscompany", "https://www.izettle.com" },
                    { 40L, "Tokyo, Japan", (short)1, "JCB (Japan Credit Bureau) is a Japanese credit card company and payment network offering credit card services accepted worldwide.", "info@jcb.co.jp", 4400, 1961, "Tokyo, Japan", true, null, "", "JCB (Japan Credit Bureau) is a Japanese credit card company and payment network offering credit card services accepted worldwide.", null, "JCB", "+81 3-5778-5400", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "JCB", null, "JCB", "poscompany", "https://www.global.jcb/en" },
                    { 41L, "Stockholm, Sweden", (short)1, "Klarna is a Swedish fintech company offering buy now, pay later payment solutions and an online payments platform for shoppers and merchants.", "customer@klarna.com", 5000, 2005, "Stockholm, Sweden", true, null, "", "Klarna is a Swedish fintech company offering buy now, pay later payment solutions and an online payments platform for shoppers and merchants.", null, "Klarna", "+46 8 120 120 00", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Klarna", null, "Klarna", "poscompany", "https://www.klarna.com" },
                    { 42L, "Montreal, Canada", (short)1, "Lightspeed is a Canadian point-of-sale and e-commerce software provider that offers cloud-based POS solutions for retail and hospitality businesses.", "info@lightspeedhq.com", 3000, 2005, "Montreal, Canada", true, null, "", "Lightspeed is a Canadian point-of-sale and e-commerce software provider that offers cloud-based POS solutions for retail and hospitality businesses.", null, "Lightspeed", "+1 514-907-1801", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Lightspeed", null, "Lightspeed", "poscompany", "https://www.lightspeedhq.com" },
                    { 43L, "Nairobi, Kenya", (short)1, "M-Pesa is a Kenyan mobile money service operated by Safaricom and Vodacom that allows users to store and transfer money and pay for goods via their mobile phones.", "info@safaricom.co.ke", 500, 2007, "Nairobi, Kenya", true, null, "", "M-Pesa is a Kenyan mobile money service operated by Safaricom and Vodacom that allows users to store and transfer money and pay for goods via their mobile phones.", null, "M-Pesa", "+254 722 000000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "M-Pesa", null, "M-Pesa", "poscompany", "https://www.safaricom.co.ke/personal/m-pesa" },
                    { 44L, "Oakland, CA, USA", (short)1, "Marqeta is an American fintech company that provides a modern card issuing platform for businesses to create and manage payment cards via APIs.", "sales@marqeta.com", 800, 2010, "Oakland, USA", true, null, "", "Marqeta is an American fintech company that provides a modern card issuing platform for businesses to create and manage payment cards via APIs.", null, "Marqeta", "+1 510-281-3700", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Marqeta", null, "Marqeta", "poscompany", "https://www.marqeta.com" },
                    { 45L, "Purchase, NY, USA", (short)1, "Mastercard is an American multinational financial services corporation that operates a global payment network and offers credit, debit, and prepaid cards.", "info@mastercard.com", 24000, 1966, "Purchase, USA", true, null, "", "Mastercard is an American multinational financial services corporation that operates a global payment network and offers credit, debit, and prepaid cards.", null, "Mastercard", "+1 914-249-2000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mastercard", null, "Mastercard", "poscompany", "https://www.mastercard.com" },
                    { 46L, "Buenos Aires, Argentina", (short)1, "MercadoPago is an Argentine fintech platform (part of MercadoLibre) that provides online payment services and digital wallet solutions across Latin America.", "ayuda@mercadopago.com", 2000, 2004, "Buenos Aires, Argentina", true, null, "", "MercadoPago is an Argentine fintech platform (part of MercadoLibre) that provides online payment services and digital wallet solutions across Latin America.", null, "MercadoPago", "+54 11 0000 0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "MercadoPago", null, "MercadoPago", "poscompany", "https://www.mercadopago.com" },
                    { 47L, "Amsterdam, Netherlands", (short)1, "Mollie is a Dutch payment gateway offering simple online payment solutions and integrations for businesses in Europe.", "info@mollie.com", 700, 2004, "Amsterdam, Netherlands", true, null, "", "Mollie is a Dutch payment gateway offering simple online payment solutions and integrations for businesses in Europe.", null, "Mollie", "+31 20 820 2080", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mollie", null, "Mollie", "poscompany", "https://www.mollie.com" },
                    { 48L, "Toronto, Canada", (short)1, "Moneris is a Canadian payment processing company (a joint venture of RBC and BMO) that provides merchant services and payment processing solutions in North America.", "support@moneris.com", 1500, 2000, "Toronto, Canada", true, null, "", "Moneris is a Canadian payment processing company (a joint venture of RBC and BMO) that provides merchant services and payment processing solutions in North America.", null, "Moneris", "+1 855-423-8472", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Moneris", null, "Moneris", "poscompany", "https://www.moneris.com" },
                    { 49L, "Dallas, TX, USA", (short)1, "MoneyGram is an American money transfer company that offers global person-to-person money transfer and bill payment services.", "customerservice@moneygram.com", 2500, 1940, "Dallas, USA", true, null, "", "MoneyGram is an American money transfer company that offers global person-to-person money transfer and bill payment services.", null, "MoneyGram", "+1 800-926-9400", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "MoneyGram", null, "MoneyGram", "poscompany", "https://www.moneygram.com" },
                    { 50L, "Atlanta, GA, USA", (short)1, "NCR Corporation is an American enterprise technology company that produces ATMs, point-of-sale (POS) systems, and other payment and retail hardware and software.", "contact@ncr.com", 36000, 1884, "Atlanta, USA", true, null, "", "NCR Corporation is an American enterprise technology company that produces ATMs, point-of-sale (POS) systems, and other payment and retail hardware and software.", null, "NCR Corporation", "+1 937-445-1936", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "NCR Corporation", null, "NCR Corporation", "poscompany", "https://www.ncr.com" },
                    { 51L, "London, UK", (short)1, "Neteller is a UK-based digital wallet service that allows users to transfer money and make payments online (part of Paysafe Group).", "support@neteller.com", 200, 1999, "London, UK", true, null, "", "Neteller is a UK-based digital wallet service that allows users to transfer money and make payments online (part of Paysafe Group).", null, "Neteller", "+44 20 3308 9525", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Neteller", null, "Neteller", "poscompany", "https://www.neteller.com" },
                    { 52L, "Ballerup, Denmark", (short)1, "Nets is a Danish payment services provider that offers digital payment solutions, including card processing and digital ID services, across Northern Europe.", "info@nets.eu", 2600, 1968, "Ballerup, Denmark", true, null, "", "Nets is a Danish payment services provider that offers digital payment solutions, including card processing and digital ID services, across Northern Europe.", null, "Nets", "+45 44 68 44 68", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Nets", null, "Nets", "poscompany", "https://www.nets.eu" },
                    { 53L, "Dubai, UAE", (short)1, "Network International is a UAE-based payment solutions provider offering merchant acquiring, card processing, and digital payment solutions across the Middle East and Africa.", "marketing@network.ae", 1500, 1994, "Dubai, UAE", true, null, "", "Network International is a UAE-based payment solutions provider offering merchant acquiring, card processing, and digital payment solutions across the Middle East and Africa.", null, "Network International", "+971 4 303 2432", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Network International", null, "Network International", "poscompany", "https://www.network.ae" },
                    { 54L, "Milan, Italy", (short)1, "Nexi is an Italian digital payments company providing payment technology solutions, including card issuing, merchant services, and digital banking, across Europe.", "info@nexi.it", 10000, 2017, "Milan, Italy", true, null, "", "Nexi is an Italian digital payments company providing payment technology solutions, including card issuing, merchant services, and digital banking, across Europe.", null, "Nexi", "+39 02 3488 0892", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Nexi", null, "Nexi", "poscompany", "https://www.nexi.it" },
                    { 55L, "Montreal, Canada", (short)1, "Nuvei is a Canadian fintech company that provides payment processing technology and services to merchants, including support for card payments and alternative payment methods.", "support@nuvei.com", 1500, 2003, "Montreal, Canada", true, null, "", "Nuvei is a Canadian fintech company that provides payment processing technology and services to merchants, including support for card payments and alternative payment methods.", null, "Nuvei", "+1 514-390-2030", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Nuvei", null, "Nuvei", "poscompany", "https://www.nuvei.com" },
                    { 56L, "Jakarta, Indonesia", (short)1, "OVO is an Indonesian digital wallet and payments service that enables users to make cashless payments, transfer funds, and earn loyalty rewards via a mobile app.", "cs@ovo.id", 500, 2017, "Jakarta, Indonesia", true, null, "", "OVO is an Indonesian digital wallet and payments service that enables users to make cashless payments, transfer funds, and earn loyalty rewards via a mobile app.", null, "OVO", "+62 21 0000 0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "OVO", null, "OVO", "poscompany", "https://www.ovo.id" },
                    { 57L, "Bangkok, Thailand", (short)1, "Omise is a Thailand-based payment gateway providing online payment processing solutions in Southeast Asia, with a focus on ease of integration for merchants.", "support@omise.co", 300, 2013, "Bangkok, Thailand", true, null, "", "Omise is a Thailand-based payment gateway providing online payment processing solutions in Southeast Asia, with a focus on ease of integration for merchants.", null, "Omise", "+66 2 000 0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Omise", null, "Omise", "poscompany", "https://www.omise.co" },
                    { 58L, "Shenzhen, China", (short)1, "PAX Technology is a Chinese manufacturer of payment terminals and POS hardware, supplying secure electronic payment devices to financial institutions worldwide.", "sales@pax.com.cn", 1800, 2000, "Shenzhen, China", true, null, "", "PAX Technology is a Chinese manufacturer of payment terminals and POS hardware, supplying secure electronic payment devices to financial institutions worldwide.", null, "PAX Technology", "+86 755 86169630", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "PAX Technology", null, "PAX Technology", "poscompany", "https://www.paxtechnology.com" },
                    { 59L, "São Paulo, Brazil", (short)1, "PagSeguro is a Brazilian fintech company offering online payment services, mobile payments, and fintech solutions, including its popular PagBank digital account.", "atendimento@pagseguro.com.br", 8000, 2006, "São Paulo, Brazil", true, null, "", "PagSeguro is a Brazilian fintech company offering online payment services, mobile payments, and fintech solutions, including its popular PagBank digital account.", null, "PagSeguro", "+55 11 3028-9000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "PagSeguro", null, "PagSeguro", "poscompany", "https://pagseguro.uol.com.br" },
                    { 60L, "Istanbul, Turkey", (short)1, "Papara is a Turkish fintech company that provides a digital wallet and prepaid card service, enabling users to send money and make online payments.", "support@papara.com", 300, 2016, "Istanbul, Turkey", true, null, "", "Papara is a Turkish fintech company that provides a digital wallet and prepaid card service, enabling users to send money and make online payments.", null, "Papara", "+90 850 340 0 340", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Papara", null, "Papara", "poscompany", "https://www.papara.com" },
                    { 61L, "Istanbul, Turkey", (short)1, "PayCore (formerly Cardtek) is a Turkish payment technology company that develops end-to-end digital payment solutions and EMV card infrastructure for banks and fintechs.", "info@paycore.com", 450, 2001, "Istanbul, Turkey", true, null, "", "PayCore (formerly Cardtek) is a Turkish payment technology company that develops end-to-end digital payment solutions and EMV card infrastructure for banks and fintechs.", null, "PayCore", "+90 216 000 0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "PayCore", null, "PayCore", "poscompany", "https://www.paycore.com" },
                    { 62L, "Cape Town, South Africa", (short)1, "PayFast is a South African online payment gateway that enables businesses and individuals to send and receive payments online easily and securely.", "support@payfast.co.za", 100, 2007, "Cape Town, South Africa", true, null, "", "PayFast is a South African online payment gateway that enables businesses and individuals to send and receive payments online easily and securely.", null, "PayFast", "+27 21 300 4455", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "PayFast", null, "PayFast", "poscompany", "https://www.payfast.co.za" },
                    { 63L, "2211 North First Street, San Jose, CA, USA", (short)1, "PayPal is an American online payments platform that allows individuals and businesses to send and receive money securely across the globe.", "support@paypal.com", 27200, 1998, "San Jose, USA", true, null, "", "PayPal is an American online payments platform that allows individuals and businesses to send and receive money securely across the globe.", null, "PayPal", "+1 888-221-1161", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "PayPal", null, "PayPal", "poscompany", "https://www.paypal.com" },
                    { 64L, "Izmir, Turkey", (short)1, "PayTR is a Turkish payment service provider that offers secure online payment solutions and e-wallet services for businesses and consumers.", "destek@paytr.com", 200, 2009, "Izmir, Turkey", true, null, "", "PayTR is a Turkish payment service provider that offers secure online payment solutions and e-wallet services for businesses and consumers.", null, "PayTR", "+90 850 811 0 729", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "PayTR", null, "PayTR", "poscompany", "https://www.paytr.com" },
                    { 65L, "Al Khobar, Saudi Arabia", (short)1, "PayTabs is a Saudi-based online payment gateway provider that offers secure payment processing solutions for businesses in the Middle East and beyond.", "sales@paytabs.com", 150, 2014, "Al Khobar, Saudi Arabia", true, null, "", "PayTabs is a Saudi-based online payment gateway provider that offers secure payment processing solutions for businesses in the Middle East and beyond.", null, "PayTabs", "+966 13 000 0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "PayTabs", null, "PayTabs", "poscompany", "https://www.paytabs.com" },
                    { 66L, "Hoofddorp, Netherlands", (short)1, "PayU is a Netherlands-based payment service provider (part of Prosus) that operates in emerging markets, enabling online merchants to accept local payment methods.", "support@payu.com", 3000, 2002, "Hoofddorp, Netherlands", true, null, "", "PayU is a Netherlands-based payment service provider (part of Prosus) that operates in emerging markets, enabling online merchants to accept local payment methods.", null, "PayU", "+31 20 000 0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "PayU", null, "PayU", "poscompany", "https://www.payu.com" },
                    { 67L, "New York, NY, USA", (short)1, "Payoneer is an American financial services company that offers online money transfer, digital payment services, and prepaid cards for cross-border payments.", "support@payoneer.com", 2000, 2005, "New York, USA", true, null, "", "Payoneer is an American financial services company that offers online money transfer, digital payment services, and prepaid cards for cross-border payments.", null, "Payoneer", "+1 212-600-9272", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Payoneer", null, "Payoneer", "poscompany", "https://www.payoneer.com" },
                    { 68L, "London, UK", (short)1, "Paysafe is a UK-based payments company that provides digital payment solutions, including online wallets (Skrill, Neteller) and payment processing services.", "info@paysafe.com", 3200, 1996, "London, UK", true, null, "", "Paysafe is a UK-based payments company that provides digital payment solutions, including online wallets (Skrill, Neteller) and payment processing services.", null, "Paysafe", "+44 20 3884 0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Paysafe", null, "Paysafe", "poscompany", "https://www.paysafe.com" },
                    { 69L, "Noida, India", (short)1, "Paytm is an Indian fintech company offering a mobile wallet, e-commerce payment system, and financial services, widely used for digital payments in India.", "care@paytm.com", 10000, 2010, "Noida, India", true, null, "", "Paytm is an Indian fintech company offering a mobile wallet, e-commerce payment system, and financial services, widely used for digital payments in India.", null, "Paytm", "+91 120 4770770", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Paytm", null, "Paytm", "poscompany", "https://www.paytm.com" },
                    { 70L, "Bangalore, India", (short)1, "PhonePe is an Indian digital payments platform that provides mobile payment and financial services through the Unified Payments Interface (UPI) and other offerings.", "support@phonepe.com", 5000, 2015, "Bangalore, India", true, null, "", "PhonePe is an Indian digital payments platform that provides mobile payment and financial services through the Unified Payments Interface (UPI) and other offerings.", null, "PhonePe", "+91 80 0000 0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "PhonePe", null, "PhonePe", "poscompany", "https://www.phonepe.com" },
                    { 71L, "Palo Alto, CA, USA", (short)1, "Poynt is an American maker of smart point-of-sale terminals and software, which allows merchants to manage payments and apps on a connected device (acquired by GoDaddy).", "info@poynt.com", 150, 2014, "Palo Alto, USA", true, null, "", "Poynt is an American maker of smart point-of-sale terminals and software, which allows merchants to manage payments and apps on a connected device (acquired by GoDaddy).", null, "Poynt", "+1 650-781-8000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Poynt", null, "Poynt", "poscompany", "https://www.poynt.com" },
                    { 72L, "London, UK", (short)1, "Rapyd is a UK-based fintech-as-a-service platform that offers a unified cloud-based payment infrastructure enabling businesses to integrate local payment methods worldwide.", "support@rapyd.net", 500, 2016, "London, UK", true, null, "", "Rapyd is a UK-based fintech-as-a-service platform that offers a unified cloud-based payment infrastructure enabling businesses to integrate local payment methods worldwide.", null, "Rapyd", "+44 20 0000 0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Rapyd", null, "Rapyd", "poscompany", "https://www.rapyd.net" },
                    { 73L, "Bangalore, India", (short)1, "Razorpay is an Indian fintech platform that provides payment gateway services, allowing businesses to accept, process, and disburse payments online.", "support@razorpay.com", 3000, 2014, "Bangalore, India", true, null, "", "Razorpay is an Indian fintech platform that provides payment gateway services, allowing businesses to accept, process, and disburse payments online.", null, "Razorpay", "+91 80 0000 0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Razorpay", null, "Razorpay", "poscompany", "https://www.razorpay.com" },
                    { 74L, "Seattle, WA, USA", (short)1, "Remitly is an American fintech company that offers a digital remittance platform, enabling individuals to send money internationally through a mobile app or website.", "service@remitly.com", 1800, 2011, "Seattle, USA", true, null, "", "Remitly is an American fintech company that offers a digital remittance platform, enabling individuals to send money internationally through a mobile app or website.", null, "Remitly", "+1 888-736-4859", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Remitly", null, "Remitly", "poscompany", "https://www.remitly.com" },
                    { 75L, "San Francisco, CA, USA", (short)1, "Revel Systems is an American company that provides an iPad-based point-of-sale system for restaurants and retailers, integrating operations and payments.", "info@revelsystems.com", 500, 2010, "San Francisco, USA", true, null, "", "Revel Systems is an American company that provides an iPad-based point-of-sale system for restaurants and retailers, integrating operations and payments.", null, "Revel Systems", "+1 415-744-1433", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Revel Systems", null, "Revel Systems", "poscompany", "https://www.revelsystems.com" },
                    { 76L, "Allentown, PA, USA", (short)1, "Shift4 Payments is an American payments technology company that provides integrated payment processing and technology solutions to restaurants, hotels, and other businesses.", "sales@shift4.com", 2300, 1999, "Allentown, USA", true, null, "", "Shift4 Payments is an American payments technology company that provides integrated payment processing and technology solutions to restaurants, hotels, and other businesses.", null, "Shift4 Payments", "+1 888-276-2108", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shift4 Payments", null, "Shift4 Payments", "poscompany", "https://www.shift4.com" },
                    { 77L, "Ottawa, Canada", (short)1, "Shopify is a Canadian e-commerce company that provides an online platform for businesses, including payment processing and point-of-sale systems for in-person sales.", "support@shopify.com", 10000, 2006, "Ottawa, Canada", true, null, "", "Shopify is a Canadian e-commerce company that provides an online platform for businesses, including payment processing and point-of-sale systems for in-person sales.", null, "Shopify", "+1 613-000-0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shopify", null, "Shopify", "poscompany", "https://www.shopify.com" },
                    { 78L, "Istanbul, Turkey", (short)1, "Sipay is a Turkish fintech company offering digital payment solutions, including a payment gateway, digital wallet, and installment payment services.", "info@sipay.com", 100, 2018, "Istanbul, Turkey", true, null, "", "Sipay is a Turkish fintech company offering digital payment solutions, including a payment gateway, digital wallet, and installment payment services.", null, "Sipay", "+90 212 000 0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sipay", null, "Sipay", "poscompany", "https://www.sipay.com" },
                    { 79L, "London, UK", (short)1, "Skrill is a UK-based digital wallet and online payments company that allows users to make transfers and payments online (part of Paysafe Group).", "help@skrill.com", 500, 2001, "London, UK", true, null, "", "Skrill is a UK-based digital wallet and online payments company that allows users to make transfers and payments online (part of Paysafe Group).", null, "Skrill", "+44 20 3308 2520", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Skrill", null, "Skrill", "poscompany", "https://www.skrill.com" },
                    { 80L, "San Francisco, CA, USA", (short)1, "Square (now Block, Inc.) is an American financial services and digital payments company that offers point-of-sale software and hardware, and mobile payment solutions for merchants.", "support@squareup.com", 6000, 2009, "San Francisco, USA", true, null, "", "Square (now Block, Inc.) is an American financial services and digital payments company that offers point-of-sale software and hardware, and mobile payment solutions for merchants.", null, "Square", "+1 855-700-6000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Square", null, "Square", "poscompany", "https://squareup.com" },
                    { 81L, "São Paulo, Brazil", (short)1, "StoneCo is a Brazilian fintech company that provides payment processing, point-of-sale technology, and financial services to merchants in Brazil.", "contato@stone.com.br", 7000, 2012, "São Paulo, Brazil", true, null, "", "StoneCo is a Brazilian fintech company that provides payment processing, point-of-sale technology, and financial services to merchants in Brazil.", null, "Stone (StoneCo)", "+55 11 0000 0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Stone (StoneCo)", null, "Stone (StoneCo)", "poscompany", "https://www.stone.com.br" },
                    { 82L, "San Francisco, CA, USA", (short)1, "Stripe is an American payment processing platform that allows businesses to accept payments online and via mobile apps, providing a suite of APIs for developers.", "support@stripe.com", 7000, 2010, "San Francisco, USA", true, null, "", "Stripe is an American payment processing platform that allows businesses to accept payments online and via mobile apps, providing a suite of APIs for developers.", null, "Stripe", "+1 888-963-8955", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Stripe", null, "Stripe", "poscompany", "https://www.stripe.com" },
                    { 83L, "London, UK", (short)1, "SumUp is a UK-based fintech company that provides mobile card readers and point-of-sale solutions, enabling small merchants to accept card payments anywhere.", "support@sumup.com", 3000, 2012, "London, UK", true, null, "", "SumUp is a UK-based fintech company that provides mobile card readers and point-of-sale solutions, enabling small merchants to accept card payments anywhere.", null, "SumUp", "+44 20 7666 1767", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SumUp", null, "SumUp", "poscompany", "https://www.sumup.com" },
                    { 84L, "Columbus, GA, USA", (short)1, "TSYS (Total System Services) is an American payment processor that provides payment processing and card issuing services to banks and merchants worldwide.", "info@tsys.com", 11000, 1983, "Columbus, USA", true, null, "", "TSYS (Total System Services) is an American payment processor that provides payment processing and card issuing services to banks and merchants worldwide.", null, "TSYS", "+1 844-663-8797", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "TSYS", null, "TSYS", "poscompany", "https://www.tsys.com" },
                    { 85L, "Boston, MA, USA", (short)1, "Toast is an American restaurant technology company that offers a cloud-based point-of-sale system specifically designed for restaurants, including integrated payment processing.", "info@toasttab.com", 3000, 2012, "Boston, USA", true, null, "", "Toast is an American restaurant technology company that offers a cloud-based point-of-sale system specifically designed for restaurants, including integrated payment processing.", null, "Toast", "+1 617-682-0225", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Toast", null, "Toast", "poscompany", "https://www.toasttab.com" },
                    { 86L, "Stockholm, Sweden", (short)1, "Trustly is a Swedish fintech company that enables online bank payments directly from consumer bank accounts, providing an alternative to card payments across Europe.", "support@trustly.com", 500, 2008, "Stockholm, Sweden", true, null, "", "Trustly is a Swedish fintech company that enables online bank payments directly from consumer bank accounts, providing an alternative to card payments across Europe.", null, "Trustly", "+46 8 446 831 33", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Trustly", null, "Trustly", "poscompany", "https://www.trustly.com" },
                    { 87L, "Sydney, Australia", (short)1, "Tyro is an Australian challenger bank specializing in merchant credit and EFTPOS services, providing payment processing and banking solutions for SMEs.", "customersupport@tyro.com", 600, 2003, "Sydney, Australia", true, null, "", "Tyro is an Australian challenger bank specializing in merchant credit and EFTPOS services, providing payment processing and banking solutions for SMEs.", null, "Tyro", "+61 2 8907 1700", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tyro", null, "Tyro", "poscompany", "https://www.tyro.com" },
                    { 88L, "Shanghai, China", (short)1, "UnionPay is a Chinese financial services corporation that operates the UnionPay card network, the largest card payment network in the world by transaction volume.", "service@unionpayintl.com", 5000, 2002, "Shanghai, China", true, null, "", "UnionPay is a Chinese financial services corporation that operates the UnionPay card network, the largest card payment network in the world by transaction volume.", null, "UnionPay", "+86 21 2026 5828", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "UnionPay", null, "UnionPay", "poscompany", "https://www.unionpayintl.com" },
                    { 89L, "San Jose, CA, USA", (short)1, "Verifone is an American company that provides secure electronic payment technologies, specializing in point-of-sale payment terminals and payment software.", "sales@verifone.com", 5000, 1981, "San Jose, USA", true, null, "", "Verifone is an American company that provides secure electronic payment technologies, specializing in point-of-sale payment terminals and payment software.", null, "Verifone", "+1 408-232-7800", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Verifone", null, "Verifone", "poscompany", "https://www.verifone.com" },
                    { 90L, "Foster City, CA, USA", (short)1, "Visa Inc. is an American multinational financial services corporation known for its global electronic payments network, facilitating card transactions between consumers, merchants, and banks.", "askvisa@visa.com", 21500, 1958, "Foster City, USA", true, null, "", "Visa Inc. is an American multinational financial services corporation known for its global electronic payments network, facilitating card transactions between consumers, merchants, and banks.", null, "Visa", "+1 650-432-3200", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Visa", null, "Visa", "poscompany", "https://www.visa.com" },
                    { 91L, "Shenzhen, China", (short)1, "WeChat Pay is a Chinese mobile payment and digital wallet service integrated into the WeChat app (by Tencent), enabling users to make payments and money transfers via smartphone.", "support@wechat.com", 10000, 2013, "Shenzhen, China", true, null, "", "WeChat Pay is a Chinese mobile payment and digital wallet service integrated into the WeChat app (by Tencent), enabling users to make payments and money transfers via smartphone.", null, "WeChat Pay", "+86 755 86013388", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "WeChat Pay", null, "WeChat Pay", "poscompany", "https://www.wechat.com" },
                    { 92L, "Redwood City, CA, USA", (short)1, "WePay is an American online payment service provider that offers integrated payments for platforms and marketplaces (acquired by JPMorgan Chase).", "support@wepay.com", 300, 2008, "Redwood City, USA", true, null, "", "WePay is an American online payment service provider that offers integrated payments for platforms and marketplaces (acquired by JPMorgan Chase).", null, "WePay", "+1 855-469-3729", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "WePay", null, "WePay", "poscompany", "https://www.wepay.com" },
                    { 93L, "Denver, CO, USA", (short)1, "Western Union is an American financial services company specializing in international money transfers, allowing individuals to send and receive money across the globe.", "customer@westernunion.com", 11000, 1851, "Denver, USA", true, null, "", "Western Union is an American financial services company specializing in international money transfers, allowing individuals to send and receive money across the globe.", null, "Western Union", "+1 800-325-6000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Western Union", null, "Western Union", "poscompany", "https://www.westernunion.com" },
                    { 94L, "Auckland, New Zealand", (short)1, "Windcave (formerly Payment Express) is a New Zealand-based payment gateway that provides secure payment processing solutions for in-store, online, and unattended payments.", "support@windcave.com", 200, 1997, "Auckland, New Zealand", true, null, "", "Windcave (formerly Payment Express) is a New Zealand-based payment gateway that provides secure payment processing solutions for in-store, online, and unattended payments.", null, "Windcave", "+64 9 123 4567", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Windcave", null, "Windcave", "poscompany", "https://www.windcave.com" },
                    { 95L, "London, UK", (short)1, "Wise (formerly TransferWise) is a British fintech company offering an online platform for international money transfers with low fees and transparent exchange rates.", "support@wise.com", 4000, 2011, "London, UK", true, null, "", "Wise (formerly TransferWise) is a British fintech company offering an online platform for international money transfers with low fees and transparent exchange rates.", null, "Wise", "+44 20 3695 0999", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Wise", null, "Wise", "poscompany", "https://www.wise.com" },
                    { 96L, "Paris, France", (short)1, "Worldline is a French multinational payment and transactional services company that provides merchants, banks, and governments with e-payment services and point-of-sale technology.", "info@worldline.com", 20000, 1973, "Paris, France", true, null, "", "Worldline is a French multinational payment and transactional services company that provides merchants, banks, and governments with e-payment services and point-of-sale technology.", null, "Worldline", "+33 1 0000 0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Worldline", null, "Worldline", "poscompany", "https://www.worldline.com" },
                    { 97L, "London, UK", (short)1, "Worldpay is a global payment processing company (originally from the UK) that provides merchant acquiring and payment processing services for businesses worldwide.", "support@worldpay.com", 8000, 1989, "London, UK", true, null, "", "Worldpay is a global payment processing company (originally from the UK) that provides merchant acquiring and payment processing services for businesses worldwide.", null, "Worldpay", "+44 20 0000 0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Worldpay", null, "Worldpay", "poscompany", "https://www.worldpay.com" },
                    { 98L, "Jakarta, Indonesia", (short)1, "Xendit is an Indonesian payment gateway startup that offers businesses in Southeast Asia a platform for online payments, disbursements, and e-wallet integrations.", "help@xendit.co", 700, 2015, "Jakarta, Indonesia", true, null, "", "Xendit is an Indonesian payment gateway startup that offers businesses in Southeast Asia a platform for online payments, disbursements, and e-wallet integrations.", null, "Xendit", "+62 21 0000 0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Xendit", null, "Xendit", "poscompany", "https://www.xendit.co" },
                    { 99L, "Cape Town, South Africa", (short)1, "Yoco is a South African fintech company that provides portable card readers and payment solutions for small businesses to accept card payments easily.", "hello@yoco.com", 200, 2013, "Cape Town, South Africa", true, null, "", "Yoco is a South African fintech company that provides portable card readers and payment solutions for small businesses to accept card payments easily.", null, "Yoco", "+27 21 000 0000", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Yoco", null, "Yoco", "poscompany", "https://www.yoco.com" },
                    { 100L, "Sydney, Australia", (short)1, "Zip Co is an Australian buy now, pay later provider that offers point-of-sale credit and digital payment solutions, allowing consumers to pay in installments.", "info@zip.co", 1000, 2013, "Sydney, Australia", true, null, "", "Zip Co is an Australian buy now, pay later provider that offers point-of-sale credit and digital payment solutions, allowing consumers to pay in installments.", null, "Zip", "+61 2 8294 2345", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Zip", null, "Zip", "poscompany", "https://www.zip.co" }
                });

            migrationBuilder.InsertData(
                schema: "posonl",
                table: "LocalizationEntries",
                columns: new[] { "Id", "CultureId", "Description", "IsPublished", "LocalizationType", "MetaDescription", "MetaKeywords", "Name", "PublishedDate", "Slug", "Title", "ViewType" },
                values: new object[,]
                {
                    { 15001L, (short)1, "PaymentMethodsDescription", true, null, "", null, "PaymentMethods", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "PaymentMethods", "PaymentMethods", "PosServiceCategory" },
                    { 15002L, (short)1, "HardwareSolutionsDescription", true, null, "", null, "HardwareSolutions", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "HardwareSolutions", "HardwareSolutions", "PosServiceCategory" },
                    { 15003L, (short)1, "BillingAndAccountingDescription", true, null, "", null, "BillingAndAccounting", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "BillingAndAccounting", "BillingAndAccounting", "PosServiceCategory" },
                    { 15004L, (short)1, "EcommerceAndOnlinePaymentsDescription", true, null, "", null, "EcommerceAndOnlinePayments", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "EcommerceAndOnlinePayments", "EcommerceAndOnlinePayments", "PosServiceCategory" },
                    { 15005L, (short)1, "SecurityAndFraudPreventionDescription", true, null, "", null, "SecurityAndFraudPrevention", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SecurityAndFraudPrevention", "SecurityAndFraudPrevention", "PosServiceCategory" },
                    { 15006L, (short)1, "CustomerAndLoyaltyProgramsDescription", true, null, "", null, "CustomerAndLoyaltyPrograms", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "CustomerAndLoyaltyPrograms", "CustomerAndLoyaltyPrograms", "PosServiceCategory" },
                    { 15007L, (short)1, "MultiCurrencyAndCrossBorderPaymentsDescription", true, null, "", null, "MultiCurrencyAndCrossBorderPayments", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "MultiCurrencyAndCrossBorderPayments", "MultiCurrencyAndCrossBorderPayments", "PosServiceCategory" },
                    { 15008L, (short)1, "ReportingAndAnalyticsDescription", true, null, "", null, "ReportingAndAnalytics", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ReportingAndAnalytics", "ReportingAndAnalytics", "PosServiceCategory" },
                    { 15009L, (short)1, "SubscriptionAndRecurringPaymentsDescription", true, null, "", null, "SubscriptionAndRecurringPayments", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SubscriptionAndRecurringPayments", "SubscriptionAndRecurringPayments", "PosServiceCategory" },
                    { 15010L, (short)1, "RegionalSpecificPOSServicesDescription", true, null, "", null, "RegionalSpecificPOSServices", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "RegionalSpecificPOSServices", "RegionalSpecificPOSServices", "PosServiceCategory" }
                });

            migrationBuilder.InsertData(
                schema: "posonl",
                table: "LocalizationEntries",
                columns: new[] { "Id", "CountryId", "CultureId", "Description", "IsGlobal", "IsPublished", "IsRegional", "LocalizationType", "MetaDescription", "MetaKeywords", "Name", "PosServiceCategoryId", "PublishedDate", "Slug", "Title", "ViewType" },
                values: new object[,]
                {
                    { 1001L, null, (short)1, "CreditCardProcessingDescription", true, true, false, null, "CreditCardProcessing", null, "CreditCardProcessing", 15001L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "CreditCardProcessing", "CreditCardProcessing", "posservice" },
                    { 1002L, null, (short)1, "ContactlessPaymentsDescription", true, true, false, null, "ContactlessPayments", null, "ContactlessPayments", 15001L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ContactlessPayments", "ContactlessPayments", "posservice" },
                    { 1003L, null, (short)1, "EMVChipPaymentsDescription", true, true, false, null, "EMVChipPayments", null, "EMVChipPayments", 15001L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "EMVChipPayments", "EMVChipPayments", "posservice" },
                    { 1004L, null, (short)1, "MobilePaymentsDescription", true, true, false, null, "MobilePayments", null, "MobilePayments", 15001L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "MobilePayments", "MobilePayments", "posservice" },
                    { 1005L, null, (short)1, "CryptoPaymentsDescription", true, true, false, null, "CryptoPayments", null, "CryptoPayments", 15001L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "CryptoPayments", "CryptoPayments", "posservice" },
                    { 1006L, null, (short)1, "BKMExpressPaymentDescription", true, true, true, null, "BKMExpressPayment", null, "BKMExpressPayment", 15001L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "BKMExpressPayment", "BKMExpressPayment", "posservice" },
                    { 1007L, null, (short)1, "UPIPaymentsIndiaDescription", true, true, true, null, "UPIPaymentsIndia", null, "UPIPaymentsIndia", 15001L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "UPIPaymentsIndia", "UPIPaymentsIndia", "posservice" },
                    { 1008L, null, (short)1, "WeChatPayChinaDescription", true, true, true, null, "WeChatPayChina", null, "WeChatPayChina", 15001L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "WeChatPayChina", "WeChatPayChina", "posservice" },
                    { 1009L, null, (short)1, "AliPayChinaDescription", true, true, true, null, "AliPayChina", null, "AliPayChina", 15001L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "AliPayChina", "AliPayChina", "posservice" },
                    { 1010L, null, (short)1, "iDealNetherlandsDescription", true, true, true, null, "iDealNetherlands", null, "iDealNetherlands", 15001L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "iDealNetherlands", "iDealNetherlands", "posservice" },
                    { 1011L, null, (short)1, "PayNowSingaporeDescription", true, true, true, null, "PayNowSingapore", null, "PayNowSingapore", 15001L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "PayNowSingapore", "PayNowSingapore", "posservice" },
                    { 1012L, null, (short)1, "TraditionalPOSMachinesDescription", true, true, false, null, "TraditionalPOSMachines", null, "TraditionalPOSMachines", 15002L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "TraditionalPOSMachines", "TraditionalPOSMachines", "posservice" },
                    { 1013L, null, (short)1, "WirelessPOSDescription", true, true, false, null, "WirelessPOS", null, "WirelessPOS", 15002L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "WirelessPOS", "WirelessPOS", "posservice" },
                    { 1014L, null, (short)1, "TabletPOSDescription", true, true, false, null, "TabletPOS", null, "TabletPOS", 15002L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "TabletPOS", "TabletPOS", "posservice" },
                    { 1015L, null, (short)1, "SelfCheckoutKiosksDescription", true, true, false, null, "SelfCheckoutKiosks", null, "SelfCheckoutKiosks", 15002L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SelfCheckoutKiosks", "SelfCheckoutKiosks", "posservice" },
                    { 1016L, null, (short)1, "HandheldPOSDescription", true, true, false, null, "HandheldPOS", null, "HandheldPOS", 15002L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "HandheldPOS", "HandheldPOS", "posservice" },
                    { 1017L, null, (short)1, "SquarePOSDescription", true, true, false, null, "SquarePOS", null, "SquarePOS", 15002L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SquarePOS", "SquarePOS", "posservice" },
                    { 1018L, null, (short)1, "ToastPOSDescription", true, true, false, null, "ToastPOS", null, "ToastPOS", 15002L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ToastPOS", "ToastPOS", "posservice" },
                    { 1019L, null, (short)1, "POSDeviceHealthMonitoringDescription", true, true, false, null, "POSDeviceHealthMonitoring", null, "POSDeviceHealthMonitoring", 15002L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "POSDeviceHealthMonitoring", "POSDeviceHealthMonitoring", "posservice" },
                    { 1020L, null, (short)1, "AutomatedInvoicingDescription", true, true, false, null, "AutomatedInvoicing", null, "AutomatedInvoicing", 15003L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "AutomatedInvoicing", "AutomatedInvoicing", "posservice" },
                    { 1021L, null, (short)1, "TaxIntegrationDescription", true, true, false, null, "TaxIntegration", null, "TaxIntegration", 15003L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "TaxIntegration", "TaxIntegration", "posservice" },
                    { 1022L, null, (short)1, "MultiCurrencyBillingDescription", true, true, false, null, "MultiCurrencyBilling", null, "MultiCurrencyBilling", 15003L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "MultiCurrencyBilling", "MultiCurrencyBilling", "posservice" },
                    { 1023L, null, (short)1, "EReceiptGenerationDescription", true, true, false, null, "EReceiptGeneration", null, "EReceiptGeneration", 15003L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "EReceiptGeneration", "EReceiptGeneration", "posservice" },
                    { 1024L, null, (short)1, "POSAccountingIntegrationDescription", true, true, false, null, "POSAccountingIntegration", null, "POSAccountingIntegration", 15003L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "POSAccountingIntegration", "POSAccountingIntegration", "posservice" },
                    { 1025L, null, (short)1, "DunningManagementDescription", true, true, false, null, "DunningManagement", null, "DunningManagement", 15003L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "DunningManagement", "DunningManagement", "posservice" },
                    { 1026L, null, (short)1, "DeferredPaymentProcessingDescription", true, true, false, null, "DeferredPaymentProcessing", null, "DeferredPaymentProcessing", 15003L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "DeferredPaymentProcessing", "DeferredPaymentProcessing", "posservice" },
                    { 1027L, null, (short)1, "PaymentLinksDescription", true, true, false, null, "PaymentLinks", null, "PaymentLinks", 15004L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "PaymentLinks", "PaymentLinks", "posservice" },
                    { 1028L, null, (short)1, "MerchantVirtualTerminalsDescription", true, true, false, null, "MerchantVirtualTerminals", null, "MerchantVirtualTerminals", 15004L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "MerchantVirtualTerminals", "MerchantVirtualTerminals", "posservice" },
                    { 1029L, null, (short)1, "CheckoutOptimizationsDescription", true, true, false, null, "CheckoutOptimizations", null, "CheckoutOptimizations", 15004L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "CheckoutOptimizations", "CheckoutOptimizations", "posservice" },
                    { 1030L, null, (short)1, "SubscriptionBillingDescription", true, true, false, null, "SubscriptionBilling", null, "SubscriptionBilling", 15009L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SubscriptionBilling", "SubscriptionBilling", "posservice" },
                    { 1031L, null, (short)1, "AutomatedSubscriptionBillingDescription", true, true, false, null, "AutomatedSubscriptionBilling", null, "AutomatedSubscriptionBilling", 15009L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "AutomatedSubscriptionBilling", "AutomatedSubscriptionBilling", "posservice" },
                    { 1032L, null, (short)1, "RecurringPaymentManagementDescription", true, true, false, null, "RecurringPaymentManagement", null, "RecurringPaymentManagement", 15009L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "RecurringPaymentManagement", "RecurringPaymentManagement", "posservice" },
                    { 1033L, null, (short)1, "UsageBasedBillingDescription", true, true, false, null, "UsageBasedBilling", null, "UsageBasedBilling", 15009L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "UsageBasedBilling", "UsageBasedBilling", "posservice" },
                    { 1034L, null, (short)1, "TwoFactorAuthenticationDescription", true, true, false, null, "TwoFactorAuthentication", null, "TwoFactorAuthentication", 15005L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "TwoFactorAuthentication", "TwoFactorAuthentication", "posservice" },
                    { 1035L, null, (short)1, "AITransactionMonitoringDescription", true, true, false, null, "AITransactionMonitoring", null, "AITransactionMonitoring", 15005L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "AITransactionMonitoring", "AITransactionMonitoring", "posservice" },
                    { 1036L, null, (short)1, "ChargebackProtectionDescription", true, true, false, null, "ChargebackProtection", null, "ChargebackProtection", 15005L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ChargebackProtection", "ChargebackProtection", "posservice" },
                    { 1037L, null, (short)1, "DeviceFingerprintingDescription", true, true, false, null, "DeviceFingerprinting", null, "DeviceFingerprinting", 15005L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "DeviceFingerprinting", "DeviceFingerprinting", "posservice" },
                    { 1038L, null, (short)1, "RiskScoringDescription", true, true, false, null, "RiskScoring", null, "RiskScoring", 15005L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "RiskScoring", "RiskScoring", "posservice" },
                    { 1039L, null, (short)1, "GiftCardManagementDescription", true, true, false, null, "GiftCardManagement", null, "GiftCardManagement", 15006L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "GiftCardManagement", "GiftCardManagement", "posservice" },
                    { 1040L, null, (short)1, "LoyaltyRewardsDescription", true, true, false, null, "LoyaltyRewards", null, "LoyaltyRewards", 15006L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "LoyaltyRewards", "LoyaltyRewards", "posservice" },
                    { 1041L, null, (short)1, "PointRedemptionDescription", true, true, false, null, "PointRedemption", null, "PointRedemption", 15006L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "PointRedemption", "PointRedemption", "posservice" },
                    { 1042L, null, (short)1, "CashbackOffersDescription", true, true, false, null, "CashbackOffers", null, "CashbackOffers", 15006L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "CashbackOffers", "CashbackOffers", "posservice" },
                    { 1043L, null, (short)1, "DynamicCurrencyConversionDescription", true, true, false, null, "DynamicCurrencyConversion", null, "DynamicCurrencyConversion", 15007L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "DynamicCurrencyConversion", "DynamicCurrencyConversion", "posservice" },
                    { 1044L, null, (short)1, "InternationalWireTransfersDescription", true, true, false, null, "InternationalWireTransfers", null, "InternationalWireTransfers", 15007L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "InternationalWireTransfers", "InternationalWireTransfers", "posservice" },
                    { 1045L, null, (short)1, "MultiCurrencyPOSBillingDescription", true, true, false, null, "MultiCurrencyPOSBilling", null, "MultiCurrencyPOSBilling", 15007L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "MultiCurrencyPOSBilling", "MultiCurrencyPOSBilling", "posservice" },
                    { 1046L, null, (short)1, "ForexRateLockingDescription", true, true, false, null, "ForexRateLocking", null, "ForexRateLocking", 15007L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ForexRateLocking", "ForexRateLocking", "posservice" },
                    { 1047L, null, (short)1, "CrossBorderTransactionSecurityDescription", true, true, false, null, "CrossBorderTransactionSecurity", null, "CrossBorderTransactionSecurity", 15007L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "CrossBorderTransactionSecurity", "CrossBorderTransactionSecurity", "posservice" },
                    { 1048L, null, (short)1, "CustomerDataAnalyticsDescription", true, true, false, null, "CustomerDataAnalytics", null, "CustomerDataAnalytics", 15008L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "CustomerDataAnalytics", "CustomerDataAnalytics", "posservice" },
                    { 1049L, null, (short)1, "SalesAnalyticsDescription", true, true, false, null, "SalesAnalytics", null, "SalesAnalytics", 15008L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SalesAnalytics", "SalesAnalytics", "posservice" },
                    { 1050L, null, (short)1, "CustomerBehaviorReportsDescription", true, true, false, null, "CustomerBehaviorReports", null, "CustomerBehaviorReports", 15008L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "CustomerBehaviorReports", "CustomerBehaviorReports", "posservice" },
                    { 1051L, null, (short)1, "InventoryTrackingDescription", true, true, false, null, "InventoryTracking", null, "InventoryTracking", 15008L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "InventoryTracking", "InventoryTracking", "posservice" },
                    { 1052L, null, (short)1, "RevenueForecastingDescription", true, true, false, null, "RevenueForecasting", null, "RevenueForecasting", 15008L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "RevenueForecasting", "RevenueForecasting", "posservice" },
                    { 1053L, null, (short)1, "BuyNowPayLaterDescription", true, true, false, null, "BuyNowPayLater", null, "BuyNowPayLater", 1L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "BuyNowPayLater", "BuyNowPayLater", "posservice" },
                    { 1054L, null, (short)1, "SplitPaymentsDescription", true, true, false, null, "SplitPayments", null, "SplitPayments", 1L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "SplitPayments", "SplitPayments", "posservice" },
                    { 1055L, null, (short)1, "ChaseMerchantServicesDescription", true, true, false, null, "ChaseMerchantServices", null, "ChaseMerchantServices", 1L, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ChaseMerchantServices", "ChaseMerchantServices", "posservice" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Country_CountryGroupId",
                schema: "posonl",
                table: "Country",
                column: "CountryGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_CountryGroupPosService_SupportedCountryGroupsId",
                schema: "posonl",
                table: "CountryGroupPosService",
                column: "SupportedCountryGroupsId");

            migrationBuilder.CreateIndex(
                name: "IX_CountryPosCompany_SupportedCountriesId",
                schema: "posonl",
                table: "CountryPosCompany",
                column: "SupportedCountriesId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationEntries_CountryId",
                schema: "posonl",
                table: "LocalizationEntries",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationEntries_CultureId",
                schema: "posonl",
                table: "LocalizationEntries",
                column: "CultureId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationEntries_PosServiceCategoryId",
                schema: "posonl",
                table: "LocalizationEntries",
                column: "PosServiceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationEntries_Slug",
                schema: "posonl",
                table: "LocalizationEntries",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PosCommissionRate_CountryId",
                schema: "posonl",
                table: "PosCommissionRate",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_PosCommissionRate_PosCompanyId",
                schema: "posonl",
                table: "PosCommissionRate",
                column: "PosCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PosCompanyPosService_PosServicesId",
                schema: "posonl",
                table: "PosCompanyPosService",
                column: "PosServicesId");

            migrationBuilder.CreateIndex(
                name: "IX_PosCompanyRating_PosCompanyId",
                schema: "posonl",
                table: "PosCompanyRating",
                column: "PosCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PosCompanyRating_RatingCategoryId",
                schema: "posonl",
                table: "PosCompanyRating",
                column: "RatingCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CountryGroupPosService",
                schema: "posonl");

            migrationBuilder.DropTable(
                name: "CountryPosCompany",
                schema: "posonl");

            migrationBuilder.DropTable(
                name: "PosCommissionRate",
                schema: "posonl");

            migrationBuilder.DropTable(
                name: "PosCompanyPosService",
                schema: "posonl");

            migrationBuilder.DropTable(
                name: "PosCompanyRating",
                schema: "posonl");

            migrationBuilder.DropTable(
                name: "LocalizationEntries",
                schema: "posonl");

            migrationBuilder.DropTable(
                name: "Country",
                schema: "posonl");

            migrationBuilder.DropTable(
                name: "Cultures",
                schema: "posonl");

            migrationBuilder.DropTable(
                name: "CountryGroup",
                schema: "posonl");
        }
    }
}
