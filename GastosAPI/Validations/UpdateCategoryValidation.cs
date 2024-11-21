using Core.Requests;
using FluentValidation;

namespace GastosAPI.Validations;

public class UpdateCategoryValidation : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryValidation()
    {
        RuleFor(x => x.Name)
       .NotNull().WithMessage("Name cannot be null")
       .NotEmpty().WithMessage("Name cannot be empty");

        RuleFor(x => x.Description)
       .NotNull().WithMessage("Description cannot be null")
       .NotEmpty().WithMessage("Description cannot be empty");
    }
}
