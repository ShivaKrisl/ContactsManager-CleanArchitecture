using Entities_Core;
using Repository_Contracts;
using Storage;

namespace Repository_Classes
{
    public class CountriesRepository : ICountriesRepository
    {
        private readonly JsonFileStore _fileStore;

        private readonly string _fileName;

        public CountriesRepository(JsonFileStore fileStore, Microsoft.Extensions.Options.IOptions<FileStorageOptions> options)
        {
            _fileStore = fileStore;
            _fileName = options.Value.CountriesFileName;
        }

        /// <summary>
        /// Add a Country to a data base
        /// </summary>
        /// <param name="country"></param>
        /// <returns></returns>
        public async Task<Country> AddCountry(Country country)
        {
            List<CountryRecord> countries = await ReadCountryRecordsAsync();
            countries.Add(CountryRecord.FromCountry(country));
            await _fileStore.WriteListAsync(_fileName, countries);
            return country;
        }

        /// <summary>
        /// Get all Countries from data base
        /// </summary>
        /// <returns></returns>
        public async Task<List<Country>?> GetAllCountries()
        {
            return (await ReadCountryRecordsAsync()).Select(record => record.ToCountry()).ToList();
        }

        /// <summary>
        /// Get Country By Id from data base
        /// </summary>
        /// <param name="countryId"></param>
        /// <returns></returns>
        public async Task<Country?> GetCountryById(Guid countryId)
        {
            return (await GetAllCountries())?.FirstOrDefault(country => country.CountryId == countryId);
        }

        /// <summary>
        /// Get Country By Name from data base
        /// </summary>
        /// <param name="countryName"></param>
        /// <returns></returns>
        public async Task<Country?> GetCountryByName(string countryName)
        {
            return (await GetAllCountries())?.FirstOrDefault(country => string.Equals(country.CountryName, countryName, StringComparison.OrdinalIgnoreCase));

        }

        private async Task<List<CountryRecord>> ReadCountryRecordsAsync()
        {
            return await _fileStore.ReadListAsync(_fileName, () => GetDefaultCountries().Select(CountryRecord.FromCountry).ToList());
        }

        private static List<Country> GetDefaultCountries()
        {
            return new List<Country>
            {
                new Country { CountryId = Guid.NewGuid(), CountryName = "India" },
                new Country { CountryId = Guid.NewGuid(), CountryName = "United States" },
                new Country { CountryId = Guid.NewGuid(), CountryName = "United Kingdom" },
                new Country { CountryId = Guid.NewGuid(), CountryName = "Canada" },
                new Country { CountryId = Guid.NewGuid(), CountryName = "Australia" }
            };
        }

        private sealed record CountryRecord(Guid CountryId, string? CountryName)
        {
            public static CountryRecord FromCountry(Country country)
            {
                return new CountryRecord(country.CountryId, country.CountryName);
            }

            public Country ToCountry()
            {
                return new Country
                {
                    CountryId = CountryId,
                    CountryName = CountryName
                };
            }
        }
    }
}
