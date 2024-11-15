using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities;

public class PasswordResetToken
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;   
    public DateTime ExpirationDate { get; set; }
    public bool IsUsed { get; set; } = false;
}
