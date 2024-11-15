using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities;

public class User
{
    public int Id { get; set; } 
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string .Empty;
    public DateTime CreationDate { get; set; }
    public DateTime UpdateDate { get; set; }
    public bool IsDeleted { get; set; } = false;
    public bool IsBlocked { get; set; } = false;

    public ICollection<Category> Categories = new List<Category>();
    public ICollection<Expense> Expenses = new List<Expense>();


}
