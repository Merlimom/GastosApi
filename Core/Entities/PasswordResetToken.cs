namespace Core.Entities;

public class PasswordResetToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;   
    public DateTime ExpirationDate { get; set; }
    public bool IsUsed { get; set; } = false;
}
