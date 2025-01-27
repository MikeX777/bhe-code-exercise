using FluentValidation;
using Sieve.Model.Api.Requests;

namespace Sieve.Model.Api.Validators
{
    public class PrimeCommandValidator : AbstractValidator<PrimeCommand>
    {
        public PrimeCommandValidator()
        {
            RuleFor(x => x.N).GreaterThanOrEqualTo(0)
                .WithMessage("Negative Indexed Prime Numbers are not Allowed.");
        }
    }
}
