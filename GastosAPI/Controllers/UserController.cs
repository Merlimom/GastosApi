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

    //si va a enviarse el correo
    //[HttpPost("request-password-reset")]
    //public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequestEmail request)
    //{
    //    try
    //    {
    //        await _service.RequestPasswordResetAsync(request.Email);
    //        return Ok("Correo de restablecimiento de contraseña enviado.");
    //    }
    //    catch (Exception ex)
    //    {
    //        return BadRequest(ex.Message);
    //    }
    //}

    [HttpPost("request-password-reset")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequestEmail request)
    {
        try
        {
            // Llamar al servicio que maneja la solicitud de restablecimiento de contraseña
            var token = await _service.RequestPasswordResetAsync(request.Email);

            // Devolver el token JWT como respuesta para pruebas (en lugar de enviar el correo)
            return Ok(new { Token = token });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            // Llamar al servicio para cambiar la contraseña
            await _service.ChangePasswordAsync(request.Token, request.NewPassword);
            return Ok("Contraseña actualizada con éxito.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message); // Devuelve el error si ocurre alguna excepción
        }
        //Console.WriteLine($"http//localsk:api/user/change-password/{token}");
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
