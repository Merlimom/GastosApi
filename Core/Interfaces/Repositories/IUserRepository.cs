using Core.DTOs;
using Core.Entities;
using Core.Requests;

namespace Infrastructure.Repositories;

public interface IUserRepository
{
    Task<UserDTO> Add(CreateUserRequest request);
    Task<UserDTO> Update(UpdateUserRequest request);
    Task<bool> ExistsByEmail(string email);
    Task<User?> GetUserByEmail(string email);
    Task<User?> GetUserById(int userId);//investigar pq usa user? 
    Task SavePasswordResetTokenAsync(string token);
    Task<PasswordResetToken?> GetPasswordResetTokenAsync(string token);
    Task InvalidatePasswordResetTokenAsync(string token);
    Task<bool> UpdatePasswordAsync(int userId, string newPassword);
}