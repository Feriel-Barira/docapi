namespace DocApi.DTOs
{
   
    public class CreateUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string RoleGlobal { get; set; } = "UTILISATEUR";
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Fonction { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

   
    public class UpdateUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleGlobal { get; set; } = "UTILISATEUR";
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Fonction { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }


}