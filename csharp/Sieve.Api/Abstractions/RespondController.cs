using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using Sieve.Model.Api;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Sieve.Api.Abstractions
{
    /// <summary>
    /// A controller used to handle the response created by other controllers.
    /// </summary>
    public class RespondController : ControllerBase
    {
        /// <summary>
        /// A general respond handler used to return an <see cref="IActionResult"/> from a controller.
        /// </summary>
        /// <typeparam name="TO">The "right" type to respond.</typeparam>
        /// <param name="either">The either that can be in either an error or right state.</param>
        /// <param name="mapRight">A handler to be able to override the default functionality of returning an Ok.</param>
        /// <returns>An <see cref="IActionResult"/>.</returns>
        protected IActionResult Respond<TO>(Either<ApiProblemDetails, TO> either, Func<TO, IActionResult>? mapRight = null) =>
            either.Match(
                Right: r => mapRight == null ? Ok(r) : mapRight(r),
                Left: e =>
                    e.Status switch
                    {
                        400 => BadRequest(e),
                        404 => NotFound(e),
                        503 => StatusCode(Status503ServiceUnavailable, e),
                        _ => StatusCode(Status500InternalServerError, e),
                    });
    }
}
