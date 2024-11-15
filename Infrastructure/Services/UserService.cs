using Core.DTOs;
using Core.Encrypt;
using Core.Interfaces.Services;
using Core.Requests;
using FluentValidation;
using Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;


    public UserService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;

    }

    public async Task<UserDTO> Add(CreateUserRequest request)
    {
        if (await _userRepository.ExistsByEmail(request.Email))
        {
            throw new Exception("Ya existe un usuario con ese correo electrónico.");
        }

        request.Password = Encrypt.GetSHA256(request.Password);

        return await _userRepository.Add(request);
    }

    public async Task<bool> ChangePassword(int userId, string currentPassword, string newPassword)
    {
        // Obtener el usuario por ID
        var user = await _userRepository.GetUserById(userId);
        if (user == null)
            throw new Exception("No se encontró un usuario con el ID ingresado.");

        var validator = new InlineValidator<string>();
        validator.RuleFor(password => password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("La contraseña debe contener al menos una letra mayúscula.")
            .Matches("[a-z]").WithMessage("La contraseña debe contener al menos una letra minúscula.")
            .Matches("[0-9]").WithMessage("La contraseña debe contener al menos un número.")
            .Matches("[^a-zA-Z0-9]").WithMessage("La contraseña debe contener al menos un carácter especial.");

        // Validar la nueva contraseña
        var validationResult = await validator.ValidateAsync(newPassword);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

       
        // Verificar si la contraseña actual coincide
        var encryptedCurrentPassword = Encrypt.GetSHA256(currentPassword);
        if (user.Password != encryptedCurrentPassword) return false;

        // Encriptar la nueva contraseña
        var encryptedNewPassword = Encrypt.GetSHA256(newPassword);

        // Actualizar la contraseña en el repositorio
        return await _userRepository.ChangePassword(userId, encryptedNewPassword);
    }

    public async Task<UserDTO> Update(UpdateUserRequest request)
    {
        // Llamar al repositorio para actualizar el usuario
        return await _userRepository.Update(request);
    }

    public async Task<string> Login(LoginRequest request)
    {
        // Obtener el usuario por correo electrónico
        var user = await _userRepository.GetUserByEmail(request.Email);

        if (user == null)
            throw new Exception("Usuario o contraseña incorrectos.");

        // Validar la contraseña (supongamos que está cifrada)
        var encryptedPassword = Encrypt.GetSHA256(request.Password);

        if (user.Password != encryptedPassword)
            throw new Exception("Usuario o contraseña incorrectos.");

        // Crear el token JWT
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["Jwt:ExpirationInMinutes"])),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return tokenString;
    }
}
