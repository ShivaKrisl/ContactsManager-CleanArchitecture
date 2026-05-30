namespace Entities_DTO
{
    public class AuthResult
    {
        public bool Succeeded { get; set; }

        public bool IsAdmin { get; set; }

        public List<string> Errors { get; set; } = new();
    }
}