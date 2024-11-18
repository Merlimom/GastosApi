using Core.Interfaces.Services;
using GastosAPI.OptionsSetup;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services;

public class JwtProvider : IJwtProvider
{
    private readonly JwtOptions _jwtSettings;

    public JwtProvider(IOptions<JwtOptions> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }


    public string Generate()
    {
        var roles = new List<string> { "User" };

        var claims = new List<Claim>();

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }


        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var signInCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
                _jwtSettings.Issuer,
                _jwtSettings.Audience,
                claims,
                null,
                DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                signInCredentials
            );

        string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        return tokenValue;
    }
}
