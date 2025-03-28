using System.ComponentModel.DataAnnotations;

namespace AuthMicroservice.Shared.Dtos.Account;

    public record RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
