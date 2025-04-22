﻿using Entities_DTO;

namespace Service_Contracts
{
    public interface IPersonsUpdaterService
    {
        /// <summary>
        /// Update a Person by Person Id
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="personRequest"></param>
        /// <returns></returns>
        public Task<PersonResponse?> UpdatePerson(Guid personId, PersonRequest personRequest);
    }
}
