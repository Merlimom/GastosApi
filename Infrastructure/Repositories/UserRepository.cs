using Core.DTOs;
using Core.Entities;
using Core.Exceptions;
using Core.Requests;
using Infrastructure.Context;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<UserDTO> Add(CreateUserRequest request)
    {
        var userToCreate = request.Adapt<User>();

        _context.Users.Add(userToCreate);

        await _context.SaveChangesAsync();

        var userDTO = userToCreate.Adapt<UserDTO>();

        return userDTO;
    }

    public async Task<bool> ChangePassword(int userId, string hashedNewPassword)
    {
            var user = await _context.Users.FindAsync(userId);
            if (user is null) throw new Exception("User was not found");

            user.Password = hashedNewPassword;
            user.UpdateDate = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
    
    }
    public async Task<UserDTO> Update(UpdateUserRequest request)
    {
        var user = await _context.Users.FindAsync(request.Id);

        if (user is null) throw new Exception("User was not found");

        user.Name = request.Username ?? user.Name;

        user.IsBlocked = request.IsBlocked ?? false;  // Si no es nulo, usa el valor de request.IsBlocked, de lo contrario, asigna false
        user.IsDeleted = request.IsDeleted ?? false;
        user.UpdateDate = DateTime.UtcNow;


        // Actualizamos la entidad en el contexto
        _context.Users.Update(user);

        // Guardamos los cambios en la base de datos
        await _context.SaveChangesAsync();

        // Mapear el usuario actualizado a DTO
        var userDTO = user.Adapt<UserDTO>();

        return userDTO;
    }

    public async Task<User?> GetUserById(int userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<bool> ExistsByEmail(string email)
    {
        return await _context.Users.AnyAsync(user => user.Email == email);
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    private async Task<User> VerifyExists(int id)
    {
        var entity = await _context.Users.FindAsync(id);
        if (entity == null) throw new Exception("No se encontro usuario con el id ingresado");
        return entity;
    }


    //public async Task<UserDTO> VerifyExists(int id)
    //{
    //    var entity = await _context.Users.FindAsync(id);

    //    if (entity is null) throw new BusinessLogicException($"User with id: {id} doest not exist");

    //    return entity.Adapt<UserDTO>();
    //}

}
