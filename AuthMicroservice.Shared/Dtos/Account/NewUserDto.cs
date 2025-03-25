namespace AuthMicroservice.Shared.Dtos.Account;

public record NewUserDto
{
    public string Email { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}