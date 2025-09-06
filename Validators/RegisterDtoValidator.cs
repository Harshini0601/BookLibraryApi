using BookLibraryApi.DTOs;
using FluentValidation;
namespace BookLibraryApi.Validators;
public class RegisterDtoValidator : AbstractValidator<RegisterDto>

{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}