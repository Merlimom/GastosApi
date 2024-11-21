using Core.DTOs;
using Core.Exceptions;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Requests;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryDTO> Add(CreateCategoryRequest request, int authenticatedUserId)
    {
        var userExists = await _categoryRepository.VerifyUserExist(request.UserId);

        if (!userExists)
        {
            throw new BusinessLogicException($"User {request.UserId} does not exist.");
        }

        // Validar que el usuario autenticado esté creando la categoría para sí mismo
        if (request.UserId != authenticatedUserId)
        {
            throw new UnauthorizedAccessException("No puedes crear categorías para otro usuario.");
        }

        return await _categoryRepository.Add(request, authenticatedUserId);
    }

    public async Task<CategoryDTO> Update(UpdateCategoryRequest request, int authenticatedUserId)
    {
        var categoryExists = await _categoryRepository.VerifyCategoryExist(request.Id);

        if (!categoryExists)
        {
            throw new BusinessLogicException($"Category {request.Id} does not exist.");
        }

        // Validar que el usuario autenticado esté creando la categoría para sí mismo
        if (request.UserId != authenticatedUserId)
        {
            throw new UnauthorizedAccessException("No puedes crear categorías para otro usuario.");
        }
        return await _categoryRepository.Update(request, authenticatedUserId);
    }

    public async Task<PaginationDTO<CategoryDTO>> List(PaginationRequest request, int authenticatedUserId)
    {
        return await _categoryRepository.List(request, authenticatedUserId);
    }

    public async Task<bool> Delete(int categoryId, int authenticatedUserId)
    {
        return await _categoryRepository.Delete(categoryId, authenticatedUserId);

    }
}
