using Core.DTOs;
using Core.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<CategoryDTO> Add(CreateCategoryRequest request, int authenticatedUserId);
    Task<bool>VerifyUserExist(int userId);
    Task<bool> VerifyCategoryExist(int categoryId);
    Task<CategoryDTO> Update(UpdateCategoryRequest request, int authenticatedUserId);
    Task<PaginationDTO<CategoryDTO>> List(PaginationRequest request, int authenticatedUserId);

    Task<bool> Delete(int categoryId, int authenticatedUserId);

}
