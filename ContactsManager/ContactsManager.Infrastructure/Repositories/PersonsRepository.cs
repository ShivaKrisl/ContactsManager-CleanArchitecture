using System.Linq.Expressions;
using Entities_Core;
using Repository_Contracts;
using Storage;

namespace Repository_Classes
{
    public class PersonsRepository : IPersonsRepository
    {
        private readonly JsonFileStore _fileStore;

        private readonly ICountriesRepository _countriesRepository;

        private readonly string _fileName;

        public PersonsRepository(JsonFileStore fileStore, ICountriesRepository countriesRepository, Microsoft.Extensions.Options.IOptions<FileStorageOptions> options)
        {
            _fileStore = fileStore;
            _countriesRepository = countriesRepository;
            _fileName = options.Value.PersonsFileName;
        }

        /// <summary>
        /// Add a Person to a data base
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public async Task<Person> AddPerson(Person person)
        {
            List<PersonRecord> persons = await ReadPersonRecordsAsync();
            persons.Add(PersonRecord.FromPerson(person));
            await _fileStore.WriteListAsync(_fileName, persons);
            return person;
        }

        /// <summary>
        /// Delete a Person from a data base
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        public async Task<bool> DeletePerson(Guid personId)
        {
            List<PersonRecord> persons = await ReadPersonRecordsAsync();
            int removedCount = persons.RemoveAll(person => person.PersonId == personId);
            if (removedCount == 0)
            {
                return false;
            }

            await _fileStore.WriteListAsync(_fileName, persons);
            return true;
        }

        /// <summary>
        /// Get all Persons from data base
        /// </summary>
        /// <returns></returns>
        public async Task<List<Person>?> GetAllPersons()
        {
            return await HydratePersonsAsync(await ReadPersonRecordsAsync());
        }

        /// <summary>
        /// Get Filtered Persons from data base
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<List<Person>?> GetFilteredPersons(Expression<Func<Person, bool>> predicate)
        {
            List<Person> persons = await GetAllPersons() ?? new List<Person>();
            return persons.AsQueryable().Where(predicate).ToList();
        }

        /// <summary>
        /// Get Person By Id from data base
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        public async Task<Person?> GetPersonById(Guid personId)
        {
            return (await GetAllPersons())?.FirstOrDefault(person => person.PersonId == personId);
        }

        public async Task<Person> UpdatePerson(Guid personId, Person person)
        {
            List<PersonRecord> persons = await ReadPersonRecordsAsync();
            PersonRecord? existing = persons.FirstOrDefault(item => item.PersonId == personId);

            if (existing == null)
            {
                return person;
            }

            persons.Remove(existing);
            persons.Add(PersonRecord.FromPerson(new Person
            {
                PersonId = personId,
                FirstName = person.FirstName,
                LastName = person.LastName,
                Email = person.Email,
                PhoneNumber = person.PhoneNumber,
                DateOfBirth = person.DateOfBirth,
                Gender = person.Gender,
                CountryId = person.CountryId
            }));

            await _fileStore.WriteListAsync(_fileName, persons);

            Person? updatedPerson = await GetPersonById(personId);
            return updatedPerson ?? person;
        }

        /// <summary>
        /// Get Person By Email Id from data base
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<Person?> GetPersonByEmail(string email)
        {
            return (await GetAllPersons())?.FirstOrDefault(person => string.Equals(person.Email, email, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Get Person By Mobile Number from data base
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public async Task<Person?> GetPersonByMobile(string mobile)
        {
            return (await GetAllPersons())?.FirstOrDefault(person => string.Equals(person.PhoneNumber, mobile, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<List<PersonRecord>> ReadPersonRecordsAsync()
        {
            return await _fileStore.ReadListAsync(_fileName, () => new List<PersonRecord>());
        }

        private async Task<List<Person>?> HydratePersonsAsync(List<PersonRecord> records)
        {
            List<Person> persons = new();
            foreach (PersonRecord record in records)
            {
                Country? country = await _countriesRepository.GetCountryById(record.CountryId);
                persons.Add(record.ToPerson(country));
            }

            return persons;
        }

        private sealed record PersonRecord(
            Guid PersonId,
            string? FirstName,
            string? LastName,
            DateTime DateOfBirth,
            string? Email,
            string? PhoneNumber,
            string? Gender,
            Guid CountryId)
        {
            public static PersonRecord FromPerson(Person person)
            {
                return new PersonRecord(
                    person.PersonId,
                    person.FirstName,
                    person.LastName,
                    person.DateOfBirth,
                    person.Email,
                    person.PhoneNumber,
                    person.Gender,
                    person.CountryId);
            }

            public Person ToPerson(Country? country)
            {
                return new Person
                {
                    PersonId = PersonId,
                    FirstName = FirstName,
                    LastName = LastName,
                    DateOfBirth = DateOfBirth,
                    Email = Email,
                    PhoneNumber = PhoneNumber,
                    Gender = Gender,
                    CountryId = CountryId,
                    Country = country ?? new Country { CountryId = CountryId, CountryName = string.Empty }
                };
            }
        }
    }
}
