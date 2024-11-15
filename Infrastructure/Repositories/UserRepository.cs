using Core.DTOs;
using Core.Encrypt;
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

    public async Task SavePasswordResetTokenAsync(string email, string token)
    {
        var expirationDate = DateTime.UtcNow.AddMinutes(10); // El token expirará en 1 hora

        var resetToken = new PasswordResetToken
        {
            Email = email,
            Token = token,
            ExpirationDate = expirationDate
        };

        _context.Tokens.Add(resetToken);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdatePasswordAsync(string token, string newPassword)
    {
        // 1. Buscar el token activo y no utilizado en la base de datos
        var resetToken = await _context.Tokens
            .FirstOrDefaultAsync(t => t.Token == token && t.ExpirationDate > DateTime.UtcNow && !t.IsUsed);

        if (resetToken == null)
        {
            return false;
        }
        // 2. Obtener el usuario asociado al correo electrónico del token
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == resetToken.Email);

        if (user == null)
        {
            // No se encuentra el usuario, no hay nada que actualizar
            return false;
        }

        // 3. Encriptar la nueva contraseña (asumimos que 'Encrypt.GetSHA256' es tu lógica de encriptado)
        user.Password = Encrypt.GetSHA256(newPassword);

        // 4. Actualizar la contraseña del usuario en la base de datos
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        // 5. Invalidar el token después de haberlo usado
        await InvalidatePasswordResetTokenAsync(token);

        return true;
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

    public async Task<PasswordResetToken?> GetActivePasswordResetTokenAsync(string email)
    {
        // Buscar un token activo y no usado para el correo
        return await _context.Tokens
                             .Where(t => t.Email == email && t.ExpirationDate > DateTime.UtcNow && !t.IsUsed)
                             .FirstOrDefaultAsync();
    }

    public async Task InvalidatePasswordResetTokenAsync(string token)
    {
        var resetToken = await _context.Tokens
                                       .FirstOrDefaultAsync(t => t.Token == token);

        if (resetToken != null)
        {
            resetToken.IsUsed = true; // Marcamos el token como usado
            _context.Tokens.Update(resetToken);
            await _context.SaveChangesAsync();
        }
    }

}
