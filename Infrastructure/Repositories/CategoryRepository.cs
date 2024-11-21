using Core.DTOs;
using Core.Entities;
using Core.Interfaces.Repositories;
using Core.Requests;
using Infrastructure.Context;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<CategoryDTO> Add(CreateCategoryRequest request, int authenticatedUserId)
    {       

        // Verificar si el usuario ya tiene una categoría con el mismo nombre
        bool categoryExists = await _context.Categories
            .AnyAsync(c => c.UserId == request.UserId && c.Name == request.Name);

        if (categoryExists)
        {
            throw new Exception($"El usuario con ID {request.UserId} ya tiene una categoría llamada '{request.Name}'.");
        }
        var categoryToCreate = request.Adapt<Category>();

        _context.Categories.Add(categoryToCreate);

        await _context.SaveChangesAsync();

        // Cargar la categoría recién creada con el usuario relacionado
        var createdCategory = await _context.Categories
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == categoryToCreate.Id);

        var categoryDTO = createdCategory.Adapt<CategoryDTO>();

        return categoryDTO;
    }

    public async Task<CategoryDTO> Update(UpdateCategoryRequest request, int authenticatedUserId)
    {
        var categoryToUpdate = request.Adapt<Category>();

        _context.Categories.Update(categoryToUpdate);

        await _context.SaveChangesAsync();

        var updatedCategory = await _context.Categories
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == categoryToUpdate.Id);

        var categoryDTO = updatedCategory.Adapt<CategoryDTO>();

        return categoryDTO;
    }

    public async Task<PaginationDTO<CategoryDTO>> List(PaginationRequest request, int authenticatedUserId)
    {
        // Filtrar las categorías del usuario autenticado
        var query = _context.Categories
            .Include(c => c.User) // Incluir la relación con el usuario
            .Where(c => c.UserId == authenticatedUserId);

        // Calcular el total de elementos para paginación
        var totalElements = await query.CountAsync();

        // Usar PageSize proporcionado por el usuario o valor por defecto (10)
        var pageSize = request.PageSize ?? 10;

        // Obtener el número de página actual (se asegura que no sea menor a 1)
        var currentPage = request.Page ?? 1;

        // Obtener los elementos paginados
        var categories = await query
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Mapear los datos a DTO
        var categoriesDTO = categories.Adapt<List<CategoryDTO>>();

        // Calcular el total de páginas en base al tamaño de página
        var totalPages = (int)Math.Ceiling((double)totalElements / pageSize);

        // Crear la respuesta con los datos de paginación
        var pagination = new PaginationDTO<CategoryDTO>
        {
            Data = categoriesDTO,
            Page = currentPage,
            PageSize = pageSize,
            TotalElements = totalElements,
            TotalPages = totalPages
        };

        return pagination;
    }

    public async Task<bool> VerifyCategoryExist(int categoryId)
    {
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == categoryId);

        return (categoryExists);
    }

    public async Task<bool> VerifyUserExist(int userId)
    {
        var userExists = await _context.Users.AnyAsync(c => c.Id == userId);

        return (userExists);
    }

    public async Task<bool> Delete(int categoryId, int authenticatedUserId)
    {
        var category = await _context.Categories
        .FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == authenticatedUserId);

        if (category == null)
        {
            return false; // Mensaje cuando no coincide
        }

        // Eliminar la categoría
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }
}
