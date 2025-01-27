using LanguageExt;
using MediatR;

namespace Sieve.Model.Api.Requests
{
    /// <summary>
    /// A command to get the nth prime number.
    /// </summary>
    public class PrimeCommand : IRequest<Either<ApiProblemDetails, long>>
    {
        public long N { get; set; }
    }
}
