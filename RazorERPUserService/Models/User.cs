namespace RazorERPUserService.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Password_Hash { get; set; }
        public string Role { get; set; }    
        public int CompanyID { get; set; }
        public string Email { get; set; }
        public DateTime DateCreated { get; set; }     
        public DateTime DateUpdated { get; set; }   
    }
}
