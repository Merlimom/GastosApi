using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Requests;

public class UpdateUserRequest
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public bool IsBlocked { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
}
