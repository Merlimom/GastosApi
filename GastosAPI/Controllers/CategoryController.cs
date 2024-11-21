using Core.Entities;
using Core.Exceptions;
using Core.Interfaces.Services;
using Core.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApi.Controllers;

namespace GastosAPI.Controllers;

public class CategoryController : BaseApiController
{
    private readonly ICategoryService _service;

    public CategoryController(ICategoryService service)
    {
        _service = service;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        int authenticatedUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        return Ok(await _service.Add(request, authenticatedUserId));
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] UpdateCategoryRequest request)
    {
        int authenticatedUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        return Ok(await _service.Update(request, authenticatedUserId));
    }

    [HttpGet("List")]
    [Authorize]
    public async Task<IActionResult> List([FromQuery] PaginationRequest request)
    {
        //Obtener el UserId del token JWT

    var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized("No se pudo obtener el UserId del token.");
        }

        int authenticatedUserId = int.Parse(userIdClaim.Value);


        return Ok(await _service.List(request, authenticatedUserId));
        
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        // Verificar que el usuario esté autenticado
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized("No se pudo obtener el UserId del token.");
        }
        // Obtener el ID del usuario autenticado
        int authenticatedUserId = int.Parse(userIdClaim.Value);

        // Llamar al servicio para eliminar la categoría
        var result = await _service.Delete(id, authenticatedUserId);

        if (!result)
        {
            // Devolver un mensaje claro en caso de que no tenga acceso
            return NotFound(new { message = $"No tienes una categoría con ID {id}." });
        }

        // Retornar éxito si se eliminó correctamente
        return Ok(new { message = "Eliminación exitosa." });
    }

}
