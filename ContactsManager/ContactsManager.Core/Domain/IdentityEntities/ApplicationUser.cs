namespace ContactsManager.Core.Domain.IdentityEntities
{
    public class ApplicationUser
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? PasswordHash { get; set; }
        public List<string> Roles { get; set; } = new();

    }
}
