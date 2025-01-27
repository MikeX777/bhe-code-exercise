using LanguageExt;
using Sieve.Model;
using Sieve.Model.Api;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Sieve.Service.V1
{
    public static class Common
    {
        private const string GENERIC_ERROR_TITLE = "Error(s) processing request";
        private const string INTERNAL_SERVER_ERROR_TITLE = "Internal Server Error";

        /// <summary>
        /// A static method to convert an <see cref="Error"/> to an <see cref="ApiProblemDetails"/>.
        /// </summary>
        /// <param name="error">The error to map to <see cref="ApiProblemDetails"/>.</param>
        /// <param name="origination">The origination used to describe where in the api the error occurred.</param>
        /// <returns>An <see cref="ApiProblemDetails"/>.</returns>
        public static ApiProblemDetails ToProblemDetails(this Error error,
            ApiProblemDetailsOrigination origination = ApiProblemDetailsOrigination.Unspecified) =>
            ApiProblemDetails.Create(GENERIC_ERROR_TITLE, (int)error.StatusCode, errors: error.Faults.Map(e => new Fault
            {
                ErrorSource = e.ErrorSource,
                ErrorType = e.ErrorType,
                Field = e.Field,
                Message = e.Message,
            }), error.RequestId.ToString(), origination);

        internal static ApiProblemDetails ToSystemErrorProblemsDetails(
            this Exception exception,
            ErrorSource errorSource = ErrorSource.SieveApi,
            ErrorType errorType = ErrorType.System) =>
            ApiProblemDetails.Create(INTERNAL_SERVER_ERROR_TITLE, Status500InternalServerError, exception.Message, errorSource, errorType,
                string.Empty);

        /// <summary>
        /// A method used to handle an either and map the left value to <see cref="ApiProblemDetails"/>.
        /// </summary>
        /// <typeparam name="TR">The right return type.</typeparam>
        /// <param name="f">The method to run and map the return of.</param>
        /// <param name="origination">The origination of the error, used to describe where it occurred in the API.</param>
        /// <returns>An <see cref="ApiProblemDetails"/>.</returns>
        public static Either<ApiProblemDetails, TR> MapLeft<TR>(Func<Either<Error, TR>> f,
            ApiProblemDetailsOrigination origination = ApiProblemDetailsOrigination.Unspecified) =>
            f().MapLeft(e => e.ToProblemDetails(origination));

        /// <summary>
        /// An asynchronous method used to handle an either and map the left value to <see cref="ApiProblemDetails"/>.
        /// </summary>
        /// <typeparam name="TR">The right return type.</typeparam>
        /// <param name="f">The method to run and map the return of.</param>
        /// <param name="origination">The origination of the error, used to describe where it occurred in the API.</param>
        /// <returns>An <see cref="ApiProblemDetails"/>.</returns>
        public static async Task<Either<ApiProblemDetails, TR>> MapLeft<TR>(Func<Task<Either<Error, TR>>> f,
            ApiProblemDetailsOrigination origination = ApiProblemDetailsOrigination.Unspecified) =>
            (await f()).MapLeft(er => er.ToProblemDetails(origination));
    }
}
