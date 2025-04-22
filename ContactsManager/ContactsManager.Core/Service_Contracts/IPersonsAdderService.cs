using Entities_DTO;

namespace Service_Contracts
{
    public interface IPersonsAdderService
    {
        /// <summary>
        /// Add a new Person
        /// </summary>
        /// <param name="personRequest"></param>
        /// <returns></returns>
        public Task<PersonResponse> AddPerson(PersonRequest personRequest);
    }
}
