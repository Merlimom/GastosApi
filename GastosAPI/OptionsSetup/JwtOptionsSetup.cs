using Microsoft.Extensions.Options;

namespace GastosAPI.OptionsSetup;

public class JwtOptionsSetup : IConfigureOptions<JwtOptions>
{
    private readonly IConfiguration _configuration;

    public JwtOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(JwtOptions options)
    {
        _configuration.GetSection("Jwt").Bind(options);
        _configuration.GetSection("ForgotPasswordJwt").Bind(options);

    }
}
