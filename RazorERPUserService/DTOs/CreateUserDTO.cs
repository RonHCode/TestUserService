namespace RazorERPUserService.DTOs
{
    public class CreateUserDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } // 'Admin' or 'User'
        public string? Email { get; set; }

        public int CompanyID { get; set; }

    }
}
