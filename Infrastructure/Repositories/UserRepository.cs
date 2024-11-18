using Core.DTOs;
using Core.Encrypt;
using Core.Entities;
using Core.Exceptions;
using Core.Requests;
using Infrastructure.Context;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
        var userToUpdate = await _context.Users.FindAsync(request.Id);

        if (userToUpdate is null) throw new Exception("El usuario especificado no existe");

        userToUpdate.Name = request.Username;
        userToUpdate.IsBlocked = request.IsBlocked;
        userToUpdate.UpdateDate = DateTime.UtcNow;


        _context.Users.Update(userToUpdate);
        await _context.SaveChangesAsync();
        var userDTO = userToUpdate.Adapt<UserDTO>();

        return userDTO;
    }

    public async Task SavePasswordResetTokenAsync(string token)
    {
        var expirationDate = DateTime.UtcNow.AddMinutes(10); // La expiración del JWT será de 10 minutos

        var resetToken = new PasswordResetToken
        {
            Token = token,
            ExpirationDate = expirationDate
        };

        _context.Tokens.Add(resetToken);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdatePasswordAsync(int userId, string newPassword)
    {
        // Buscar al usuario por ID
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return false;
        }

        // Actualizar la contraseña
        user.Password = newPassword;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return true;
    }

    //public async Task<bool> UpdatePasswordAsync(string token, string newPassword)
    //{
    //    //// 1. Buscar el token activo y no utilizado en la base de datos
    //    var resetToken = await _context.Tokens
    //    .FirstOrDefaultAsync(t => t.Token == token && t.ExpirationDate > DateTime.UtcNow && !t.IsUsed);

    //    if (resetToken == null)
    //    {
    //         return false;
    //    }

    //    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == Id);
    //    if (user == null)
    //    {
    //        return false;
    //    }

    //    user.Password = newPassword;
    //    _context.Users.Update(user);
    //    await _context.SaveChangesAsync();

    //    await InvalidatePasswordResetTokenAsync(token);

    //    return true;
    //}

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
        if (entity == null) throw new NotFoundException("No se encontro usuario con el id ingresado");
        return entity;
    }

    public async Task<PasswordResetToken?> GetPasswordResetTokenAsync(string token)
    {
        return await _context.Tokens
                             .FirstOrDefaultAsync(t => t.Token == token);
    }

    //public async Task<PasswordResetToken?> GetActivePasswordResetTokenAsync(string token)
    //{
    //    // Extraer UserId del token
    //    var userId = GetUserIdFromJwt(token);

    //    if (userId == null)
    //        return null;

    //    // Verificar si el token existe y está activo
    //    return await _context.Tokens
    //                         .Where(t => t.Token == token && t.ExpirationDate > DateTime.UtcNow && !t.IsUsed)
    //                         .FirstOrDefaultAsync();
    //}

    private int? GetUserIdFromJwt(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

        var userIdClaim = jwtToken?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier);

        return userIdClaim != null ? int.Parse(userIdClaim.Value) : (int?)null;
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
