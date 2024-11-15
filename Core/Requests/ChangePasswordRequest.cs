namespace Core.Requests;

public class ChangePasswordRequest
{
    public string Token { get; set; } = string.Empty;   
    public string NewPassword { get; set; } = string.Empty;
}
