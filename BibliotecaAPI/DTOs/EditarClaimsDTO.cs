using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs
{
    public class EditarClaimsDTO
    {
        [EmailAddress]
        [Required]
        public required string Email { get; set; }
    }
}
