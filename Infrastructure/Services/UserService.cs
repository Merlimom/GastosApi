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
    private readonly IEmailService _emailService;


    public UserService(IUserRepository userRepository, IConfiguration configuration, IEmailService emailService)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _emailService = emailService;

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
    public async Task<string> RequestPasswordResetAsync(string email)
    {
        // Verificar si el correo existe en la base de datos
        var userExists = await _userRepository.ExistsByEmail(email);
        if (!userExists)
        {
            throw new Exception("El correo no está registrado.");
        }

        // Verificar si el correo ya tiene un token activo no expirado
        var existingToken = await _userRepository.GetActivePasswordResetTokenAsync(email);
        if (existingToken != null && existingToken.ExpirationDate > DateTime.UtcNow)
        {
            throw new Exception("Ya existe una solicitud de restablecimiento de contraseña activa para este correo.");
        }

        // Generar un token único para el restablecimiento
        var token = Guid.NewGuid().ToString();

        // Guardar el token en la base de datos con fecha de expiración
        await _userRepository.SavePasswordResetTokenAsync(email, token);

        // Aquí, en lugar de enviar el correo, simplemente devolvemos el token para pruebas
        return token;
    }

    public async Task ChangePasswordAsync(string token, string newPassword)
    {
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

        var encryptedNewPassword = Encrypt.GetSHA256(newPassword);

        // Llamar al repositorio para actualizar la contraseña
        var passwordUpdated = await _userRepository.UpdatePasswordAsync(token, encryptedNewPassword);

        if (!passwordUpdated)
        {
            throw new Exception("Error al actualizar la contraseña.");
        }

        await _userRepository.InvalidatePasswordResetTokenAsync(token);
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
