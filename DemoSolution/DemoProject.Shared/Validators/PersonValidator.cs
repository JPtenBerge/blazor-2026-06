using DemoProject.Shared.Entities;
using FluentValidation;

namespace DemoProject.Shared.Validators;

public class PersonValidator : AbstractValidator<Person>
{
    public PersonValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Vul in aub");
        RuleFor(x => x.Age).InclusiveBetween(12, 67).WithMessage("Te jong of te oud!");

        When(x => x.Age > 20, () =>
        {
            RuleFor(x => x.PhotoUrl).NotEmpty().WithMessage("Ouder dan 20 = foto graag");
        });
    }
}
