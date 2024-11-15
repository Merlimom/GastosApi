using Core.DTOs;
using Core.Entities;
using Core.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Services;

public interface IUserService
{
    Task<UserDTO> Add(CreateUserRequest request);
    Task<UserDTO> Update(UpdateUserRequest request);
    Task<bool> ChangePassword(int userId, string currentPassword, string newPassword);
    Task<string> Login(LoginRequest request);

}
