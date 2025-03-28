using System.ComponentModel.DataAnnotations;

namespace AuthMicroservice.Shared.Dtos.Account;
    public record LoginDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
