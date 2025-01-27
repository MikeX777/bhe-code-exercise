using LanguageExt;
using MediatR;
using Sieve.Interfaces;
using Sieve.Model;
using Sieve.Model.Api;
using Sieve.Model.Api.Requests;

namespace Sieve.Service.V1
{
    public class SieveHandler :
        IRequestHandler<PrimeCommand, Either<ApiProblemDetails, long>>
    {
        private readonly ISieve sieve;

        public SieveHandler(ISieve sieve) => this.sieve = sieve;

        public async Task<Either<ApiProblemDetails, long>> Handle(PrimeCommand request, CancellationToken cancellationToken) =>
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
