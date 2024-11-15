using Core.Requests;
using FluentValidation;

namespace GastosAPI.Validations;

public class CreateUserRequestValidation : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidation()
    {
        RuleFor(x => x.Name)
        .NotNull().WithMessage("Name cannot be null")
        .NotEmpty().WithMessage("Name cannot be empty");

        RuleFor(x => x.Email)
          .EmailAddress();

        RuleFor(x => x.Password)
          .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("La contraseña debe contener al menos una letra mayúscula.")
            .Matches("[a-z]").WithMessage("La contraseña debe contener al menos una letra minúscula.")
            .Matches("[0-9]").WithMessage("La contraseña debe contener al menos un número.")
            .Matches("[^a-zA-Z0-9]").WithMessage("La contraseña debe contener al menos un carácter especial.");
    }
}

