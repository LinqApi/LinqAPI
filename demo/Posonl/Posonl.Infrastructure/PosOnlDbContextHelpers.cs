using Microsoft.EntityFrameworkCore;
using Posonl.Domain;

namespace Posonl.Infrastructure
{
    public static class PosOnlDbContextHelpers
    {

        public static Dictionary<string, (string Name, string Description, string Category, bool IsRegional)> GetPosServiceData() =>
            // Bu mapping, resource dosyanızdaki "PosService.[Key]" formatındaki key'ler ile eşleşir.
            // Name değeri resource key'i (örneğin "ContactlessPayments") ve
            // Description değeri ise karşılık gelen resource key'i (örneğin "ContactlessPaymentsDescription") olarak tanımlanmıştır.
            // Localizer bu key'leri ilgili resx değerlerine çevirir.
            new Dictionary<string, (string, string, string, bool)>
    {
        // Payment Methods
        { "CreditCardProcessing", ("CreditCardProcessing", "CreditCardProcessingDescription", "PaymentMethods", false) },
        { "ContactlessPayments",   ("ContactlessPayments",   "ContactlessPaymentsDescription",   "PaymentMethods", false) },
        { "EMVChipPayments",        ("EMVChipPayments",        "EMVChipPaymentsDescription",        "PaymentMethods", false) },
        { "MobilePayments",         ("MobilePayments",         "MobilePaymentsDescription",         "PaymentMethods", false) },
        { "CryptoPayments",         ("CryptoPayments",         "CryptoPaymentsDescription",         "PaymentMethods", false) },
        { "BKMExpressPayment",      ("BKMExpressPayment",      "BKMExpressPaymentDescription",      "PaymentMethods", true) },
        { "UPIPaymentsIndia",       ("UPIPaymentsIndia",       "UPIPaymentsIndiaDescription",       "PaymentMethods", true) },
        { "WeChatPayChina",         ("WeChatPayChina",         "WeChatPayChinaDescription",         "PaymentMethods", true) },
        { "AliPayChina",            ("AliPayChina",            "AliPayChinaDescription",            "PaymentMethods", true) },
        { "iDealNetherlands",       ("iDealNetherlands",       "iDealNetherlandsDescription",       "PaymentMethods", true) },
        { "PayNowSingapore",        ("PayNowSingapore",        "PayNowSingaporeDescription",        "PaymentMethods", true) },

        // Hardware Solutions
        { "TraditionalPOSMachines", ("TraditionalPOSMachines", "TraditionalPOSMachinesDescription", "HardwareSolutions", false) },
        { "WirelessPOS",            ("WirelessPOS",            "WirelessPOSDescription",            "HardwareSolutions", false) },
        { "TabletPOS",              ("TabletPOS",              "TabletPOSDescription",              "HardwareSolutions", false) },
        { "SelfCheckoutKiosks",     ("SelfCheckoutKiosks",     "SelfCheckoutKiosksDescription",     "HardwareSolutions", false) },
        { "HandheldPOS",            ("HandheldPOS",            "HandheldPOSDescription",            "HardwareSolutions", false) },
        { "SquarePOS",              ("SquarePOS",              "SquarePOSDescription",              "HardwareSolutions", false) },
        { "ToastPOS",               ("ToastPOS",               "ToastPOSDescription",               "HardwareSolutions", false) },
        { "POSDeviceHealthMonitoring", ("POSDeviceHealthMonitoring", "POSDeviceHealthMonitoringDescription", "HardwareSolutions", false) },

        // Billing and Accounting
        { "AutomatedInvoicing",     ("AutomatedInvoicing",     "AutomatedInvoicingDescription",     "BillingAndAccounting", false) },
        { "TaxIntegration",         ("TaxIntegration",         "TaxIntegrationDescription",         "BillingAndAccounting", false) },
        { "MultiCurrencyBilling",   ("MultiCurrencyBilling",   "MultiCurrencyBillingDescription",   "BillingAndAccounting", false) },
        { "EReceiptGeneration",     ("EReceiptGeneration",     "EReceiptGenerationDescription",     "BillingAndAccounting", false) },
        { "POSAccountingIntegration", ("POSAccountingIntegration", "POSAccountingIntegrationDescription", "BillingAndAccounting", false) },
        { "DunningManagement",      ("DunningManagement",      "DunningManagementDescription",      "BillingAndAccounting", false) },
        { "DeferredPaymentProcessing", ("DeferredPaymentProcessing", "DeferredPaymentProcessingDescription", "BillingAndAccounting", false) },

        // Ecommerce and Online Payments
        { "PaymentLinks",           ("PaymentLinks",           "PaymentLinksDescription",           "EcommerceAndOnlinePayments", false) },
        { "MerchantVirtualTerminals", ("MerchantVirtualTerminals", "MerchantVirtualTerminalsDescription", "EcommerceAndOnlinePayments", false) },
        { "CheckoutOptimizations",  ("CheckoutOptimizations",  "CheckoutOptimizationsDescription",  "EcommerceAndOnlinePayments", false) },

        // Subscription and Recurring Payments
        { "SubscriptionBilling",    ("SubscriptionBilling",    "SubscriptionBillingDescription",    "SubscriptionAndRecurringPayments", false) },
        { "AutomatedSubscriptionBilling", ("AutomatedSubscriptionBilling", "AutomatedSubscriptionBillingDescription", "SubscriptionAndRecurringPayments", false) },
        { "RecurringPaymentManagement", ("RecurringPaymentManagement", "RecurringPaymentManagementDescription", "SubscriptionAndRecurringPayments", false) },
        { "UsageBasedBilling",      ("UsageBasedBilling",      "UsageBasedBillingDescription",      "SubscriptionAndRecurringPayments", false) },

        // Security and Fraud Prevention
        { "TwoFactorAuthentication", ("TwoFactorAuthentication", "TwoFactorAuthenticationDescription", "SecurityAndFraudPrevention", false) },
        { "AITransactionMonitoring", ("AITransactionMonitoring", "AITransactionMonitoringDescription", "SecurityAndFraudPrevention", false) },
        { "ChargebackProtection",   ("ChargebackProtection",   "ChargebackProtectionDescription",   "SecurityAndFraudPrevention", false) },
        { "DeviceFingerprinting",   ("DeviceFingerprinting",   "DeviceFingerprintingDescription",   "SecurityAndFraudPrevention", false) },
        { "RiskScoring",            ("RiskScoring",            "RiskScoringDescription",            "SecurityAndFraudPrevention", false) },

        // Customer and Loyalty Programs
        { "GiftCardManagement",     ("GiftCardManagement",     "GiftCardManagementDescription",     "CustomerAndLoyaltyPrograms", false) },
        { "LoyaltyRewards",         ("LoyaltyRewards",         "LoyaltyRewardsDescription",         "CustomerAndLoyaltyPrograms", false) },
        { "PointRedemption",        ("PointRedemption",        "PointRedemptionDescription",        "CustomerAndLoyaltyPrograms", false) },
        { "CashbackOffers",         ("CashbackOffers",         "CashbackOffersDescription",         "CustomerAndLoyaltyPrograms", false) },

        // Multi-Currency and Cross-Border Payments
        { "DynamicCurrencyConversion", ("DynamicCurrencyConversion", "DynamicCurrencyConversionDescription", "MultiCurrencyAndCrossBorderPayments", false) },
        { "InternationalWireTransfers", ("InternationalWireTransfers", "InternationalWireTransfersDescription", "MultiCurrencyAndCrossBorderPayments", false) },
        { "MultiCurrencyPOSBilling", ("MultiCurrencyPOSBilling", "MultiCurrencyPOSBillingDescription", "MultiCurrencyAndCrossBorderPayments", false) },
        { "ForexRateLocking",       ("ForexRateLocking",       "ForexRateLockingDescription",       "MultiCurrencyAndCrossBorderPayments", false) },
        { "CrossBorderTransactionSecurity", ("CrossBorderTransactionSecurity", "CrossBorderTransactionSecurityDescription", "MultiCurrencyAndCrossBorderPayments", false) },

        // Reporting and Analytics
        { "CustomerDataAnalytics",  ("CustomerDataAnalytics",  "CustomerDataAnalyticsDescription",  "ReportingAndAnalytics", false) },
        { "SalesAnalytics",         ("SalesAnalytics",         "SalesAnalyticsDescription",         "ReportingAndAnalytics", false) },
        { "CustomerBehaviorReports", ("CustomerBehaviorReports", "CustomerBehaviorReportsDescription", "ReportingAndAnalytics", false) },
        { "InventoryTracking",      ("InventoryTracking",      "InventoryTrackingDescription",      "ReportingAndAnalytics", false) },
        { "RevenueForecasting",     ("RevenueForecasting",     "RevenueForecastingDescription",     "ReportingAndAnalytics", false) },

        // Other Services
        { "BuyNowPayLater",         ("BuyNowPayLater",         "BuyNowPayLaterDescription",         "Other", false) },
        { "SplitPayments",          ("SplitPayments",          "SplitPaymentsDescription",          "Other", false) },
        { "ChaseMerchantServices",  ("ChaseMerchantServices",  "ChaseMerchantServicesDescription",  "Other", false) }
    };


        public static List<Country> GetSeedCountries()
        {
            return new List<Country>
        {
            new Country { Id = 1, Code = "tr", Name = "Türkiye", Currency = "TRY", LanguageCode = "tr-TR" },
            new Country { Id = 2, Code = "us", Name = "United States", Currency = "USD", LanguageCode = "en-US" },
            new Country { Id = 3, Code = "de", Name = "Germany", Currency = "EUR", LanguageCode = "de-DE" },
            new Country { Id = 4, Code = "nl", Name = "Netherlands", Currency = "EUR", LanguageCode = "nl-NL" },
            new Country { Id = 5, Code = "cn", Name = "China", Currency = "CNY", LanguageCode = "zh-CN" },
            new Country { Id = 6, Code = "jp", Name = "Japan", Currency = "JPY", LanguageCode = "ja-JP" },
            new Country { Id = 7, Code = "in", Name = "India", Currency = "INR", LanguageCode = "hi-IN" },
            new Country { Id = 8, Code = "uk", Name = "United Kingdom", Currency = "GBP", LanguageCode = "en-GB" },
            new Country { Id = 9, Code = "ru", Name = "Russia", Currency = "RUB", LanguageCode = "ru-RU" },
            new Country { Id = 10, Code = "br", Name = "Brazil", Currency = "BRL", LanguageCode = "pt-BR" }
        };
        }

        public static List<PosServiceCategory> GetSeedPosServiceCategories()
        {
            var categories = new List<PosServiceCategory>
            {
                new PosServiceCategory {Title = "PaymentMethods",Slug = "PaymentMethods",MetaDescription = "",CultureId = 1, Id = 15001, Name = "PaymentMethods", Description = "PaymentMethodsDescription" },
                new PosServiceCategory {Title = "HardwareSolutions",Slug = "HardwareSolutions",MetaDescription = "",CultureId = 1, Id = 15002, Name = "HardwareSolutions", Description = "HardwareSolutionsDescription" },
                new PosServiceCategory {Title = "BillingAndAccounting",Slug = "BillingAndAccounting",MetaDescription = "",CultureId = 1, Id = 15003, Name = "BillingAndAccounting", Description = "BillingAndAccountingDescription" },
                new PosServiceCategory {Title = "EcommerceAndOnlinePayments",Slug = "EcommerceAndOnlinePayments",MetaDescription = "",CultureId = 1, Id = 15004, Name = "EcommerceAndOnlinePayments", Description = "EcommerceAndOnlinePaymentsDescription" },
                new PosServiceCategory {Title = "SecurityAndFraudPrevention",Slug = "SecurityAndFraudPrevention",MetaDescription = "",CultureId = 1, Id = 15005, Name = "SecurityAndFraudPrevention", Description = "SecurityAndFraudPreventionDescription" },
                new PosServiceCategory {Title = "CustomerAndLoyaltyPrograms",Slug = "CustomerAndLoyaltyPrograms",MetaDescription = "",CultureId = 1, Id = 15006, Name = "CustomerAndLoyaltyPrograms", Description = "CustomerAndLoyaltyProgramsDescription" },
                new PosServiceCategory {Title = "MultiCurrencyAndCrossBorderPayments",Slug = "MultiCurrencyAndCrossBorderPayments",MetaDescription = "",CultureId = 1, Id = 15007, Name = "MultiCurrencyAndCrossBorderPayments", Description = "MultiCurrencyAndCrossBorderPaymentsDescription" },
                new PosServiceCategory {Title = "ReportingAndAnalytics",Slug = "ReportingAndAnalytics",MetaDescription = "",CultureId = 1, Id = 15008, Name = "ReportingAndAnalytics", Description = "ReportingAndAnalyticsDescription" },
                new PosServiceCategory {Title = "SubscriptionAndRecurringPayments",Slug = "SubscriptionAndRecurringPayments",MetaDescription = "",CultureId = 1, Id = 15009, Name = "SubscriptionAndRecurringPayments", Description = "SubscriptionAndRecurringPaymentsDescription" },
                new PosServiceCategory {Title = "RegionalSpecificPOSServices",Slug = "RegionalSpecificPOSServices",MetaDescription = "",CultureId = 1, Id = 15010, Name = "RegionalSpecificPOSServices", Description = "RegionalSpecificPOSServicesDescription" }
            };
            return categories;
        }

        public static List<PosCompany> SeedPosCompanies(ModelBuilder builder)
        {

            long _id = 0;
            List<PosCompany> posCompanies = new List<PosCompany>
{
    new PosCompany
    {
        Id = ++_id,
        Name = "2C2P",
        LogoUrl = "",
        Description = "2C2P is a Singapore-based payment platform providing payment processing solutions across Asia.",
        Website = "https://www.2c2p.com",
        Headquarters = "Singapore, Singapore",
        EmployeeCount = 300,
        FoundedYear = 2003,
        PhoneNumber = "+65 3123 4567",
        Email = "contact@2c2p.com",
        Address = "Singapore, Singapore"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Adyen",
        LogoUrl = "",
        Description = "Adyen is a Dutch payments company that offers a global omnichannel payment platform for businesses.",
        Website = "https://www.adyen.com",
        Headquarters = "Amsterdam, Netherlands",
        EmployeeCount = 4000,
        FoundedYear = 2006,
        PhoneNumber = "+31 20 240 1240",
        Email = "info@adyen.com",
        Address = "Amsterdam, Netherlands"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Affirm",
        LogoUrl = "",
        Description = "Affirm is an American fintech company that provides buy now, pay later financing for online consumers.",
        Website = "https://www.affirm.com",
        Headquarters = "San Francisco, USA",
        EmployeeCount = 2000,
        FoundedYear = 2012,
        PhoneNumber = "+1 415-123-4567",
        Email = "support@affirm.com",
        Address = "San Francisco, CA, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Afterpay",
        LogoUrl = "",
        Description = "Afterpay is an Australian fintech known for its buy now, pay later payment service, allowing shoppers to pay in installments.",
        Website = "https://www.afterpay.com",
        Headquarters = "Melbourne, Australia",
        EmployeeCount = 1000,
        FoundedYear = 2014,
        PhoneNumber = "+61 3 9100 0000",
        Email = "info@afterpay.com",
        Address = "Melbourne, Australia"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Alipay",
        LogoUrl = "",
        Description = "Alipay is a Chinese online and mobile payment platform launched by Ant Group, widely used for digital payments in China.",
        Website = "https://www.alipay.com",
        Headquarters = "Hangzhou, China",
        EmployeeCount = 10000,
        FoundedYear = 2004,
        PhoneNumber = "+86 571 2688 8888",
        Email = "support@alipay.com",
        Address = "Hangzhou, China"
    },
    new PosCompany
    {Id = ++_id,
        Name = "American Express",
        LogoUrl = "",
        Description = "American Express (Amex) is an American multinational financial services corporation best known for its credit card, charge card, and traveler's cheque businesses.",
        Website = "https://www.americanexpress.com",
        Headquarters = "New York, USA",
        EmployeeCount = 64000,
        FoundedYear = 1850,
        PhoneNumber = "+1 212-640-2000",
        Email = "info@americanexpress.com",
        Address = "New York, NY, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Authorize.Net",
        LogoUrl = "",
        Description = "Authorize.Net is an American payment gateway service that enables merchants to accept credit card and electronic payments online.",
        Website = "https://www.authorize.net",
        Headquarters = "Foster City, USA",
        EmployeeCount = 500,
        FoundedYear = 1996,
        PhoneNumber = "+1 888-323-4289",
        Email = "support@authorize.net",
        Address = "Foster City, CA, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "BillDesk",
        LogoUrl = "",
        Description = "BillDesk is an Indian online payment gateway platform used for bill payments and other online transactions.",
        Website = "https://www.billdesk.com",
        Headquarters = "Mumbai, India",
        EmployeeCount = 800,
        FoundedYear = 2000,
        PhoneNumber = "+91 22 7131 0000",
        Email = "support@billdesk.com",
        Address = "Mumbai, India"
    },
    new PosCompany
    {Id = ++_id,
        Name = "BitPay",
        LogoUrl = "",
        Description = "BitPay is an American cryptocurrency payment processor that allows merchants to accept Bitcoin and other crypto payments.",
        Website = "https://www.bitpay.com",
        Headquarters = "Atlanta, USA",
        EmployeeCount = 100,
        FoundedYear = 2011,
        PhoneNumber = "+1 404-890-7700",
        Email = "support@bitpay.com",
        Address = "Atlanta, GA, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "BlueSnap",
        LogoUrl = "",
        Description = "BlueSnap is a US-based payment gateway that provides an all-in-one payment platform for businesses to accept payments globally.",
        Website = "https://www.bluesnap.com",
        Headquarters = "Waltham, USA",
        EmployeeCount = 300,
        FoundedYear = 2001,
        PhoneNumber = "+1 866-312-7733",
        Email = "info@bluesnap.com",
        Address = "Waltham, MA, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Braintree",
        LogoUrl = "",
        Description = "Braintree is an American payments company that specializes in mobile and web payment systems for e-commerce (now a PayPal subsidiary).",
        Website = "https://www.braintreepayments.com",
        Headquarters = "Chicago, USA",
        EmployeeCount = 500,
        FoundedYear = 2007,
        PhoneNumber = "+1 877-434-2894",
        Email = "support@braintreepayments.com",
        Address = "Chicago, IL, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "CCAvenue",
        LogoUrl = "",
        Description = "CCAvenue is an Indian payment gateway platform that offers online payment processing for e-commerce businesses in India.",
        Website = "https://www.ccavenue.com",
        Headquarters = "Mumbai, India",
        EmployeeCount = 300,
        FoundedYear = 2001,
        PhoneNumber = "+91 22 6742 5555",
        Email = "support@ccavenue.com",
        Address = "Mumbai, India"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Chase Paymentech",
        LogoUrl = "",
        Description = "Chase Paymentech is an American payment processing and merchant acquiring service, part of JPMorgan Chase, serving businesses with payment solutions.",
        Website = "https://www.chasepaymentech.com",
        Headquarters = "Dallas, USA",
        EmployeeCount = 1000,
        FoundedYear = 1985,
        PhoneNumber = "+1 800-934-7717",
        Email = "support@chasepaymentech.com",
        Address = "Dallas, TX, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Checkout.com",
        LogoUrl = "",
        Description = "Checkout.com is a UK-based payment processor that provides online payment solutions for businesses worldwide.",
        Website = "https://www.checkout.com",
        Headquarters = "London, UK",
        EmployeeCount = 1700,
        FoundedYear = 2012,
        PhoneNumber = "+44 20 8068 0000",
        Email = "info@checkout.com",
        Address = "London, UK"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Chipper Cash",
        LogoUrl = "",
        Description = "Chipper Cash is a fintech company that offers a cross-border peer-to-peer payment service across Africa via a mobile-based platform.",
        Website = "https://www.chippercash.com",
        Headquarters = "San Francisco, USA",
        EmployeeCount = 300,
        FoundedYear = 2018,
        PhoneNumber = "+1 415-000-0000",
        Email = "support@chippercash.com",
        Address = "San Francisco, CA, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Cielo",
        LogoUrl = "",
        Description = "Cielo is a Brazilian payment processing company and the largest credit and debit card payment operator in Latin America.",
        Website = "https://www.cielo.com.br",
        Headquarters = "Barueri, Brazil",
        EmployeeCount = 5000,
        FoundedYear = 1995,
        PhoneNumber = "+55 11 3165-2000",
        Email = "atendimento@cielo.com.br",
        Address = "Barueri, São Paulo, Brazil"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Clover",
        LogoUrl = "",
        Description = "Clover is an American point-of-sale platform offering smart payment terminals and POS software for small and medium businesses (a Fiserv company).",
        Website = "https://www.clover.com",
        Headquarters = "Sunnyvale, USA",
        EmployeeCount = 500,
        FoundedYear = 2010,
        PhoneNumber = "+1 408-123-4567",
        Email = "sales@clover.com",
        Address = "Sunnyvale, CA, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "CyberSource",
        LogoUrl = "",
        Description = "CyberSource is an American payment management platform that provides credit card processing, fraud management, and payment security services (owned by Visa).",
        Website = "https://www.cybersource.com",
        Headquarters = "Foster City, USA",
        EmployeeCount = 1000,
        FoundedYear = 1994,
        PhoneNumber = "+1 888-330-2300",
        Email = "support@cybersource.com",
        Address = "Foster City, CA, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "DLocal",
        LogoUrl = "",
        Description = "DLocal is a Uruguayan fintech company that specializes in cross-border payment processing for emerging markets.",
        Website = "https://www.dlocal.com",
        Headquarters = "Montevideo, Uruguay",
        EmployeeCount = 500,
        FoundedYear = 2016,
        PhoneNumber = "+598 000 0000",
        Email = "info@dlocal.com",
        Address = "Montevideo, Uruguay"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Diebold Nixdorf",
        LogoUrl = "",
        Description = "Diebold Nixdorf is an American multinational that provides ATMs, point-of-sale terminals, and integrated services for banking and retail.",
        Website = "https://www.dieboldnixdorf.com",
        Headquarters = "North Canton, USA",
        EmployeeCount = 22000,
        FoundedYear = 1859,
        PhoneNumber = "+1 330-490-4000",
        Email = "info@dieboldnixdorf.com",
        Address = "North Canton, OH, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Discover",
        LogoUrl = "",
        Description = "Discover is an American financial services company that operates Discover Card and network, providing credit card services and payment network solutions.",
        Website = "https://www.discover.com",
        Headquarters = "Riverwoods, USA",
        EmployeeCount = 17000,
        FoundedYear = 1985,
        PhoneNumber = "+1 800-347-2683",
        Email = "support@discover.com",
        Address = "Riverwoods, IL, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Dwolla",
        LogoUrl = "",
        Description = "Dwolla is an American fintech company that offers an API platform for bank transfers and ACH payments in the United States.",
        Website = "https://www.dwolla.com",
        Headquarters = "Des Moines, USA",
        EmployeeCount = 200,
        FoundedYear = 2008,
        PhoneNumber = "+1 515-000-0000",
        Email = "support@dwolla.com",
        Address = "Des Moines, IA, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "EBANX",
        LogoUrl = "",
        Description = "EBANX is a Brazilian fintech company providing local payment solutions in Latin America, enabling global merchants to accept regional payment methods.",
        Website = "https://www.ebanx.com",
        Headquarters = "Curitiba, Brazil",
        EmployeeCount = 1000,
        FoundedYear = 2012,
        PhoneNumber = "+55 41 0000-0000",
        Email = "business@ebanx.com",
        Address = "Curitiba, Brazil"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Elavon",
        LogoUrl = "",
        Description = "Elavon is an American payment processing company (a subsidiary of U.S. Bancorp) that provides merchant acquiring services globally.",
        Website = "https://www.elavon.com",
        Headquarters = "Atlanta, USA",
        EmployeeCount = 4000,
        FoundedYear = 1991,
        PhoneNumber = "+1 800-725-1243",
        Email = "customer@elavon.com",
        Address = "Atlanta, GA, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "EVO Payments",
        LogoUrl = "",
        Description = "EVO Payments is an American payment services provider offering merchant acquiring and payment processing solutions to merchants across the world.",
        Website = "https://www.evopayments.com",
        Headquarters = "Atlanta, USA",
        EmployeeCount = 2400,
        FoundedYear = 1989,
        PhoneNumber = "+1 770-709-3000",
        Email = "info@evopayments.com",
        Address = "Atlanta, GA, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Eway",
        LogoUrl = "",
        Description = "Eway is an Australian online payment gateway that processes secure credit card payments for merchants, offering omnichannel payment solutions.",
        Website = "https://www.eway.com.au",
        Headquarters = "Sydney, Australia",
        EmployeeCount = 100,
        FoundedYear = 1998,
        PhoneNumber = "+61 2 0000 0000",
        Email = "support@eway.com.au",
        Address = "Sydney, Australia"
    },
    new PosCompany
    {Id = ++_id,
        Name = "FIS",
        LogoUrl = "",
        Description = "FIS (Fidelity National Information Services) is an American provider of financial technology solutions for merchants, banks, and capital markets, including payment processing services.",
        Website = "https://www.fisglobal.com",
        Headquarters = "Jacksonville, USA",
        EmployeeCount = 50000,
        FoundedYear = 1968,
        PhoneNumber = "+1 904-438-6000",
        Email = "info@fisglobal.com",
        Address = "Jacksonville, FL, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Fawry",
        LogoUrl = "",
        Description = "Fawry is an Egyptian electronic payments network offering a variety of financial services, including bill payments and mobile wallet services.",
        Website = "https://www.fawry.com",
        Headquarters = "Cairo, Egypt",
        EmployeeCount = 1000,
        FoundedYear = 2008,
        PhoneNumber = "+20 2 0000 0000",
        Email = "support@fawry.com",
        Address = "Cairo, Egypt"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Fiserv",
        LogoUrl = "",
        Description = "Fiserv is an American global fintech and payments company that provides payment processing, financial technology, and merchant services (merged with First Data).",
        Website = "https://www.fiserv.com",
        Headquarters = "Brookfield, USA",
        EmployeeCount = 44000,
        FoundedYear = 1984,
        PhoneNumber = "+1 262-879-5000",
        Email = "info@fiserv.com",
        Address = "Brookfield, WI, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Flutterwave",
        LogoUrl = "",
        Description = "Flutterwave is a payments technology company founded in Nigeria (with HQ in the US) that enables businesses in Africa to accept and process payments across the globe.",
        Website = "https://www.flutterwave.com",
        Headquarters = "San Francisco, USA",
        EmployeeCount = 400,
        FoundedYear = 2016,
        PhoneNumber = "+1 415-000-0000",
        Email = "support@flutterwave.com",
        Address = "San Francisco, CA, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "GCash",
        LogoUrl = "",
        Description = "GCash is a Philippine mobile wallet and digital payment service that allows users to pay bills, transfer money, and make purchases via their mobile phones.",
        Website = "https://www.gcash.com",
        Headquarters = "Taguig, Philippines",
        EmployeeCount = 800,
        FoundedYear = 2004,
        PhoneNumber = "+63 2 0000 0000",
        Email = "support@gcash.com",
        Address = "Taguig, Philippines"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Global Payments",
        LogoUrl = "",
        Description = "Global Payments Inc. is an American payment technology company offering payment processing and software solutions for merchants, issuers, and consumers worldwide.",
        Website = "https://www.globalpayments.com",
        Headquarters = "Atlanta, USA",
        EmployeeCount = 27000,
        FoundedYear = 2000,
        PhoneNumber = "+1 770-829-8000",
        Email = "info@globalpay.com",
        Address = "Atlanta, GA, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "GoCardless",
        LogoUrl = "",
        Description = "GoCardless is a British fintech company that specializes in recurring payments and direct debit solutions for businesses.",
        Website = "https://www.gocardless.com",
        Headquarters = "London, UK",
        EmployeeCount = 500,
        FoundedYear = 2011,
        PhoneNumber = "+44 20 7183 8674",
        Email = "help@gocardless.com",
        Address = "London, UK"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Heartland Payment Systems",
        LogoUrl = "",
        Description = "Heartland Payment Systems is an American payment processing company that provides merchant services and payment technology solutions (now part of Global Payments).",
        Website = "https://www.heartland.us",
        Headquarters = "Princeton, USA",
        EmployeeCount = 4000,
        FoundedYear = 1997,
        PhoneNumber = "+1 888-798-3131",
        Email = "support@heartland.us",
        Address = "Princeton, NJ, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Ingenico",
        LogoUrl = "",
        Description = "Ingenico is a French payment technology company known for its point-of-sale payment terminals and secure electronic transaction solutions.",
        Website = "https://www.ingenico.com",
        Headquarters = "Paris, France",
        EmployeeCount = 8000,
        FoundedYear = 1980,
        PhoneNumber = "+33 1 00000000",
        Email = "sales@ingenico.com",
        Address = "Paris, France"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Ininal",
        LogoUrl = "",
        Description = "Ininal is a Turkish fintech company offering a prepaid card and digital wallet platform, enabling users to make cashless payments.",
        Website = "https://www.ininal.com",
        Headquarters = "Istanbul, Turkey",
        EmployeeCount = 150,
        FoundedYear = 2012,
        PhoneNumber = "+90 212 000 0000",
        Email = "info@ininal.com",
        Address = "Istanbul, Turkey"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Interswitch",
        LogoUrl = "",
        Description = "Interswitch is a Nigerian digital payment processing company providing integrated payment and transaction switching services across Africa.",
        Website = "https://www.interswitchgroup.com",
        Headquarters = "Lagos, Nigeria",
        EmployeeCount = 1000,
        FoundedYear = 2002,
        PhoneNumber = "+234 1 000 0000",
        Email = "info@interswitchng.com",
        Address = "Lagos, Nigeria"
    },
    new PosCompany
    {Id = ++_id,
        Name = "iyzico",
        LogoUrl = "",
        Description = "iyzico is a Turkish fintech company that provides a secure payment gateway and digital wallet solutions for online businesses and marketplaces.",
        Website = "https://www.iyzico.com",
        Headquarters = "Istanbul, Turkey",
        EmployeeCount = 350,
        FoundedYear = 2013,
        PhoneNumber = "+90 212 909 6972",
        Email = "destek@iyzico.com",
        Address = "Istanbul, Turkey"
    },
    new PosCompany
    {Id = ++_id,
        Name = "iZettle",
        LogoUrl = "",
        Description = "iZettle is a Swedish fintech company that provides mobile point-of-sale solutions and card readers for small businesses (acquired by PayPal, now operating as Zettle).",
        Website = "https://www.izettle.com",
        Headquarters = "Stockholm, Sweden",
        EmployeeCount = 600,
        FoundedYear = 2010,
        PhoneNumber = "+46 8 0000 0000",
        Email = "support@izettle.com",
        Address = "Stockholm, Sweden"
    },
    new PosCompany
    {Id = ++_id,
        Name = "JCB",
        LogoUrl = "",
        Description = "JCB (Japan Credit Bureau) is a Japanese credit card company and payment network offering credit card services accepted worldwide.",
        Website = "https://www.global.jcb/en",
        Headquarters = "Tokyo, Japan",
        EmployeeCount = 4400,
        FoundedYear = 1961,
        PhoneNumber = "+81 3-5778-5400",
        Email = "info@jcb.co.jp",
        Address = "Tokyo, Japan"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Klarna",
        LogoUrl = "",
        Description = "Klarna is a Swedish fintech company offering buy now, pay later payment solutions and an online payments platform for shoppers and merchants.",
        Website = "https://www.klarna.com",
        Headquarters = "Stockholm, Sweden",
        EmployeeCount = 5000,
        FoundedYear = 2005,
        PhoneNumber = "+46 8 120 120 00",
        Email = "customer@klarna.com",
        Address = "Stockholm, Sweden"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Lightspeed",
        LogoUrl = "",
        Description = "Lightspeed is a Canadian point-of-sale and e-commerce software provider that offers cloud-based POS solutions for retail and hospitality businesses.",
        Website = "https://www.lightspeedhq.com",
        Headquarters = "Montreal, Canada",
        EmployeeCount = 3000,
        FoundedYear = 2005,
        PhoneNumber = "+1 514-907-1801",
        Email = "info@lightspeedhq.com",
        Address = "Montreal, Canada"
    },
    new PosCompany
    {Id = ++_id,
        Name = "M-Pesa",
        LogoUrl = "",
        Description = "M-Pesa is a Kenyan mobile money service operated by Safaricom and Vodacom that allows users to store and transfer money and pay for goods via their mobile phones.",
        Website = "https://www.safaricom.co.ke/personal/m-pesa",
        Headquarters = "Nairobi, Kenya",
        EmployeeCount = 500,
        FoundedYear = 2007,
        PhoneNumber = "+254 722 000000",
        Email = "info@safaricom.co.ke",
        Address = "Nairobi, Kenya"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Marqeta",
        LogoUrl = "",
        Description = "Marqeta is an American fintech company that provides a modern card issuing platform for businesses to create and manage payment cards via APIs.",
        Website = "https://www.marqeta.com",
        Headquarters = "Oakland, USA",
        EmployeeCount = 800,
        FoundedYear = 2010,
        PhoneNumber = "+1 510-281-3700",
        Email = "sales@marqeta.com",
        Address = "Oakland, CA, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Mastercard",
        LogoUrl = "",
        Description = "Mastercard is an American multinational financial services corporation that operates a global payment network and offers credit, debit, and prepaid cards.",
        Website = "https://www.mastercard.com",
        Headquarters = "Purchase, USA",
        EmployeeCount = 24000,
        FoundedYear = 1966,
        PhoneNumber = "+1 914-249-2000",
        Email = "info@mastercard.com",
        Address = "Purchase, NY, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "MercadoPago",
        LogoUrl = "",
        Description = "MercadoPago is an Argentine fintech platform (part of MercadoLibre) that provides online payment services and digital wallet solutions across Latin America.",
        Website = "https://www.mercadopago.com",
        Headquarters = "Buenos Aires, Argentina",
        EmployeeCount = 2000,
        FoundedYear = 2004,
        PhoneNumber = "+54 11 0000 0000",
        Email = "ayuda@mercadopago.com",
        Address = "Buenos Aires, Argentina"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Mollie",
        LogoUrl = "",
        Description = "Mollie is a Dutch payment gateway offering simple online payment solutions and integrations for businesses in Europe.",
        Website = "https://www.mollie.com",
        Headquarters = "Amsterdam, Netherlands",
        EmployeeCount = 700,
        FoundedYear = 2004,
        PhoneNumber = "+31 20 820 2080",
        Email = "info@mollie.com",
        Address = "Amsterdam, Netherlands"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Moneris",
        LogoUrl = "",
        Description = "Moneris is a Canadian payment processing company (a joint venture of RBC and BMO) that provides merchant services and payment processing solutions in North America.",
        Website = "https://www.moneris.com",
        Headquarters = "Toronto, Canada",
        EmployeeCount = 1500,
        FoundedYear = 2000,
        PhoneNumber = "+1 855-423-8472",
        Email = "support@moneris.com",
        Address = "Toronto, Canada"
    },
    new PosCompany
    {Id = ++_id,
        Name = "MoneyGram",
        LogoUrl = "",
        Description = "MoneyGram is an American money transfer company that offers global person-to-person money transfer and bill payment services.",
        Website = "https://www.moneygram.com",
        Headquarters = "Dallas, USA",
        EmployeeCount = 2500,
        FoundedYear = 1940,
        PhoneNumber = "+1 800-926-9400",
        Email = "customerservice@moneygram.com",
        Address = "Dallas, TX, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "NCR Corporation",
        LogoUrl = "",
        Description = "NCR Corporation is an American enterprise technology company that produces ATMs, point-of-sale (POS) systems, and other payment and retail hardware and software.",
        Website = "https://www.ncr.com",
        Headquarters = "Atlanta, USA",
        EmployeeCount = 36000,
        FoundedYear = 1884,
        PhoneNumber = "+1 937-445-1936",
        Email = "contact@ncr.com",
        Address = "Atlanta, GA, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Neteller",
        LogoUrl = "",
        Description = "Neteller is a UK-based digital wallet service that allows users to transfer money and make payments online (part of Paysafe Group).",
        Website = "https://www.neteller.com",
        Headquarters = "London, UK",
        EmployeeCount = 200,
        FoundedYear = 1999,
        PhoneNumber = "+44 20 3308 9525",
        Email = "support@neteller.com",
        Address = "London, UK"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Nets",
        LogoUrl = "",
        Description = "Nets is a Danish payment services provider that offers digital payment solutions, including card processing and digital ID services, across Northern Europe.",
        Website = "https://www.nets.eu",
        Headquarters = "Ballerup, Denmark",
        EmployeeCount = 2600,
        FoundedYear = 1968,
        PhoneNumber = "+45 44 68 44 68",
        Email = "info@nets.eu",
        Address = "Ballerup, Denmark"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Network International",
        LogoUrl = "",
        Description = "Network International is a UAE-based payment solutions provider offering merchant acquiring, card processing, and digital payment solutions across the Middle East and Africa.",
        Website = "https://www.network.ae",
        Headquarters = "Dubai, UAE",
        EmployeeCount = 1500,
        FoundedYear = 1994,
        PhoneNumber = "+971 4 303 2432",
        Email = "marketing@network.ae",
        Address = "Dubai, UAE"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Nexi",
        LogoUrl = "",
        Description = "Nexi is an Italian digital payments company providing payment technology solutions, including card issuing, merchant services, and digital banking, across Europe.",
        Website = "https://www.nexi.it",
        Headquarters = "Milan, Italy",
        EmployeeCount = 10000,
        FoundedYear = 2017,
        PhoneNumber = "+39 02 3488 0892",
        Email = "info@nexi.it",
        Address = "Milan, Italy"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Nuvei",
        LogoUrl = "",
        Description = "Nuvei is a Canadian fintech company that provides payment processing technology and services to merchants, including support for card payments and alternative payment methods.",
        Website = "https://www.nuvei.com",
        Headquarters = "Montreal, Canada",
        EmployeeCount = 1500,
        FoundedYear = 2003,
        PhoneNumber = "+1 514-390-2030",
        Email = "support@nuvei.com",
        Address = "Montreal, Canada"
    },
    new PosCompany
    {Id = ++_id,
        Name = "OVO",
        LogoUrl = "",
        Description = "OVO is an Indonesian digital wallet and payments service that enables users to make cashless payments, transfer funds, and earn loyalty rewards via a mobile app.",
        Website = "https://www.ovo.id",
        Headquarters = "Jakarta, Indonesia",
        EmployeeCount = 500,
        FoundedYear = 2017,
        PhoneNumber = "+62 21 0000 0000",
        Email = "cs@ovo.id",
        Address = "Jakarta, Indonesia"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Omise",
        LogoUrl = "",
        Description = "Omise is a Thailand-based payment gateway providing online payment processing solutions in Southeast Asia, with a focus on ease of integration for merchants.",
        Website = "https://www.omise.co",
        Headquarters = "Bangkok, Thailand",
        EmployeeCount = 300,
        FoundedYear = 2013,
        PhoneNumber = "+66 2 000 0000",
        Email = "support@omise.co",
        Address = "Bangkok, Thailand"
    },
    new PosCompany
    {Id = ++_id,
        Name = "PAX Technology",
        LogoUrl = "",
        Description = "PAX Technology is a Chinese manufacturer of payment terminals and POS hardware, supplying secure electronic payment devices to financial institutions worldwide.",
        Website = "https://www.paxtechnology.com",
        Headquarters = "Shenzhen, China",
        EmployeeCount = 1800,
        FoundedYear = 2000,
        PhoneNumber = "+86 755 86169630",
        Email = "sales@pax.com.cn",
        Address = "Shenzhen, China"
    },
    new PosCompany
    {Id = ++_id,
        Name = "PagSeguro",
        LogoUrl = "",
        Description = "PagSeguro is a Brazilian fintech company offering online payment services, mobile payments, and fintech solutions, including its popular PagBank digital account.",
        Website = "https://pagseguro.uol.com.br",
        Headquarters = "São Paulo, Brazil",
        EmployeeCount = 8000,
        FoundedYear = 2006,
        PhoneNumber = "+55 11 3028-9000",
        Email = "atendimento@pagseguro.com.br",
        Address = "São Paulo, Brazil"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Papara",
        LogoUrl = "",
        Description = "Papara is a Turkish fintech company that provides a digital wallet and prepaid card service, enabling users to send money and make online payments.",
        Website = "https://www.papara.com",
        Headquarters = "Istanbul, Turkey",
        EmployeeCount = 300,
        FoundedYear = 2016,
        PhoneNumber = "+90 850 340 0 340",
        Email = "support@papara.com",
        Address = "Istanbul, Turkey"
    },
    new PosCompany
    {Id = ++_id,
        Name = "PayCore",
        LogoUrl = "",
        Description = "PayCore (formerly Cardtek) is a Turkish payment technology company that develops end-to-end digital payment solutions and EMV card infrastructure for banks and fintechs.",
        Website = "https://www.paycore.com",
        Headquarters = "Istanbul, Turkey",
        EmployeeCount = 450,
        FoundedYear = 2001,
        PhoneNumber = "+90 216 000 0000",
        Email = "info@paycore.com",
        Address = "Istanbul, Turkey"
    },
    new PosCompany
    {Id = ++_id,
        Name = "PayFast",
        LogoUrl = "",
        Description = "PayFast is a South African online payment gateway that enables businesses and individuals to send and receive payments online easily and securely.",
        Website = "https://www.payfast.co.za",
        Headquarters = "Cape Town, South Africa",
        EmployeeCount = 100,
        FoundedYear = 2007,
        PhoneNumber = "+27 21 300 4455",
        Email = "support@payfast.co.za",
        Address = "Cape Town, South Africa"
    },
    new PosCompany
    {Id = ++_id,
        Name = "PayPal",
        LogoUrl = "",
        Description = "PayPal is an American online payments platform that allows individuals and businesses to send and receive money securely across the globe.",
        Website = "https://www.paypal.com",
        Headquarters = "San Jose, USA",
        EmployeeCount = 27200,
        FoundedYear = 1998,
        PhoneNumber = "+1 888-221-1161",
        Email = "support@paypal.com",
        Address = "2211 North First Street, San Jose, CA, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "PayTR",
        LogoUrl = "",
        Description = "PayTR is a Turkish payment service provider that offers secure online payment solutions and e-wallet services for businesses and consumers.",
        Website = "https://www.paytr.com",
        Headquarters = "Izmir, Turkey",
        EmployeeCount = 200,
        FoundedYear = 2009,
        PhoneNumber = "+90 850 811 0 729",
        Email = "destek@paytr.com",
        Address = "Izmir, Turkey"
    },
    new PosCompany
    {Id = ++_id,
        Name = "PayTabs",
        LogoUrl = "",
        Description = "PayTabs is a Saudi-based online payment gateway provider that offers secure payment processing solutions for businesses in the Middle East and beyond.",
        Website = "https://www.paytabs.com",
        Headquarters = "Al Khobar, Saudi Arabia",
        EmployeeCount = 150,
        FoundedYear = 2014,
        PhoneNumber = "+966 13 000 0000",
        Email = "sales@paytabs.com",
        Address = "Al Khobar, Saudi Arabia"
    },
    new PosCompany
    {Id = ++_id,
        Name = "PayU",
        LogoUrl = "",
        Description = "PayU is a Netherlands-based payment service provider (part of Prosus) that operates in emerging markets, enabling online merchants to accept local payment methods.",
        Website = "https://www.payu.com",
        Headquarters = "Hoofddorp, Netherlands",
        EmployeeCount = 3000,
        FoundedYear = 2002,
        PhoneNumber = "+31 20 000 0000",
        Email = "support@payu.com",
        Address = "Hoofddorp, Netherlands"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Payoneer",
        LogoUrl = "",
        Description = "Payoneer is an American financial services company that offers online money transfer, digital payment services, and prepaid cards for cross-border payments.",
        Website = "https://www.payoneer.com",
        Headquarters = "New York, USA",
        EmployeeCount = 2000,
        FoundedYear = 2005,
        PhoneNumber = "+1 212-600-9272",
        Email = "support@payoneer.com",
        Address = "New York, NY, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Paysafe",
        LogoUrl = "",
        Description = "Paysafe is a UK-based payments company that provides digital payment solutions, including online wallets (Skrill, Neteller) and payment processing services.",
        Website = "https://www.paysafe.com",
        Headquarters = "London, UK",
        EmployeeCount = 3200,
        FoundedYear = 1996,
        PhoneNumber = "+44 20 3884 0000",
        Email = "info@paysafe.com",
        Address = "London, UK"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Paytm",
        LogoUrl = "",
        Description = "Paytm is an Indian fintech company offering a mobile wallet, e-commerce payment system, and financial services, widely used for digital payments in India.",
        Website = "https://www.paytm.com",
        Headquarters = "Noida, India",
        EmployeeCount = 10000,
        FoundedYear = 2010,
        PhoneNumber = "+91 120 4770770",
        Email = "care@paytm.com",
        Address = "Noida, India"
    },
    new PosCompany
    {Id = ++_id,
        Name = "PhonePe",
        LogoUrl = "",
        Description = "PhonePe is an Indian digital payments platform that provides mobile payment and financial services through the Unified Payments Interface (UPI) and other offerings.",
        Website = "https://www.phonepe.com",
        Headquarters = "Bangalore, India",
        EmployeeCount = 5000,
        FoundedYear = 2015,
        PhoneNumber = "+91 80 0000 0000",
        Email = "support@phonepe.com",
        Address = "Bangalore, India"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Poynt",
        LogoUrl = "",
        Description = "Poynt is an American maker of smart point-of-sale terminals and software, which allows merchants to manage payments and apps on a connected device (acquired by GoDaddy).",
        Website = "https://www.poynt.com",
        Headquarters = "Palo Alto, USA",
        EmployeeCount = 150,
        FoundedYear = 2014,
        PhoneNumber = "+1 650-781-8000",
        Email = "info@poynt.com",
        Address = "Palo Alto, CA, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Rapyd",
        LogoUrl = "",
        Description = "Rapyd is a UK-based fintech-as-a-service platform that offers a unified cloud-based payment infrastructure enabling businesses to integrate local payment methods worldwide.",
        Website = "https://www.rapyd.net",
        Headquarters = "London, UK",
        EmployeeCount = 500,
        FoundedYear = 2016,
        PhoneNumber = "+44 20 0000 0000",
        Email = "support@rapyd.net",
        Address = "London, UK"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Razorpay",
        LogoUrl = "",
        Description = "Razorpay is an Indian fintech platform that provides payment gateway services, allowing businesses to accept, process, and disburse payments online.",
        Website = "https://www.razorpay.com",
        Headquarters = "Bangalore, India",
        EmployeeCount = 3000,
        FoundedYear = 2014,
        PhoneNumber = "+91 80 0000 0000",
        Email = "support@razorpay.com",
        Address = "Bangalore, India"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Remitly",
        LogoUrl = "",
        Description = "Remitly is an American fintech company that offers a digital remittance platform, enabling individuals to send money internationally through a mobile app or website.",
        Website = "https://www.remitly.com",
        Headquarters = "Seattle, USA",
        EmployeeCount = 1800,
        FoundedYear = 2011,
        PhoneNumber = "+1 888-736-4859",
        Email = "service@remitly.com",
        Address = "Seattle, WA, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Revel Systems",
        LogoUrl = "",
        Description = "Revel Systems is an American company that provides an iPad-based point-of-sale system for restaurants and retailers, integrating operations and payments.",
        Website = "https://www.revelsystems.com",
        Headquarters = "San Francisco, USA",
        EmployeeCount = 500,
        FoundedYear = 2010,
        PhoneNumber = "+1 415-744-1433",
        Email = "info@revelsystems.com",
        Address = "San Francisco, CA, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Shift4 Payments",
        LogoUrl = "",
        Description = "Shift4 Payments is an American payments technology company that provides integrated payment processing and technology solutions to restaurants, hotels, and other businesses.",
        Website = "https://www.shift4.com",
        Headquarters = "Allentown, USA",
        EmployeeCount = 2300,
        FoundedYear = 1999,
        PhoneNumber = "+1 888-276-2108",
        Email = "sales@shift4.com",
        Address = "Allentown, PA, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Shopify",
        LogoUrl = "",
        Description = "Shopify is a Canadian e-commerce company that provides an online platform for businesses, including payment processing and point-of-sale systems for in-person sales.",
        Website = "https://www.shopify.com",
        Headquarters = "Ottawa, Canada",
        EmployeeCount = 10000,
        FoundedYear = 2006,
        PhoneNumber = "+1 613-000-0000",
        Email = "support@shopify.com",
        Address = "Ottawa, Canada"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Sipay",
        LogoUrl = "",
        Description = "Sipay is a Turkish fintech company offering digital payment solutions, including a payment gateway, digital wallet, and installment payment services.",
        Website = "https://www.sipay.com",
        Headquarters = "Istanbul, Turkey",
        EmployeeCount = 100,
        FoundedYear = 2018,
        PhoneNumber = "+90 212 000 0000",
        Email = "info@sipay.com",
        Address = "Istanbul, Turkey"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Skrill",
        LogoUrl = "",
        Description = "Skrill is a UK-based digital wallet and online payments company that allows users to make transfers and payments online (part of Paysafe Group).",
        Website = "https://www.skrill.com",
        Headquarters = "London, UK",
        EmployeeCount = 500,
        FoundedYear = 2001,
        PhoneNumber = "+44 20 3308 2520",
        Email = "help@skrill.com",
        Address = "London, UK"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Square",
        LogoUrl = "",
        Description = "Square (now Block, Inc.) is an American financial services and digital payments company that offers point-of-sale software and hardware, and mobile payment solutions for merchants.",
        Website = "https://squareup.com",
        Headquarters = "San Francisco, USA",
        EmployeeCount = 6000,
        FoundedYear = 2009,
        PhoneNumber = "+1 855-700-6000",
        Email = "support@squareup.com",
        Address = "San Francisco, CA, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Stone (StoneCo)",
        LogoUrl = "",
        Description = "StoneCo is a Brazilian fintech company that provides payment processing, point-of-sale technology, and financial services to merchants in Brazil.",
        Website = "https://www.stone.com.br",
        Headquarters = "São Paulo, Brazil",
        EmployeeCount = 7000,
        FoundedYear = 2012,
        PhoneNumber = "+55 11 0000 0000",
        Email = "contato@stone.com.br",
        Address = "São Paulo, Brazil"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Stripe",
        LogoUrl = "",
        Description = "Stripe is an American payment processing platform that allows businesses to accept payments online and via mobile apps, providing a suite of APIs for developers.",
        Website = "https://www.stripe.com",
        Headquarters = "San Francisco, USA",
        EmployeeCount = 7000,
        FoundedYear = 2010,
        PhoneNumber = "+1 888-963-8955",
        Email = "support@stripe.com",
        Address = "San Francisco, CA, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "SumUp",
        LogoUrl = "",
        Description = "SumUp is a UK-based fintech company that provides mobile card readers and point-of-sale solutions, enabling small merchants to accept card payments anywhere.",
        Website = "https://www.sumup.com",
        Headquarters = "London, UK",
        EmployeeCount = 3000,
        FoundedYear = 2012,
        PhoneNumber = "+44 20 7666 1767",
        Email = "support@sumup.com",
        Address = "London, UK"
    },
    new PosCompany
    {Id = ++_id,
        Name = "TSYS",
        LogoUrl = "",
        Description = "TSYS (Total System Services) is an American payment processor that provides payment processing and card issuing services to banks and merchants worldwide.",
        Website = "https://www.tsys.com",
        Headquarters = "Columbus, USA",
        EmployeeCount = 11000,
        FoundedYear = 1983,
        PhoneNumber = "+1 844-663-8797",
        Email = "info@tsys.com",
        Address = "Columbus, GA, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Toast",
        LogoUrl = "",
        Description = "Toast is an American restaurant technology company that offers a cloud-based point-of-sale system specifically designed for restaurants, including integrated payment processing.",
        Website = "https://www.toasttab.com",
        Headquarters = "Boston, USA",
        EmployeeCount = 3000,
        FoundedYear = 2012,
        PhoneNumber = "+1 617-682-0225",
        Email = "info@toasttab.com",
        Address = "Boston, MA, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Trustly",
        LogoUrl = "",
        Description = "Trustly is a Swedish fintech company that enables online bank payments directly from consumer bank accounts, providing an alternative to card payments across Europe.",
        Website = "https://www.trustly.com",
        Headquarters = "Stockholm, Sweden",
        EmployeeCount = 500,
        FoundedYear = 2008,
        PhoneNumber = "+46 8 446 831 33",
        Email = "support@trustly.com",
        Address = "Stockholm, Sweden"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Tyro",
        LogoUrl = "",
        Description = "Tyro is an Australian challenger bank specializing in merchant credit and EFTPOS services, providing payment processing and banking solutions for SMEs.",
        Website = "https://www.tyro.com",
        Headquarters = "Sydney, Australia",
        EmployeeCount = 600,
        FoundedYear = 2003,
        PhoneNumber = "+61 2 8907 1700",
        Email = "customersupport@tyro.com",
        Address = "Sydney, Australia"
    },
    new PosCompany
    {Id = ++_id,
        Name = "UnionPay",
        LogoUrl = "",
        Description = "UnionPay is a Chinese financial services corporation that operates the UnionPay card network, the largest card payment network in the world by transaction volume.",
        Website = "https://www.unionpayintl.com",
        Headquarters = "Shanghai, China",
        EmployeeCount = 5000,
        FoundedYear = 2002,
        PhoneNumber = "+86 21 2026 5828",
        Email = "service@unionpayintl.com",
        Address = "Shanghai, China"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Verifone",
        LogoUrl = "",
        Description = "Verifone is an American company that provides secure electronic payment technologies, specializing in point-of-sale payment terminals and payment software.",
        Website = "https://www.verifone.com",
        Headquarters = "San Jose, USA",
        EmployeeCount = 5000,
        FoundedYear = 1981,
        PhoneNumber = "+1 408-232-7800",
        Email = "sales@verifone.com",
        Address = "San Jose, CA, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Visa",
        LogoUrl = "",
        Description = "Visa Inc. is an American multinational financial services corporation known for its global electronic payments network, facilitating card transactions between consumers, merchants, and banks.",
        Website = "https://www.visa.com",
        Headquarters = "Foster City, USA",
        EmployeeCount = 21500,
        FoundedYear = 1958,
        PhoneNumber = "+1 650-432-3200",
        Email = "askvisa@visa.com",
        Address = "Foster City, CA, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "WeChat Pay",
        LogoUrl = "",
        Description = "WeChat Pay is a Chinese mobile payment and digital wallet service integrated into the WeChat app (by Tencent), enabling users to make payments and money transfers via smartphone.",
        Website = "https://www.wechat.com",
        Headquarters = "Shenzhen, China",
        EmployeeCount = 10000,
        FoundedYear = 2013,
        PhoneNumber = "+86 755 86013388",
        Email = "support@wechat.com",
        Address = "Shenzhen, China"
    },
    new PosCompany
    {Id = ++_id,
        Name = "WePay",
        LogoUrl = "",
        Description = "WePay is an American online payment service provider that offers integrated payments for platforms and marketplaces (acquired by JPMorgan Chase).",
        Website = "https://www.wepay.com",
        Headquarters = "Redwood City, USA",
        EmployeeCount = 300,
        FoundedYear = 2008,
        PhoneNumber = "+1 855-469-3729",
        Email = "support@wepay.com",
        Address = "Redwood City, CA, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Western Union",
        LogoUrl = "",
        Description = "Western Union is an American financial services company specializing in international money transfers, allowing individuals to send and receive money across the globe.",
        Website = "https://www.westernunion.com",
        Headquarters = "Denver, USA",
        EmployeeCount = 11000,
        FoundedYear = 1851,
        PhoneNumber = "+1 800-325-6000",
        Email = "customer@westernunion.com",
        Address = "Denver, CO, USA"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Windcave",
        LogoUrl = "",
        Description = "Windcave (formerly Payment Express) is a New Zealand-based payment gateway that provides secure payment processing solutions for in-store, online, and unattended payments.",
        Website = "https://www.windcave.com",
        Headquarters = "Auckland, New Zealand",
        EmployeeCount = 200,
        FoundedYear = 1997,
        PhoneNumber = "+64 9 123 4567",
        Email = "support@windcave.com",
        Address = "Auckland, New Zealand"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Wise",
        LogoUrl = "",
        Description = "Wise (formerly TransferWise) is a British fintech company offering an online platform for international money transfers with low fees and transparent exchange rates.",
        Website = "https://www.wise.com",
        Headquarters = "London, UK",
        EmployeeCount = 4000,
        FoundedYear = 2011,
        PhoneNumber = "+44 20 3695 0999",
        Email = "support@wise.com",
        Address = "London, UK"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Worldline",
        LogoUrl = "",
        Description = "Worldline is a French multinational payment and transactional services company that provides merchants, banks, and governments with e-payment services and point-of-sale technology.",
        Website = "https://www.worldline.com",
        Headquarters = "Paris, France",
        EmployeeCount = 20000,
        FoundedYear = 1973,
        PhoneNumber = "+33 1 0000 0000",
        Email = "info@worldline.com",
        Address = "Paris, France"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Worldpay",
        LogoUrl = "",
        Description = "Worldpay is a global payment processing company (originally from the UK) that provides merchant acquiring and payment processing services for businesses worldwide.",
        Website = "https://www.worldpay.com",
        Headquarters = "London, UK",
        EmployeeCount = 8000,
        FoundedYear = 1989,
        PhoneNumber = "+44 20 0000 0000",
        Email = "support@worldpay.com",
        Address = "London, UK"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Xendit",
        LogoUrl = "",
        Description = "Xendit is an Indonesian payment gateway startup that offers businesses in Southeast Asia a platform for online payments, disbursements, and e-wallet integrations.",
        Website = "https://www.xendit.co",
        Headquarters = "Jakarta, Indonesia",
        EmployeeCount = 700,
        FoundedYear = 2015,
        PhoneNumber = "+62 21 0000 0000",
        Email = "help@xendit.co",
        Address = "Jakarta, Indonesia"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Yoco",
        LogoUrl = "",
        Description = "Yoco is a South African fintech company that provides portable card readers and payment solutions for small businesses to accept card payments easily.",
        Website = "https://www.yoco.com",
        Headquarters = "Cape Town, South Africa",
        EmployeeCount = 200,
        FoundedYear = 2013,
        PhoneNumber = "+27 21 000 0000",
        Email = "hello@yoco.com",
        Address = "Cape Town, South Africa"
    },
    new PosCompany
    {Id = ++_id,
        Name = "Zip",
        LogoUrl = "",
        Description = "Zip Co is an Australian buy now, pay later provider that offers point-of-sale credit and digital payment solutions, allowing consumers to pay in installments.",
        Website = "https://www.zip.co",
        Headquarters = "Sydney, Australia",
        EmployeeCount = 1000,
        FoundedYear = 2013,
        PhoneNumber = "+61 2 8294 2345",
        Email = "info@zip.co",
        Address = "Sydney, Australia"
    }
};

            foreach (var item in posCompanies)
            {
                item.Slug = item.Name.ToString();
                item.MetaDescription = item.Description.ToString();
                item.Title = item.Id.ToString();
                item.CultureId = 1;
                item.Title = item.Name;
                item.Slug = item.Name;
            }
            return posCompanies;
        }


        public static Dictionary<string, long> SeedPosServicesFromData(
            ModelBuilder builder,
            List<PosServiceCategory> categories,
            Dictionary<string, (string Name, string Description, string Category, bool IsRegional)> serviceData)
        {
            // Kategori adından Id çekmek için
            Func<string, long> getCatId = categoryName =>
            {
                var cat = categories.FirstOrDefault(c => string.Equals(c.Name, categoryName, StringComparison.OrdinalIgnoreCase));
                var toReturn = cat != null ? cat.Id : 1; // Eğer bulunamazsa varsayılan olarak 1'e ata

                return toReturn;

            };

            var services = new List<PosService>();
            var serviceKeyToIdMap = new Dictionary<string, long>();
            long nextId = 1001;
            foreach (var kvp in serviceData)
            {
                string key = kvp.Key;
                var (name, description, category, isRegional) = kvp.Value;
                services.Add(new PosService
                {
                    Id = nextId,
                    Name = name,
                    Slug = name,
                    Title = name,
                    MetaDescription = name,
                    Description = description,
                    // Kategori, resx verilerindeki key ile eşleşmeli; burada boşluk ve büyük/küçük harf farkı olmaması için:
                    PosServiceCategoryId = getCatId(category.Replace(" ", "")),
                    IsGlobal = true,
                    IsRegional = isRegional,
                    CultureId = 1
                });
                serviceKeyToIdMap[key] = nextId;
                nextId++;
            }
            builder.Entity<PosService>().HasData(services);
            return serviceKeyToIdMap;
        }
    }
}