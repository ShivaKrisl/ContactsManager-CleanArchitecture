﻿using Entities_Core;
using Entities_DTO;
using Exceptions;
using Repository_Contracts;
using Helpers;
using Service_Contracts;

namespace Service_Classes
{
    public class PersonsUpdaterService : IPersonsUpdaterService
    {

        private readonly IPersonsRepository _personsRepository;

        private readonly ICountriesGetterService _countriesGetterService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="personsRepository"></param>
        /// <param name="countriesService"></param>
        public PersonsUpdaterService(IPersonsRepository personsRepository, ICountriesGetterService countriesService)
        {
            _personsRepository = personsRepository;
            _countriesGetterService = countriesService;
        }



        /// <summary>
        /// Update a Person by Person Id
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="personRequest"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<PersonResponse?> UpdatePerson(Guid personId, PersonRequest personRequest)
        {
            if (personId == Guid.Empty)
            {
                throw new ArgumentException(nameof(personId), "Invalid Person Id");
            }

            if (personRequest == null)
            {
                throw new ArgumentException(nameof(personRequest), "PersonRequest cannot be null");
            }


            bool isModelValid = ValidationHelper.IsModelValid(personRequest);
            if (!isModelValid)
            {
                throw new ArgumentException(nameof(personRequest), ValidationHelper.Errors);
            }

            Person? person = await _personsRepository.GetPersonById(personId);
            if (person == null)
            {
                throw new PersonNotFoundException("Person not found!!");
            }

            Person? p = await _personsRepository.GetPersonByEmail(personRequest?.Email);
            if (p != null && p.PersonId != personId)
            {
                throw new ArgumentException(nameof(personRequest.Email), "Email already exists");
            }

            p = await _personsRepository.GetPersonByMobile(personRequest?.PhoneNumber);
            if (p != null && p.PersonId != personId)
            {
                throw new ArgumentException(nameof(personRequest.PhoneNumber), "Phone number already exists");
            }

            CountryResponse? countryResponse = await _countriesGetterService.GetCountryById(personRequest.CountryId);
            if (countryResponse == null)
            {
                throw new ArgumentException(nameof(personRequest.CountryId), "Country not found");
            }

            Person personUpdated = await _personsRepository.UpdatePerson(personId, personRequest.ToPerson());

            return personUpdated.ToPersonResponse();
        }
    }
}
