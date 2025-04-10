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
                name: "HomePages",
                schema: "posonl",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomePages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PosCompanies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Headquarters = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmployeeCount = table.Column<int>(type: "int", nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FoundedYear = table.Column<int>(type: "int", nullable: false),
                    StockTicker = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosCompanies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PosServiceCategory",
                schema: "posonl",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosServiceCategory", x => x.Id);
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
                name: "RatingCategory",
                schema: "posonl",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CultureId = table.Column<short>(type: "smallint", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetaDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetaKeywords = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatingCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RatingCategory_Cultures_CultureId",
                        column: x => x.CultureId,
                        principalSchema: "posonl",
                        principalTable: "Cultures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HomePageLocalizations",
                schema: "posonl",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HomePageId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    MetaDescription = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    MetaKeywords = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CultureId = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomePageLocalizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HomePageLocalizations_Cultures_CultureId",
                        column: x => x.CultureId,
                        principalSchema: "posonl",
                        principalTable: "Cultures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HomePageLocalizations_HomePages_HomePageId",
                        column: x => x.HomePageId,
                        principalSchema: "posonl",
                        principalTable: "HomePages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PosCompanyLocalization",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PosCompanyId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetaDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetaKeywords = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CultureId = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosCompanyLocalization", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PosCompanyLocalization_Cultures_CultureId",
                        column: x => x.CultureId,
                        principalSchema: "posonl",
                        principalTable: "Cultures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PosCompanyLocalization_PosCompanies_PosCompanyId",
                        column: x => x.PosCompanyId,
                        principalTable: "PosCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PosServiceCategoryLocalization",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PosServiceCategoryId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetaDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetaKeywords = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CultureId = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosServiceCategoryLocalization", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PosServiceCategoryLocalization_Cultures_CultureId",
                        column: x => x.CultureId,
                        principalSchema: "posonl",
                        principalTable: "Cultures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PosServiceCategoryLocalization_PosServiceCategory_PosServiceCategoryId",
                        column: x => x.PosServiceCategoryId,
                        principalSchema: "posonl",
                        principalTable: "PosServiceCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                        name: "FK_PosCommissionRate_PosCompanies_PosCompanyId",
                        column: x => x.PosCompanyId,
                        principalTable: "PosCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PosServices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PosServiceCategoryId = table.Column<long>(type: "bigint", nullable: false),
                    IsGlobal = table.Column<bool>(type: "bit", nullable: false),
                    IsRegional = table.Column<bool>(type: "bit", nullable: false),
                    CountryId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PosServices_Country_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "posonl",
                        principalTable: "Country",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PosServices_PosServiceCategory_PosServiceCategoryId",
                        column: x => x.PosServiceCategoryId,
                        principalSchema: "posonl",
                        principalTable: "PosServiceCategory",
                        principalColumn: "Id");
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
                        name: "FK_PosCompanyRating_PosCompanies_PosCompanyId",
                        column: x => x.PosCompanyId,
                        principalTable: "PosCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PosCompanyRating_RatingCategory_RatingCategoryId",
                        column: x => x.RatingCategoryId,
                        principalSchema: "posonl",
                        principalTable: "RatingCategory",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CountryGroupPosService",
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
                        name: "FK_CountryGroupPosService_PosServices_PosServicesId",
                        column: x => x.PosServicesId,
                        principalTable: "PosServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PosCompanyPosService",
                columns: table => new
                {
                    PosCompaniesId = table.Column<long>(type: "bigint", nullable: false),
                    PosServicesId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosCompanyPosService", x => new { x.PosCompaniesId, x.PosServicesId });
                    table.ForeignKey(
                        name: "FK_PosCompanyPosService_PosCompanies_PosCompaniesId",
                        column: x => x.PosCompaniesId,
                        principalTable: "PosCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PosCompanyPosService_PosServices_PosServicesId",
                        column: x => x.PosServicesId,
                        principalTable: "PosServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PosServiceLocalization",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PosServiceId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetaDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetaKeywords = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CultureId = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PosServiceLocalization", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PosServiceLocalization_Cultures_CultureId",
                        column: x => x.CultureId,
                        principalSchema: "posonl",
                        principalTable: "Cultures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PosServiceLocalization_PosServices_PosServiceId",
                        column: x => x.PosServiceId,
                        principalTable: "PosServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    { (short)1, "tr-TR", "Türkçe" },
                    { (short)2, "en-US", "English" },
                    { (short)3, "de-DE", "Deutsch" }
                });

            migrationBuilder.InsertData(
                table: "PosCompanies",
                columns: new[] { "Id", "Address", "Description", "Email", "EmployeeCount", "FoundedYear", "Headquarters", "LogoUrl", "Name", "PhoneNumber", "StockTicker", "Website" },
                values: new object[,]
                {
                    { 1L, "Singapore, Singapore", "2C2P is a Singapore-based payment platform providing payment processing solutions across Asia.", "contact@2c2p.com", 300, 2003, "Singapore, Singapore", "", "2C2P", "+65 3123 4567", null, "https://www.2c2p.com" },
                    { 2L, "Amsterdam, Netherlands", "Adyen is a Dutch payments company that offers a global omnichannel payment platform for businesses.", "info@adyen.com", 4000, 2006, "Amsterdam, Netherlands", "", "Adyen", "+31 20 240 1240", null, "https://www.adyen.com" },
                    { 3L, "San Francisco, CA, USA", "Affirm is an American fintech company that provides buy now, pay later financing for online consumers.", "support@affirm.com", 2000, 2012, "San Francisco, USA", "", "Affirm", "+1 415-123-4567", null, "https://www.affirm.com" },
                    { 4L, "Melbourne, Australia", "Afterpay is an Australian fintech known for its buy now, pay later payment service, allowing shoppers to pay in installments.", "info@afterpay.com", 1000, 2014, "Melbourne, Australia", "", "Afterpay", "+61 3 9100 0000", null, "https://www.afterpay.com" },
                    { 5L, "Hangzhou, China", "Alipay is a Chinese online and mobile payment platform launched by Ant Group, widely used for digital payments in China.", "support@alipay.com", 10000, 2004, "Hangzhou, China", "", "Alipay", "+86 571 2688 8888", null, "https://www.alipay.com" },
                    { 6L, "New York, NY, USA", "American Express (Amex) is an American multinational financial services corporation best known for its credit card, charge card, and traveler's cheque businesses.", "info@americanexpress.com", 64000, 1850, "New York, USA", "", "American Express", "+1 212-640-2000", null, "https://www.americanexpress.com" },
                    { 7L, "Foster City, CA, USA", "Authorize.Net is an American payment gateway service that enables merchants to accept credit card and electronic payments online.", "support@authorize.net", 500, 1996, "Foster City, USA", "", "Authorize.Net", "+1 888-323-4289", null, "https://www.authorize.net" },
                    { 8L, "Mumbai, India", "BillDesk is an Indian online payment gateway platform used for bill payments and other online transactions.", "support@billdesk.com", 800, 2000, "Mumbai, India", "", "BillDesk", "+91 22 7131 0000", null, "https://www.billdesk.com" },
                    { 9L, "Atlanta, GA, USA", "BitPay is an American cryptocurrency payment processor that allows merchants to accept Bitcoin and other crypto payments.", "support@bitpay.com", 100, 2011, "Atlanta, USA", "", "BitPay", "+1 404-890-7700", null, "https://www.bitpay.com" },
                    { 10L, "Waltham, MA, USA", "BlueSnap is a US-based payment gateway that provides an all-in-one payment platform for businesses to accept payments globally.", "info@bluesnap.com", 300, 2001, "Waltham, USA", "", "BlueSnap", "+1 866-312-7733", null, "https://www.bluesnap.com" },
                    { 11L, "Chicago, IL, USA", "Braintree is an American payments company that specializes in mobile and web payment systems for e-commerce (now a PayPal subsidiary).", "support@braintreepayments.com", 500, 2007, "Chicago, USA", "", "Braintree", "+1 877-434-2894", null, "https://www.braintreepayments.com" },
                    { 12L, "Mumbai, India", "CCAvenue is an Indian payment gateway platform that offers online payment processing for e-commerce businesses in India.", "support@ccavenue.com", 300, 2001, "Mumbai, India", "", "CCAvenue", "+91 22 6742 5555", null, "https://www.ccavenue.com" },
                    { 13L, "Dallas, TX, USA", "Chase Paymentech is an American payment processing and merchant acquiring service, part of JPMorgan Chase, serving businesses with payment solutions.", "support@chasepaymentech.com", 1000, 1985, "Dallas, USA", "", "Chase Paymentech", "+1 800-934-7717", null, "https://www.chasepaymentech.com" },
                    { 14L, "London, UK", "Checkout.com is a UK-based payment processor that provides online payment solutions for businesses worldwide.", "info@checkout.com", 1700, 2012, "London, UK", "", "Checkout.com", "+44 20 8068 0000", null, "https://www.checkout.com" },
                    { 15L, "San Francisco, CA, USA", "Chipper Cash is a fintech company that offers a cross-border peer-to-peer payment service across Africa via a mobile-based platform.", "support@chippercash.com", 300, 2018, "San Francisco, USA", "", "Chipper Cash", "+1 415-000-0000", null, "https://www.chippercash.com" },
                    { 16L, "Barueri, São Paulo, Brazil", "Cielo is a Brazilian payment processing company and the largest credit and debit card payment operator in Latin America.", "atendimento@cielo.com.br", 5000, 1995, "Barueri, Brazil", "", "Cielo", "+55 11 3165-2000", null, "https://www.cielo.com.br" },
                    { 17L, "Sunnyvale, CA, USA", "Clover is an American point-of-sale platform offering smart payment terminals and POS software for small and medium businesses (a Fiserv company).", "sales@clover.com", 500, 2010, "Sunnyvale, USA", "", "Clover", "+1 408-123-4567", null, "https://www.clover.com" },
                    { 18L, "Foster City, CA, USA", "CyberSource is an American payment management platform that provides credit card processing, fraud management, and payment security services (owned by Visa).", "support@cybersource.com", 1000, 1994, "Foster City, USA", "", "CyberSource", "+1 888-330-2300", null, "https://www.cybersource.com" },
                    { 19L, "Montevideo, Uruguay", "DLocal is a Uruguayan fintech company that specializes in cross-border payment processing for emerging markets.", "info@dlocal.com", 500, 2016, "Montevideo, Uruguay", "", "DLocal", "+598 000 0000", null, "https://www.dlocal.com" },
                    { 20L, "North Canton, OH, USA", "Diebold Nixdorf is an American multinational that provides ATMs, point-of-sale terminals, and integrated services for banking and retail.", "info@dieboldnixdorf.com", 22000, 1859, "North Canton, USA", "", "Diebold Nixdorf", "+1 330-490-4000", null, "https://www.dieboldnixdorf.com" },
                    { 21L, "Riverwoods, IL, USA", "Discover is an American financial services company that operates Discover Card and network, providing credit card services and payment network solutions.", "support@discover.com", 17000, 1985, "Riverwoods, USA", "", "Discover", "+1 800-347-2683", null, "https://www.discover.com" },
                    { 22L, "Des Moines, IA, USA", "Dwolla is an American fintech company that offers an API platform for bank transfers and ACH payments in the United States.", "support@dwolla.com", 200, 2008, "Des Moines, USA", "", "Dwolla", "+1 515-000-0000", null, "https://www.dwolla.com" },
                    { 23L, "Curitiba, Brazil", "EBANX is a Brazilian fintech company providing local payment solutions in Latin America, enabling global merchants to accept regional payment methods.", "business@ebanx.com", 1000, 2012, "Curitiba, Brazil", "", "EBANX", "+55 41 0000-0000", null, "https://www.ebanx.com" },
                    { 24L, "Atlanta, GA, USA", "Elavon is an American payment processing company (a subsidiary of U.S. Bancorp) that provides merchant acquiring services globally.", "customer@elavon.com", 4000, 1991, "Atlanta, USA", "", "Elavon", "+1 800-725-1243", null, "https://www.elavon.com" },
                    { 25L, "Atlanta, GA, USA", "EVO Payments is an American payment services provider offering merchant acquiring and payment processing solutions to merchants across the world.", "info@evopayments.com", 2400, 1989, "Atlanta, USA", "", "EVO Payments", "+1 770-709-3000", null, "https://www.evopayments.com" },
                    { 26L, "Sydney, Australia", "Eway is an Australian online payment gateway that processes secure credit card payments for merchants, offering omnichannel payment solutions.", "support@eway.com.au", 100, 1998, "Sydney, Australia", "", "Eway", "+61 2 0000 0000", null, "https://www.eway.com.au" },
                    { 27L, "Jacksonville, FL, USA", "FIS (Fidelity National Information Services) is an American provider of financial technology solutions for merchants, banks, and capital markets, including payment processing services.", "info@fisglobal.com", 50000, 1968, "Jacksonville, USA", "", "FIS", "+1 904-438-6000", null, "https://www.fisglobal.com" },
                    { 28L, "Cairo, Egypt", "Fawry is an Egyptian electronic payments network offering a variety of financial services, including bill payments and mobile wallet services.", "support@fawry.com", 1000, 2008, "Cairo, Egypt", "", "Fawry", "+20 2 0000 0000", null, "https://www.fawry.com" },
                    { 29L, "Brookfield, WI, USA", "Fiserv is an American global fintech and payments company that provides payment processing, financial technology, and merchant services (merged with First Data).", "info@fiserv.com", 44000, 1984, "Brookfield, USA", "", "Fiserv", "+1 262-879-5000", null, "https://www.fiserv.com" },
                    { 30L, "San Francisco, CA, USA", "Flutterwave is a payments technology company founded in Nigeria (with HQ in the US) that enables businesses in Africa to accept and process payments across the globe.", "support@flutterwave.com", 400, 2016, "San Francisco, USA", "", "Flutterwave", "+1 415-000-0000", null, "https://www.flutterwave.com" },
                    { 31L, "Taguig, Philippines", "GCash is a Philippine mobile wallet and digital payment service that allows users to pay bills, transfer money, and make purchases via their mobile phones.", "support@gcash.com", 800, 2004, "Taguig, Philippines", "", "GCash", "+63 2 0000 0000", null, "https://www.gcash.com" },
                    { 32L, "Atlanta, GA, USA", "Global Payments Inc. is an American payment technology company offering payment processing and software solutions for merchants, issuers, and consumers worldwide.", "info@globalpay.com", 27000, 2000, "Atlanta, USA", "", "Global Payments", "+1 770-829-8000", null, "https://www.globalpayments.com" },
                    { 33L, "London, UK", "GoCardless is a British fintech company that specializes in recurring payments and direct debit solutions for businesses.", "help@gocardless.com", 500, 2011, "London, UK", "", "GoCardless", "+44 20 7183 8674", null, "https://www.gocardless.com" },
                    { 34L, "Princeton, NJ, USA", "Heartland Payment Systems is an American payment processing company that provides merchant services and payment technology solutions (now part of Global Payments).", "support@heartland.us", 4000, 1997, "Princeton, USA", "", "Heartland Payment Systems", "+1 888-798-3131", null, "https://www.heartland.us" },
                    { 35L, "Paris, France", "Ingenico is a French payment technology company known for its point-of-sale payment terminals and secure electronic transaction solutions.", "sales@ingenico.com", 8000, 1980, "Paris, France", "", "Ingenico", "+33 1 00000000", null, "https://www.ingenico.com" },
                    { 36L, "Istanbul, Turkey", "Ininal is a Turkish fintech company offering a prepaid card and digital wallet platform, enabling users to make cashless payments.", "info@ininal.com", 150, 2012, "Istanbul, Turkey", "", "Ininal", "+90 212 000 0000", null, "https://www.ininal.com" },
                    { 37L, "Lagos, Nigeria", "Interswitch is a Nigerian digital payment processing company providing integrated payment and transaction switching services across Africa.", "info@interswitchng.com", 1000, 2002, "Lagos, Nigeria", "", "Interswitch", "+234 1 000 0000", null, "https://www.interswitchgroup.com" },
                    { 38L, "Istanbul, Turkey", "iyzico is a Turkish fintech company that provides a secure payment gateway and digital wallet solutions for online businesses and marketplaces.", "destek@iyzico.com", 350, 2013, "Istanbul, Turkey", "", "iyzico", "+90 212 909 6972", null, "https://www.iyzico.com" },
                    { 39L, "Stockholm, Sweden", "iZettle is a Swedish fintech company that provides mobile point-of-sale solutions and card readers for small businesses (acquired by PayPal, now operating as Zettle).", "support@izettle.com", 600, 2010, "Stockholm, Sweden", "", "iZettle", "+46 8 0000 0000", null, "https://www.izettle.com" },
                    { 40L, "Tokyo, Japan", "JCB (Japan Credit Bureau) is a Japanese credit card company and payment network offering credit card services accepted worldwide.", "info@jcb.co.jp", 4400, 1961, "Tokyo, Japan", "", "JCB", "+81 3-5778-5400", null, "https://www.global.jcb/en" },
                    { 41L, "Stockholm, Sweden", "Klarna is a Swedish fintech company offering buy now, pay later payment solutions and an online payments platform for shoppers and merchants.", "customer@klarna.com", 5000, 2005, "Stockholm, Sweden", "", "Klarna", "+46 8 120 120 00", null, "https://www.klarna.com" },
                    { 42L, "Montreal, Canada", "Lightspeed is a Canadian point-of-sale and e-commerce software provider that offers cloud-based POS solutions for retail and hospitality businesses.", "info@lightspeedhq.com", 3000, 2005, "Montreal, Canada", "", "Lightspeed", "+1 514-907-1801", null, "https://www.lightspeedhq.com" },
                    { 43L, "Nairobi, Kenya", "M-Pesa is a Kenyan mobile money service operated by Safaricom and Vodacom that allows users to store and transfer money and pay for goods via their mobile phones.", "info@safaricom.co.ke", 500, 2007, "Nairobi, Kenya", "", "M-Pesa", "+254 722 000000", null, "https://www.safaricom.co.ke/personal/m-pesa" },
                    { 44L, "Oakland, CA, USA", "Marqeta is an American fintech company that provides a modern card issuing platform for businesses to create and manage payment cards via APIs.", "sales@marqeta.com", 800, 2010, "Oakland, USA", "", "Marqeta", "+1 510-281-3700", null, "https://www.marqeta.com" },
                    { 45L, "Purchase, NY, USA", "Mastercard is an American multinational financial services corporation that operates a global payment network and offers credit, debit, and prepaid cards.", "info@mastercard.com", 24000, 1966, "Purchase, USA", "", "Mastercard", "+1 914-249-2000", null, "https://www.mastercard.com" },
                    { 46L, "Buenos Aires, Argentina", "MercadoPago is an Argentine fintech platform (part of MercadoLibre) that provides online payment services and digital wallet solutions across Latin America.", "ayuda@mercadopago.com", 2000, 2004, "Buenos Aires, Argentina", "", "MercadoPago", "+54 11 0000 0000", null, "https://www.mercadopago.com" },
                    { 47L, "Amsterdam, Netherlands", "Mollie is a Dutch payment gateway offering simple online payment solutions and integrations for businesses in Europe.", "info@mollie.com", 700, 2004, "Amsterdam, Netherlands", "", "Mollie", "+31 20 820 2080", null, "https://www.mollie.com" },
                    { 48L, "Toronto, Canada", "Moneris is a Canadian payment processing company (a joint venture of RBC and BMO) that provides merchant services and payment processing solutions in North America.", "support@moneris.com", 1500, 2000, "Toronto, Canada", "", "Moneris", "+1 855-423-8472", null, "https://www.moneris.com" },
                    { 49L, "Dallas, TX, USA", "MoneyGram is an American money transfer company that offers global person-to-person money transfer and bill payment services.", "customerservice@moneygram.com", 2500, 1940, "Dallas, USA", "", "MoneyGram", "+1 800-926-9400", null, "https://www.moneygram.com" },
                    { 50L, "Atlanta, GA, USA", "NCR Corporation is an American enterprise technology company that produces ATMs, point-of-sale (POS) systems, and other payment and retail hardware and software.", "contact@ncr.com", 36000, 1884, "Atlanta, USA", "", "NCR Corporation", "+1 937-445-1936", null, "https://www.ncr.com" },
                    { 51L, "London, UK", "Neteller is a UK-based digital wallet service that allows users to transfer money and make payments online (part of Paysafe Group).", "support@neteller.com", 200, 1999, "London, UK", "", "Neteller", "+44 20 3308 9525", null, "https://www.neteller.com" },
                    { 52L, "Ballerup, Denmark", "Nets is a Danish payment services provider that offers digital payment solutions, including card processing and digital ID services, across Northern Europe.", "info@nets.eu", 2600, 1968, "Ballerup, Denmark", "", "Nets", "+45 44 68 44 68", null, "https://www.nets.eu" },
                    { 53L, "Dubai, UAE", "Network International is a UAE-based payment solutions provider offering merchant acquiring, card processing, and digital payment solutions across the Middle East and Africa.", "marketing@network.ae", 1500, 1994, "Dubai, UAE", "", "Network International", "+971 4 303 2432", null, "https://www.network.ae" },
                    { 54L, "Milan, Italy", "Nexi is an Italian digital payments company providing payment technology solutions, including card issuing, merchant services, and digital banking, across Europe.", "info@nexi.it", 10000, 2017, "Milan, Italy", "", "Nexi", "+39 02 3488 0892", null, "https://www.nexi.it" },
                    { 55L, "Montreal, Canada", "Nuvei is a Canadian fintech company that provides payment processing technology and services to merchants, including support for card payments and alternative payment methods.", "support@nuvei.com", 1500, 2003, "Montreal, Canada", "", "Nuvei", "+1 514-390-2030", null, "https://www.nuvei.com" },
                    { 56L, "Jakarta, Indonesia", "OVO is an Indonesian digital wallet and payments service that enables users to make cashless payments, transfer funds, and earn loyalty rewards via a mobile app.", "cs@ovo.id", 500, 2017, "Jakarta, Indonesia", "", "OVO", "+62 21 0000 0000", null, "https://www.ovo.id" },
                    { 57L, "Bangkok, Thailand", "Omise is a Thailand-based payment gateway providing online payment processing solutions in Southeast Asia, with a focus on ease of integration for merchants.", "support@omise.co", 300, 2013, "Bangkok, Thailand", "", "Omise", "+66 2 000 0000", null, "https://www.omise.co" },
                    { 58L, "Shenzhen, China", "PAX Technology is a Chinese manufacturer of payment terminals and POS hardware, supplying secure electronic payment devices to financial institutions worldwide.", "sales@pax.com.cn", 1800, 2000, "Shenzhen, China", "", "PAX Technology", "+86 755 86169630", null, "https://www.paxtechnology.com" },
                    { 59L, "São Paulo, Brazil", "PagSeguro is a Brazilian fintech company offering online payment services, mobile payments, and fintech solutions, including its popular PagBank digital account.", "atendimento@pagseguro.com.br", 8000, 2006, "São Paulo, Brazil", "", "PagSeguro", "+55 11 3028-9000", null, "https://pagseguro.uol.com.br" },
                    { 60L, "Istanbul, Turkey", "Papara is a Turkish fintech company that provides a digital wallet and prepaid card service, enabling users to send money and make online payments.", "support@papara.com", 300, 2016, "Istanbul, Turkey", "", "Papara", "+90 850 340 0 340", null, "https://www.papara.com" },
                    { 61L, "Istanbul, Turkey", "PayCore (formerly Cardtek) is a Turkish payment technology company that develops end-to-end digital payment solutions and EMV card infrastructure for banks and fintechs.", "info@paycore.com", 450, 2001, "Istanbul, Turkey", "", "PayCore", "+90 216 000 0000", null, "https://www.paycore.com" },
                    { 62L, "Cape Town, South Africa", "PayFast is a South African online payment gateway that enables businesses and individuals to send and receive payments online easily and securely.", "support@payfast.co.za", 100, 2007, "Cape Town, South Africa", "", "PayFast", "+27 21 300 4455", null, "https://www.payfast.co.za" },
                    { 63L, "2211 North First Street, San Jose, CA, USA", "PayPal is an American online payments platform that allows individuals and businesses to send and receive money securely across the globe.", "support@paypal.com", 27200, 1998, "San Jose, USA", "", "PayPal", "+1 888-221-1161", null, "https://www.paypal.com" },
                    { 64L, "Izmir, Turkey", "PayTR is a Turkish payment service provider that offers secure online payment solutions and e-wallet services for businesses and consumers.", "destek@paytr.com", 200, 2009, "Izmir, Turkey", "", "PayTR", "+90 850 811 0 729", null, "https://www.paytr.com" },
                    { 65L, "Al Khobar, Saudi Arabia", "PayTabs is a Saudi-based online payment gateway provider that offers secure payment processing solutions for businesses in the Middle East and beyond.", "sales@paytabs.com", 150, 2014, "Al Khobar, Saudi Arabia", "", "PayTabs", "+966 13 000 0000", null, "https://www.paytabs.com" },
                    { 66L, "Hoofddorp, Netherlands", "PayU is a Netherlands-based payment service provider (part of Prosus) that operates in emerging markets, enabling online merchants to accept local payment methods.", "support@payu.com", 3000, 2002, "Hoofddorp, Netherlands", "", "PayU", "+31 20 000 0000", null, "https://www.payu.com" },
                    { 67L, "New York, NY, USA", "Payoneer is an American financial services company that offers online money transfer, digital payment services, and prepaid cards for cross-border payments.", "support@payoneer.com", 2000, 2005, "New York, USA", "", "Payoneer", "+1 212-600-9272", null, "https://www.payoneer.com" },
                    { 68L, "London, UK", "Paysafe is a UK-based payments company that provides digital payment solutions, including online wallets (Skrill, Neteller) and payment processing services.", "info@paysafe.com", 3200, 1996, "London, UK", "", "Paysafe", "+44 20 3884 0000", null, "https://www.paysafe.com" },
                    { 69L, "Noida, India", "Paytm is an Indian fintech company offering a mobile wallet, e-commerce payment system, and financial services, widely used for digital payments in India.", "care@paytm.com", 10000, 2010, "Noida, India", "", "Paytm", "+91 120 4770770", null, "https://www.paytm.com" },
                    { 70L, "Bangalore, India", "PhonePe is an Indian digital payments platform that provides mobile payment and financial services through the Unified Payments Interface (UPI) and other offerings.", "support@phonepe.com", 5000, 2015, "Bangalore, India", "", "PhonePe", "+91 80 0000 0000", null, "https://www.phonepe.com" },
                    { 71L, "Palo Alto, CA, USA", "Poynt is an American maker of smart point-of-sale terminals and software, which allows merchants to manage payments and apps on a connected device (acquired by GoDaddy).", "info@poynt.com", 150, 2014, "Palo Alto, USA", "", "Poynt", "+1 650-781-8000", null, "https://www.poynt.com" },
                    { 72L, "London, UK", "Rapyd is a UK-based fintech-as-a-service platform that offers a unified cloud-based payment infrastructure enabling businesses to integrate local payment methods worldwide.", "support@rapyd.net", 500, 2016, "London, UK", "", "Rapyd", "+44 20 0000 0000", null, "https://www.rapyd.net" },
                    { 73L, "Bangalore, India", "Razorpay is an Indian fintech platform that provides payment gateway services, allowing businesses to accept, process, and disburse payments online.", "support@razorpay.com", 3000, 2014, "Bangalore, India", "", "Razorpay", "+91 80 0000 0000", null, "https://www.razorpay.com" },
                    { 74L, "Seattle, WA, USA", "Remitly is an American fintech company that offers a digital remittance platform, enabling individuals to send money internationally through a mobile app or website.", "service@remitly.com", 1800, 2011, "Seattle, USA", "", "Remitly", "+1 888-736-4859", null, "https://www.remitly.com" },
                    { 75L, "San Francisco, CA, USA", "Revel Systems is an American company that provides an iPad-based point-of-sale system for restaurants and retailers, integrating operations and payments.", "info@revelsystems.com", 500, 2010, "San Francisco, USA", "", "Revel Systems", "+1 415-744-1433", null, "https://www.revelsystems.com" },
                    { 76L, "Allentown, PA, USA", "Shift4 Payments is an American payments technology company that provides integrated payment processing and technology solutions to restaurants, hotels, and other businesses.", "sales@shift4.com", 2300, 1999, "Allentown, USA", "", "Shift4 Payments", "+1 888-276-2108", null, "https://www.shift4.com" },
                    { 77L, "Ottawa, Canada", "Shopify is a Canadian e-commerce company that provides an online platform for businesses, including payment processing and point-of-sale systems for in-person sales.", "support@shopify.com", 10000, 2006, "Ottawa, Canada", "", "Shopify", "+1 613-000-0000", null, "https://www.shopify.com" },
                    { 78L, "Istanbul, Turkey", "Sipay is a Turkish fintech company offering digital payment solutions, including a payment gateway, digital wallet, and installment payment services.", "info@sipay.com", 100, 2018, "Istanbul, Turkey", "", "Sipay", "+90 212 000 0000", null, "https://www.sipay.com" },
                    { 79L, "London, UK", "Skrill is a UK-based digital wallet and online payments company that allows users to make transfers and payments online (part of Paysafe Group).", "help@skrill.com", 500, 2001, "London, UK", "", "Skrill", "+44 20 3308 2520", null, "https://www.skrill.com" },
                    { 80L, "San Francisco, CA, USA", "Square (now Block, Inc.) is an American financial services and digital payments company that offers point-of-sale software and hardware, and mobile payment solutions for merchants.", "support@squareup.com", 6000, 2009, "San Francisco, USA", "", "Square", "+1 855-700-6000", null, "https://squareup.com" },
                    { 81L, "São Paulo, Brazil", "StoneCo is a Brazilian fintech company that provides payment processing, point-of-sale technology, and financial services to merchants in Brazil.", "contato@stone.com.br", 7000, 2012, "São Paulo, Brazil", "", "Stone (StoneCo)", "+55 11 0000 0000", null, "https://www.stone.com.br" },
                    { 82L, "San Francisco, CA, USA", "Stripe is an American payment processing platform that allows businesses to accept payments online and via mobile apps, providing a suite of APIs for developers.", "support@stripe.com", 7000, 2010, "San Francisco, USA", "", "Stripe", "+1 888-963-8955", null, "https://www.stripe.com" },
                    { 83L, "London, UK", "SumUp is a UK-based fintech company that provides mobile card readers and point-of-sale solutions, enabling small merchants to accept card payments anywhere.", "support@sumup.com", 3000, 2012, "London, UK", "", "SumUp", "+44 20 7666 1767", null, "https://www.sumup.com" },
                    { 84L, "Columbus, GA, USA", "TSYS (Total System Services) is an American payment processor that provides payment processing and card issuing services to banks and merchants worldwide.", "info@tsys.com", 11000, 1983, "Columbus, USA", "", "TSYS", "+1 844-663-8797", null, "https://www.tsys.com" },
                    { 85L, "Boston, MA, USA", "Toast is an American restaurant technology company that offers a cloud-based point-of-sale system specifically designed for restaurants, including integrated payment processing.", "info@toasttab.com", 3000, 2012, "Boston, USA", "", "Toast", "+1 617-682-0225", null, "https://www.toasttab.com" },
                    { 86L, "Stockholm, Sweden", "Trustly is a Swedish fintech company that enables online bank payments directly from consumer bank accounts, providing an alternative to card payments across Europe.", "support@trustly.com", 500, 2008, "Stockholm, Sweden", "", "Trustly", "+46 8 446 831 33", null, "https://www.trustly.com" },
                    { 87L, "Sydney, Australia", "Tyro is an Australian challenger bank specializing in merchant credit and EFTPOS services, providing payment processing and banking solutions for SMEs.", "customersupport@tyro.com", 600, 2003, "Sydney, Australia", "", "Tyro", "+61 2 8907 1700", null, "https://www.tyro.com" },
                    { 88L, "Shanghai, China", "UnionPay is a Chinese financial services corporation that operates the UnionPay card network, the largest card payment network in the world by transaction volume.", "service@unionpayintl.com", 5000, 2002, "Shanghai, China", "", "UnionPay", "+86 21 2026 5828", null, "https://www.unionpayintl.com" },
                    { 89L, "San Jose, CA, USA", "Verifone is an American company that provides secure electronic payment technologies, specializing in point-of-sale payment terminals and payment software.", "sales@verifone.com", 5000, 1981, "San Jose, USA", "", "Verifone", "+1 408-232-7800", null, "https://www.verifone.com" },
                    { 90L, "Foster City, CA, USA", "Visa Inc. is an American multinational financial services corporation known for its global electronic payments network, facilitating card transactions between consumers, merchants, and banks.", "askvisa@visa.com", 21500, 1958, "Foster City, USA", "", "Visa", "+1 650-432-3200", null, "https://www.visa.com" },
                    { 91L, "Shenzhen, China", "WeChat Pay is a Chinese mobile payment and digital wallet service integrated into the WeChat app (by Tencent), enabling users to make payments and money transfers via smartphone.", "support@wechat.com", 10000, 2013, "Shenzhen, China", "", "WeChat Pay", "+86 755 86013388", null, "https://www.wechat.com" },
                    { 92L, "Redwood City, CA, USA", "WePay is an American online payment service provider that offers integrated payments for platforms and marketplaces (acquired by JPMorgan Chase).", "support@wepay.com", 300, 2008, "Redwood City, USA", "", "WePay", "+1 855-469-3729", null, "https://www.wepay.com" },
                    { 93L, "Denver, CO, USA", "Western Union is an American financial services company specializing in international money transfers, allowing individuals to send and receive money across the globe.", "customer@westernunion.com", 11000, 1851, "Denver, USA", "", "Western Union", "+1 800-325-6000", null, "https://www.westernunion.com" },
                    { 94L, "Auckland, New Zealand", "Windcave (formerly Payment Express) is a New Zealand-based payment gateway that provides secure payment processing solutions for in-store, online, and unattended payments.", "support@windcave.com", 200, 1997, "Auckland, New Zealand", "", "Windcave", "+64 9 123 4567", null, "https://www.windcave.com" },
                    { 95L, "London, UK", "Wise (formerly TransferWise) is a British fintech company offering an online platform for international money transfers with low fees and transparent exchange rates.", "support@wise.com", 4000, 2011, "London, UK", "", "Wise", "+44 20 3695 0999", null, "https://www.wise.com" },
                    { 96L, "Paris, France", "Worldline is a French multinational payment and transactional services company that provides merchants, banks, and governments with e-payment services and point-of-sale technology.", "info@worldline.com", 20000, 1973, "Paris, France", "", "Worldline", "+33 1 0000 0000", null, "https://www.worldline.com" },
                    { 97L, "London, UK", "Worldpay is a global payment processing company (originally from the UK) that provides merchant acquiring and payment processing services for businesses worldwide.", "support@worldpay.com", 8000, 1989, "London, UK", "", "Worldpay", "+44 20 0000 0000", null, "https://www.worldpay.com" },
                    { 98L, "Jakarta, Indonesia", "Xendit is an Indonesian payment gateway startup that offers businesses in Southeast Asia a platform for online payments, disbursements, and e-wallet integrations.", "help@xendit.co", 700, 2015, "Jakarta, Indonesia", "", "Xendit", "+62 21 0000 0000", null, "https://www.xendit.co" },
                    { 99L, "Cape Town, South Africa", "Yoco is a South African fintech company that provides portable card readers and payment solutions for small businesses to accept card payments easily.", "hello@yoco.com", 200, 2013, "Cape Town, South Africa", "", "Yoco", "+27 21 000 0000", null, "https://www.yoco.com" },
                    { 100L, "Sydney, Australia", "Zip Co is an Australian buy now, pay later provider that offers point-of-sale credit and digital payment solutions, allowing consumers to pay in installments.", "info@zip.co", 1000, 2013, "Sydney, Australia", "", "Zip", "+61 2 8294 2345", null, "https://www.zip.co" }
                });

            migrationBuilder.InsertData(
                schema: "posonl",
                table: "PosServiceCategory",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 15001L, "PaymentMethodsDescription", "PaymentMethods" },
                    { 15002L, "HardwareSolutionsDescription", "HardwareSolutions" },
                    { 15003L, "BillingAndAccountingDescription", "BillingAndAccounting" },
                    { 15004L, "EcommerceAndOnlinePaymentsDescription", "EcommerceAndOnlinePayments" },
                    { 15005L, "SecurityAndFraudPreventionDescription", "SecurityAndFraudPrevention" },
                    { 15006L, "CustomerAndLoyaltyProgramsDescription", "CustomerAndLoyaltyPrograms" },
                    { 15007L, "MultiCurrencyAndCrossBorderPaymentsDescription", "MultiCurrencyAndCrossBorderPayments" },
                    { 15008L, "ReportingAndAnalyticsDescription", "ReportingAndAnalytics" },
                    { 15009L, "SubscriptionAndRecurringPaymentsDescription", "SubscriptionAndRecurringPayments" },
                    { 15010L, "RegionalSpecificPOSServicesDescription", "RegionalSpecificPOSServices" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Country_CountryGroupId",
                schema: "posonl",
                table: "Country",
                column: "CountryGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_CountryGroupPosService_SupportedCountryGroupsId",
                table: "CountryGroupPosService",
                column: "SupportedCountryGroupsId");

            migrationBuilder.CreateIndex(
                name: "IX_CountryPosCompany_SupportedCountriesId",
                table: "CountryPosCompany",
                column: "SupportedCountriesId");

            migrationBuilder.CreateIndex(
                name: "IX_HomePageLocalizations_CultureId",
                schema: "posonl",
                table: "HomePageLocalizations",
                column: "CultureId");

            migrationBuilder.CreateIndex(
                name: "IX_HomePageLocalizations_HomePageId",
                schema: "posonl",
                table: "HomePageLocalizations",
                column: "HomePageId");

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
                name: "IX_PosCompanyLocalization_CultureId",
                table: "PosCompanyLocalization",
                column: "CultureId");

            migrationBuilder.CreateIndex(
                name: "IX_PosCompanyLocalization_PosCompanyId",
                table: "PosCompanyLocalization",
                column: "PosCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PosCompanyPosService_PosServicesId",
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

            migrationBuilder.CreateIndex(
                name: "IX_PosServiceCategoryLocalization_CultureId",
                table: "PosServiceCategoryLocalization",
                column: "CultureId");

            migrationBuilder.CreateIndex(
                name: "IX_PosServiceCategoryLocalization_PosServiceCategoryId",
                table: "PosServiceCategoryLocalization",
                column: "PosServiceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PosServiceLocalization_CultureId",
                table: "PosServiceLocalization",
                column: "CultureId");

            migrationBuilder.CreateIndex(
                name: "IX_PosServiceLocalization_PosServiceId",
                table: "PosServiceLocalization",
                column: "PosServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PosServices_CountryId",
                table: "PosServices",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_PosServices_PosServiceCategoryId",
                table: "PosServices",
                column: "PosServiceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RatingCategory_CultureId",
                schema: "posonl",
                table: "RatingCategory",
                column: "CultureId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CountryGroupPosService");

            migrationBuilder.DropTable(
                name: "CountryPosCompany");

            migrationBuilder.DropTable(
                name: "HomePageLocalizations",
                schema: "posonl");

            migrationBuilder.DropTable(
                name: "PosCommissionRate",
                schema: "posonl");

            migrationBuilder.DropTable(
                name: "PosCompanyLocalization");

            migrationBuilder.DropTable(
                name: "PosCompanyPosService");

            migrationBuilder.DropTable(
                name: "PosCompanyRating",
                schema: "posonl");

            migrationBuilder.DropTable(
                name: "PosServiceCategoryLocalization");

            migrationBuilder.DropTable(
                name: "PosServiceLocalization");

            migrationBuilder.DropTable(
                name: "HomePages",
                schema: "posonl");

            migrationBuilder.DropTable(
                name: "PosCompanies");

            migrationBuilder.DropTable(
                name: "RatingCategory",
                schema: "posonl");

            migrationBuilder.DropTable(
                name: "PosServices");

            migrationBuilder.DropTable(
                name: "Cultures",
                schema: "posonl");

            migrationBuilder.DropTable(
                name: "Country",
                schema: "posonl");

            migrationBuilder.DropTable(
                name: "PosServiceCategory",
                schema: "posonl");

            migrationBuilder.DropTable(
                name: "CountryGroup",
                schema: "posonl");
        }
    }
}
