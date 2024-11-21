using Core.DTOs;
using Core.Requests;

namespace Core.Interfaces.Services;

public interface ICategoryService
{
    Task<CategoryDTO> Add(CreateCategoryRequest request, int authenticatedUserId);
    Task<CategoryDTO> Update(UpdateCategoryRequest request, int authenticatedUserId);
    Task<PaginationDTO<CategoryDTO>> List(PaginationRequest request, int authenticatedUserId);
    Task<bool> Delete(int categoryId, int authenticatedUserId);


}
