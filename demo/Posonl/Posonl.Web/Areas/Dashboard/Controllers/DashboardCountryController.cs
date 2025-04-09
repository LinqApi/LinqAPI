using LinqApi.Repository;
using Microsoft.AspNetCore.Mvc;
using Posonl.Domain;
using Posonl.Web.Areas.Dashboard.Models;

namespace Posonl.Web.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [ApiController]
    [Route("dashboard/api/[controller]")]
    public class DashboardCountryController : ControllerBase
    {
        private readonly ILinqRepository<Country, long> _countryRepository;
        private readonly ILinqRepository<CountryGroup, long> _countryGroupRepository;

        // Inject your country repository via dependency injection.
        public DashboardCountryController(ILinqRepository<Country, long> countryRepository, ILinqRepository<CountryGroup, long> countryGroupRepository)
        {
            _countryRepository = countryRepository;
            _countryGroupRepository = countryGroupRepository;
        }

        /// <summary>
        /// Endpoint to add existing countries (that currently have no group) to a specified CountryGroup.
        /// </summary>
        /// <param name="model">The payload containing the CountryGroup id and the array of country ids.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>An IActionResult indicating success or failure.</returns>
        [HttpPut("AddToGroup")]
        public async Task<IActionResult> AddCountriesToGroup(
            [FromBody] AddCountriesToGroupRequest model,
            CancellationToken cancellationToken)
        {
            // Validate that model is not null and there are country ids provided.
            if (model == null || model.CountryIds == null || model.CountryIds.Length == 0)
            {
                return BadRequest("No country ids provided.");
            }

            foreach (var countryId in model.CountryIds)
            {
                // For each countryId, update its CountryGroup property.
                // We assume that the repository's UpdateAsync method accepts a Country entity update.
                // You might need to first fetch the entity, update its CountryGroup or CountryGroupId property,
                // then call the update method.
                var country = await _countryRepository.GetByIdAsync(countryId, cancellationToken);
                if (country == null)
                {
                    // Optionally skip or return error if not found.
                    continue;
                }

                // Set the country's group property.
                country.CountryGroup = await _countryGroupRepository.GetByIdAsync(model.CountryGroupId, cancellationToken);

                // Update the country.
                await _countryRepository.UpdateAsync(country, cancellationToken);
            }

            return Ok(new { Message = "Countries updated successfully." });
        }


        /// <summary>
        /// Endpoint to remove countries from a specified CountryGroup.
        /// This sets the CountryGroup property of the selected country to null.
        /// </summary>
        [HttpPut("RemoveFromGroup")]
        public async Task<IActionResult> RemoveCountriesFromGroup(
            [FromBody] RemoveCountriesFromGroupRequest model,
            CancellationToken cancellationToken)
        {
            if (model == null || model.CountryIds == null || model.CountryIds.Length == 0)
            {
                return BadRequest("No country ids provided.");
            }

            foreach (var countryId in model.CountryIds)
            {
                var country = await _countryRepository.GetByIdAsync(countryId, cancellationToken);
                if (country == null)
                {
                    continue;
                }

                country.CountryGroupId = null;
                await _countryRepository.UpdateAsync(country, cancellationToken);
            }

            return Ok(new { Message = "Selected countries have been successfully removed from the group." });
        }
    }
}
