using LanguageExt;
using MediatR;
using Sieve.Interfaces;
using Sieve.Model;
using Sieve.Model.Api;

namespace Sieve.Service.V1
{
    /// <summary>
    /// A command to get the nth prime number.
    /// </summary>
    /// <param name="N">The index used to find the nth prime number.</param>
    public record GetNthPrime(long N) : IRequest<Either<ApiProblemDetails, long>>;

    public class SieveHandler :
        IRequestHandler<GetNthPrime, Either<ApiProblemDetails, long>>
    {
        private readonly ISieve sieve;

        public SieveHandler(ISieve sieve) => this.sieve = sieve;

        public async Task<Either<ApiProblemDetails, long>> Handle(GetNthPrime request, CancellationToken cancellationToken) =>
            await (
                from n in Common.MapLeft(() => TryRun(() => sieve.NthPrime(request.N))).ToAsync()
                select n);


        private Either<Error, T> TryRun<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch (Exception e)
            {
                return Error.Create(ErrorSource.SieveApi,
                    System.Net.HttpStatusCode.InternalServerError,
                    e.Message,
                    ErrorType.System);
            }
        }
    }
}
