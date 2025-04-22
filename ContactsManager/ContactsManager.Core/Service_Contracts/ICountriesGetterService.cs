using Entities_DTO;

namespace Service_Contracts
{
    public interface ICountriesGetterService
    {
        /// <summary>
        /// Get All Countries
        /// </summary>
        /// <returns></returns>
        public Task<List<CountryResponse>?> GetAllCountries();

        /// <summary>
        /// Get a Country by Country Id
        /// </summary>
        /// <param name="countryId"></param>
        /// <returns></returns>
        public Task<CountryResponse?> GetCountryById(Guid countryId);
    }
}
