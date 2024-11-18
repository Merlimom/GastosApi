using Core.DTOs;
using Core.Encrypt;
using Core.Exceptions;
using Core.Interfaces.Services;
using Core.Requests;
using FluentValidation;
using GastosAPI.OptionsSetup;
using Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly JwtOptions _jwtOptions;



    public UserService(IUserRepository userRepository, IConfiguration configuration, IOptions<JwtOptions> jwtOptions)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<UserDTO> Add(CreateUserRequest request)
    {
        if (await _userRepository.ExistsByEmail(request.Email))
        {
            throw new BusinessLogicException("Ya existe un usuario con ese correo electrónico.");
        }

        request.Password = Encrypt.GetSHA256(request.Password);

        return await _userRepository.Add(request);
    }
    public async Task<string> RequestPasswordResetAsync(string email)
    {
        // verificar si el correo existe en la base de datos
        var user = await _userRepository.GetUserByEmail(email);
        if (user == null)
        {
            throw new NotFoundException("El correo no está registrado.");
        }

        // generar un jwt para el restablecimiento de contraseña
        var token = GeneratePasswordResetJwtToken(user.Email, user.Id);

        // guardar el token en la base de datos con fecha de expiración
        await _userRepository.SavePasswordResetTokenAsync(token);

        // devolver el token generado (en lugar de enviar un correo, por ahora devolvemos el token para pruebas)
        return token;
    }

    private string GeneratePasswordResetJwtToken(string email, int userId)
    {
        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        new Claim(ClaimTypes.Name, email),
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiration = DateTime.UtcNow.AddMinutes(10); // Expiración del token a 10 minutos

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiration,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public async Task ChangePasswordAsync(string token, string newPassword)
    {
        try
        {
            // Validar el token JWT y extraer el UserId
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["ForgotPasswordJwt:Secret"]);

            SecurityToken validatedToken;
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _configuration["ForgotPasswordJwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["ForgotPasswordJwt:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // Elimina el desfase de tiempo en la validación
            }, out validatedToken);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new BusinessLogicException("Token inválido o no contiene un identificador de usuario.");
            }

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                throw new BusinessLogicException("El identificador de usuario en el token no es válido.");
            }

            // Verificar si el token ya ha sido usado
            var resetToken = await _userRepository.GetPasswordResetTokenAsync(token);
            if (resetToken != null && resetToken.IsUsed)
            {
                throw new BusinessLogicException("Este token ya ha sido utilizado para restablecer la contraseña.");
            }

            // Validar la nueva contraseña
            var validator = new InlineValidator<string>();
            validator.RuleFor(password => password)
                .NotEmpty().WithMessage("La contraseña es obligatoria.")
                .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
                .Matches("[A-Z]").WithMessage("La contraseña debe contener al menos una letra mayúscula.")
                .Matches("[a-z]").WithMessage("La contraseña debe contener al menos una letra minúscula.")
                .Matches("[0-9]").WithMessage("La contraseña debe contener al menos un número.")
                .Matches("[^a-zA-Z0-9]").WithMessage("La contraseña debe contener al menos un carácter especial.");

            var validationResult = await validator.ValidateAsync(newPassword);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // Encriptar la nueva contraseña y actualizarla en la base de datos
            var encryptedNewPassword = Encrypt.GetSHA256(newPassword);
            var passwordUpdated = await _userRepository.UpdatePasswordAsync(userId, encryptedNewPassword);

            if (!passwordUpdated)
            {
                throw new BusinessLogicException("Error al actualizar la contraseña.");
            }

            // Invalidar el token después de usarlo
            await _userRepository.InvalidatePasswordResetTokenAsync(token);
        }
        catch (SecurityTokenExpiredException ex)
        {
            // Captura específica cuando el token ha expirado
            throw new BusinessLogicException("El token de restablecimiento de contraseña ha expirado. Por favor, solicite un nuevo restablecimiento.");
        }
        catch (Exception ex)
        {
            // Manejo de otros errores
            throw new BusinessLogicException($"Error al cambiar la contraseña: {ex.Message}");
        }
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
            throw new BusinessLogicException("Usuario o contraseña incorrectos.");

        // Validar la contraseña (supongamos que está cifrada)
        var encryptedPassword = Encrypt.GetSHA256(request.Password);

        if (user.Password != encryptedPassword)
            throw new BusinessLogicException("Usuario o contraseña incorrectos.");

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
