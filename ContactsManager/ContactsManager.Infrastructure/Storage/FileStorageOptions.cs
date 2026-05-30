namespace Storage
{
    public class FileStorageOptions
    {
        public string RootPath { get; set; } = string.Empty;

        public string PersonsFileName { get; set; } = "persons.json";

        public string CountriesFileName { get; set; } = "countries.json";

        public string UsersFileName { get; set; } = "users.json";

        public string RolesFileName { get; set; } = "roles.json";
    }
}