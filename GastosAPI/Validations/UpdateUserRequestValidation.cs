using Core.Requests;
using FluentValidation;

namespace GastosAPI.Validations;

public class UpdateUserRequestValidation : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidation()
    {
        RuleFor(x => x.Username)
       .NotNull().WithMessage("Name cannot be null")
       .NotEmpty().WithMessage("Name cannot be empty");

        RuleFor(x => x.IsBlocked)
       .Must(x => x == true || x == false).WithMessage("IsBlocked must be true or false");

        RuleFor(x => x.IsDeleted)
        .Must(x => x == true || x == false).WithMessage("IsDeleted must be true or false");


    }
}
