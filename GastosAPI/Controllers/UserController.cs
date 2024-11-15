using Core.Exceptions;
using Core.Interfaces.Services;
using Core.Requests;
using Infrastructure.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WebApi.Controllers;

namespace GastosAPI.Controllers;

public class UserController : BaseApiController
{
    private readonly IUserService _service;

    public UserController(IUserService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        return Ok(await _service.Add(request));
    }

    [HttpPost("ChangePassword")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var result = await _service.ChangePassword(request.UserId, request.CurrentPassword, request.NewPassword);

        if (!result)
        {
            throw new BusinessLogicException("La contraseña actual es incorrecta");
        }

        return Ok("Contraseña cambiada exitosamente.");
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateUserRequest request)
    {
        return Ok(await _service.Update(request));
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var token = await _service.Login(request);
            return Ok(new { Token = token });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { Message = ex.Message });
        }
    }
}
