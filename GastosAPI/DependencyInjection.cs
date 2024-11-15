using FluentValidation.AspNetCore;
using FluentValidation;
using System.Reflection;
using Core.Requests;
using GastosAPI.Validations;
using GastosAPI.OptionsSetup;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace GastosAPI;

public static class DependencyInjection
{
    public static IServiceCollection AddWebApi(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddValidation();
        services.SetAuthentication(configuration);



        return services;
    }
    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddFluentValidation();
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
    private static IServiceCollection SetAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.ConfigureOptions<JwtOptionsSetup>();
        services.ConfigureOptions<JwtBearerOptionsSetup>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }
}
