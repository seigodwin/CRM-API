namespace CRMApi.Domain.DTOs
{
    public class ResetPasswordDTO
    {
            public required string Email { get; set; }
            public string? OTP { get; set; }
            public string? NewPassword { get; set; }
            public string? ConfirmPassword { get; set; }
    }
}
