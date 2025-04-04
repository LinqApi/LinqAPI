using LinqApi.Controller;
using LinqApi.Logging;
using LinqApi.Repository;
using Microsoft.AspNetCore.Mvc;
using Posonl.Domain;
using Posonl.Web.ViewModels;

namespace Posonl.Web.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
        public IActionResult Example() => View();
        public IActionResult Country() => View();
        public IActionResult CountryGroup() => View();
        public IActionResult PosCompany() => View();
        public IActionResult PosService() => View();
        public IActionResult PosServiceCategory() => View();
        public IActionResult PosCommissionRate() => View();
        public IActionResult RatingCategory() => View();
        public IActionResult LinqSqlLogs() => View();
        public IActionResult LinqHttpCallLogs() => View();
        public IActionResult LinqEventLogs() => View();
        public IActionResult Healthcheck() => View();
    }

    //[ApiController]
    //[Area("Dashboard")]
    //[Route("api/[controller]")]
    //public class PosServiceCategoryController : LinqVmController<PosServiceCategory, CreatePosServiceCategoryVm, UpdatePosServiceCategoryVm, long>
    //{
    //    public PosServiceCategoryController(ILinqRepository<PosServiceCategory, long> repo) : base(repo) { }

    //    protected override PosServiceCategory MapToEntityFromCreate(CreatePosServiceCategoryVm vm) => new()
    //    {
    //        Description = vm.Description,
    //        Name = vm.Name
    //    };

    //    protected override PosServiceCategory MapToEntityFromUpdate(UpdatePosServiceCategoryVm vm)
    //    {
    //        var entity = MapToEntityFromCreate(vm);
    //        entity.Id = vm.Id;
    //        return entity;
    //    }
    //}

    //[ApiController]
    //[Area("Dashboard")]
    //[Route("api/[controller]")]
    //public class LinqHttpCallLogsController : LinqController<LinqHttpCallLog, long>
    //{
    //    public LinqHttpCallLogsController(ILinqRepository<LinqHttpCallLog, long> repo) : base(repo)
    //    {
    //    }
    //}


    //[ApiController]
    //[Area("Dashboard")]
    //[Route("api/[controller]")]
    //public class LinqSqlLogsController : LinqController<LinqSqlLog, long>
    //{
    //    public LinqSqlLogsController(ILinqRepository<LinqSqlLog, long> repo) : base(repo)
    //    {
    //    }
    //}

    //[ApiController]
    //[Area("Dashboard")]
    //[Route("api/[controller]")]
    //public class LinqSqlErrorLogsController : LinqController<LinqSqlErrorLog, long>
    //{
    //    public LinqSqlErrorLogsController(ILinqRepository<LinqSqlErrorLog, long> repo) : base(repo)
    //    {
    //    }
    //}

    //[ApiController]
    //[Area("Dashboard")]
    //[Route("api/[controller]")]
    //public class LinqEventLogsController : LinqController<LinqEventLog, long>
    //{
    //    public LinqEventLogsController(ILinqRepository<LinqEventLog, long> repo) : base(repo)
    //    {
    //    }
    //}



    //[Area("Dashboard")]
    //[ApiController]
    //[Route("api/[controller]")]
    //public class PosServiceController : LinqVmController<PosService, CreatePosServiceVm, UpdatePosServiceVm, long>
    //{
    //    public PosServiceController(ILinqRepository<PosService, long> repo) : base(repo) { }

    //    protected override PosService MapToEntityFromCreate(CreatePosServiceVm vm) => new()
    //    {
    //        PosServiceCategoryId = vm.PosServiceCategoryId,
    //        IsGlobal = vm.IsGlobal,
    //        IsRegional = vm.IsRegional,
    //        Name = vm.Name,
    //        Description = vm.Description
    //    };

    //    protected override PosService MapToEntityFromUpdate(UpdatePosServiceVm vm)
    //    {
    //        var entity = MapToEntityFromCreate(vm);
    //        entity.Id = vm.Id;
    //        return entity;
    //    }
    //}

    //[Area("Dashboard")]
    //[ApiController]
    //[Route("api/[controller]")]
    //public class PosCompanyController : LinqVmController<PosCompany, CreatePosCompanyVm, UpdatePosCompanyVm, long>
    //{
    //    public PosCompanyController(ILinqRepository<PosCompany, long> repo) : base(repo) { }

    //    protected override PosCompany MapToEntityFromCreate(CreatePosCompanyVm vm) => new()
    //    {
    //        Website = vm.Website,
    //        Headquarters = vm.Headquarters,
    //        EmployeeCount = vm.EmployeeCount,
    //        LogoUrl = vm.LogoUrl,
    //        PhoneNumber = vm.PhoneNumber,
    //        Email = vm.Email,
    //        Address = vm.Address,
    //        FoundedYear = vm.FoundedYear,
    //        StockTicker = vm.StockTicker,
    //        Name = vm.Name,
    //        Description = vm.Description
    //    };

    //    protected override PosCompany MapToEntityFromUpdate(UpdatePosCompanyVm vm)
    //    {
    //        var entity = MapToEntityFromCreate(vm);
    //        entity.Id = vm.Id;
    //        return entity;
    //    }
    //}

    //[Area("Dashboard")]
    //[ApiController]
    //[Route("api/[controller]")]
    //public class RatingCategoryController : LinqVmController<RatingCategory, CreateRatingCategoryVm, UpdateRatingCategoryVm, long>
    //{
    //    public RatingCategoryController(ILinqRepository<RatingCategory, long> repo) : base(repo) { }

    //    protected override RatingCategory MapToEntityFromCreate(CreateRatingCategoryVm vm) => new()
    //    {
    //        Name = vm.Name,
    //        Description = vm.Description
    //    };

    //    protected override RatingCategory MapToEntityFromUpdate(UpdateRatingCategoryVm vm)
    //    {
    //        var entity = MapToEntityFromCreate(vm);
    //        entity.Id = vm.Id;
    //        return entity;
    //    }
    //}

    //[Area("Dashboard")]
    //[ApiController]
    //[Route("api/[controller]")]
    //public class CountryController : LinqVmController<Country, CreateCountryVm, UpdateCountryVm, long>
    //{
    //    public CountryController(ILinqRepository<Country, long> repo) : base(repo) { }

    //    protected override Country MapToEntityFromCreate(CreateCountryVm vm) => new Country
    //    {
    //        Name = vm.Name,
    //        Code = vm.Code,
    //        Currency = vm.Currency,
    //        LanguageCode = vm.LanguageCode
    //    };

    //    protected override Country MapToEntityFromUpdate(UpdateCountryVm vm)
    //    {
    //        var entity = MapToEntityFromCreate(vm);
    //        entity.Id = vm.Id;
    //        return entity;
    //    }
    //}

    //[Area("Dashboard")]
    //[ApiController]
    //[Route("api/[controller]")]
    //public class CountryGroupController : LinqVmController<CountryGroup, CreateCountryGroupVm, UpdateCountryGroupVm, long>
    //{
    //    public CountryGroupController(ILinqRepository<CountryGroup, long> repo) : base(repo) { }

    //    protected override CountryGroup MapToEntityFromCreate(CreateCountryGroupVm vm) => new CountryGroup
    //    {
    //        Name = vm.Name
    //    };

    //    protected override CountryGroup MapToEntityFromUpdate(UpdateCountryGroupVm vm)
    //    {
    //        var entity = MapToEntityFromCreate(vm);
    //        entity.Id = vm.Id;
    //        return entity;
    //    }
    //}

    //[Area("Dashboard")]
    //[ApiController]
    //[Route("api/[controller]")]
    //public class PosCommissionRateController : LinqVmController<PosCommissionRate, CreatePosCommissionRateVm, UpdatePosCommissionRateVm, long>
    //{
    //    public PosCommissionRateController(ILinqRepository<PosCommissionRate, long> repo) : base(repo) { }

    //    protected override PosCommissionRate MapToEntityFromCreate(CreatePosCommissionRateVm vm) => new PosCommissionRate
    //    {
    //        PosCompanyId = vm.PosCompanyId,
    //        CountryId = vm.CountryId,
    //        CommissionPercentage = vm.CommissionPercentage
    //    };

    //    protected override PosCommissionRate MapToEntityFromUpdate(UpdatePosCommissionRateVm vm)
    //    {
    //        var entity = MapToEntityFromCreate(vm);
    //        entity.Id = vm.Id;
    //        return entity;
    //    }
    //}

}
