using LinqApi.Repository;
using Microsoft.AspNetCore.Mvc;
using Posonl.Domain;
using Posonl.Web.Areas.Dashboard.Models;

namespace Posonl.Web.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [ApiController]
    [Route("dashboard/api/[controller]")]
    public class DashboardPosCompanyController : ControllerBase
    {
        private readonly ILinqRepository<PosCompany, long> _posCompanyRepository;
        private readonly ILinqRepository<Country, long> _countryRepository;

        public DashboardPosCompanyController(
            ILinqRepository<PosCompany, long> posCompanyRepository,
            ILinqRepository<Country, long> countryRepository)
        {
            _posCompanyRepository = posCompanyRepository;
            _countryRepository = countryRepository;
        }

        ///// <summary>
        ///// Adds supported countries to a given PosCompany.
        ///// </summary>
        ///// <param name="model">Payload containing the PosCompany Id and an array of country Ids to add.</param>
        ///// <param name="cancellationToken"></param>
        ///// <returns></returns>
        //[HttpPut("AddSupportedCountries")]
        //public async Task<IActionResult> AddSupportedCountries(
        //    [FromBody] AddSupportedCountryGroupsRequest model,
        //    CancellationToken cancellationToken)
        //{
        //    if (model == null || model.CountryIds == null || model.CountryIds.Length == 0)
        //    {
        //        return BadRequest("No country ids provided.");
        //    }

        //    var posCompany = await _posCompanyRepository.GetByIdAsync(model.PosCompanyId, cancellationToken);
        //    if (posCompany == null)
        //    {
        //        return NotFound("PosCompany not found.");
        //    }

        //    if (posCompany.CountryGroups == null)
        //    {
        //        posCompany.CountryGroups = new System.Collections.Generic.List<Country>();
        //    }

        //    foreach (var countryId in model.CountryIds)
        //    {
        //        var country = await _countryRepository.GetByIdAsync(countryId, cancellationToken);
        //        if (country != null && !posCompany.SupportedCountries.Any(c => c.Id == country.Id))
        //        {
        //            posCompany.SupportedCountries.Add(country);
        //        }
        //    }

        //    var updated = await _posCompanyRepository.UpdateAsync(posCompany, cancellationToken);
        //    return Ok(new { Message = "Countries added successfully.", UpdatedEntity = updated });
        //}

        ///// <summary>
        ///// Removes supported countries from a given PosCompany.
        ///// </summary>
        ///// <param name="model">Payload containing the PosCompany Id and an array of country Ids to remove.</param>
        ///// <param name="cancellationToken"></param>
        ///// <returns></returns>
        //[HttpPut("RemoveSupportedCountries")]
        //public async Task<IActionResult> RemoveSupportedCountries(
        //    [FromBody] RemoveSupportedCountryGroupsRequest model,
        //    CancellationToken cancellationToken)
        //{
        //    if (model == null || model.CountryIds == null || model.CountryIds.Length == 0)
        //    {
        //        return BadRequest("No country ids provided.");
        //    }

        //    var posCompany = await _posCompanyRepository.GetByIdAsync(model.PosCompanyId, cancellationToken);
        //    if (posCompany == null)
        //    {
        //        return NotFound("PosCompany not found.");
        //    }

        //    if (posCompany.SupportedCountries != null)
        //    {
        //        // Remove those countries that match the provided Ids.
        //        posCompany.SupportedCountries = posCompany.SupportedCountries
        //            .Where(c => !model.CountryIds.Contains(c.Id))
        //            .ToList();
        //    }

        //    var updated = await _posCompanyRepository.UpdateAsync(posCompany, cancellationToken);
        //    return Ok(new { Message = "Countries removed successfully.", UpdatedEntity = updated });
        //}
    }
}
