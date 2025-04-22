﻿using Entities_Core;
using Entities_DTO;
using Repository_Contracts;
using Helpers;
using Service_Contracts;

namespace Service_Classes
{
    public class PersonsAdderService : IPersonsAdderService
    {
        private readonly IPersonsRepository _personsRepository;

        private readonly ICountriesGetterService _countriesGetterService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="personsRepository"></param>
        /// <param name="countriesService"></param>
        public PersonsAdderService(IPersonsRepository personsRepository, ICountriesGetterService countriesService)
        {
            _personsRepository = personsRepository;
            _countriesGetterService = countriesService;
        }

        /// <summary>
        /// Add a Person to the database
        /// </summary>
        /// <param name="personRequest"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public async Task<PersonResponse> AddPerson(PersonRequest personRequest)
        {
            if (personRequest == null)
            {
                throw new ArgumentNullException(nameof(personRequest), "PersonRequest cannot be null");
            }

            bool isModelValid = ValidationHelper.IsModelValid(personRequest);
            if (!isModelValid)
            {
                throw new ArgumentException(nameof(personRequest), ValidationHelper.Errors);
            }

            bool isEmailExists = await _personsRepository.GetPersonByEmail(personRequest?.Email) != null ? true : false;
            if (isEmailExists)
            {
                throw new ArgumentException(nameof(personRequest.Email), "Email already exists");
            }

            bool isPhoneExists = await _personsRepository.GetPersonByMobile(personRequest?.PhoneNumber) != null ? true : false;

            if (isPhoneExists)
            {
                throw new ArgumentException(nameof(personRequest.PhoneNumber), "Phone number already exists");
            }

            CountryResponse? countryResponse = await _countriesGetterService.GetCountryById(personRequest.CountryId);
            if (countryResponse == null)
            {
                throw new ArgumentException(nameof(personRequest.CountryId), "Country not found");
            }

            Person person = personRequest.ToPerson();
            person.PersonId = Guid.NewGuid();

            await _personsRepository.AddPerson(person);

            return person.ToPersonResponse();

        }
    }
}
