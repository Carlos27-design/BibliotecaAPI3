using Microsoft.AspNetCore.Identity;

namespace BibliotecaAPI.Enitities
{
    public class Usuario : IdentityUser
    {
        public DateTime FechaNacimiento { get; set; }
    }
}
