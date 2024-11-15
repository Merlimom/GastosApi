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
    Task<bool> ChangePassword(int userId, string hashedNewPassword);
    Task<User?> GetUserById(int userId);//investigar pq usa user? 
}