namespace RazorERPUserService.DTOs
{
    public class UserDTO
    {
        public int Id { get; set; }               
        public string Username { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }        
        public string Role { get; set; }
        public int CompanyID { get; set; }

        public DateTime DateCreated { get; set; }  
        public DateTime DateUpdated { get; set; }  

    }
}
