using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Api.Abstractions;
using Sieve.Model.Api;
using Sieve.Model.Api.Requests;
using Swashbuckle.AspNetCore.Annotations;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Sieve.Api.Controllers.V1
{
    /// <summary>
    /// A Controller used interact with primes
    /// </summary>
    /// <remarks>
    /// Constructor for <see cref="PrimeController"/>.
    /// </remarks>
    /// <param name="mediator">The mediator instance used to send commands.</param>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    [ProducesResponseType(Status401Unauthorized, Type = typeof(ApiProblemDetails))]
    [ProducesResponseType(Status403Forbidden, Type = typeof(ApiProblemDetails))]
    [ProducesResponseType(Status404NotFound, Type = typeof(ApiProblemDetails))]
    [ProducesResponseType(Status503ServiceUnavailable, Type = typeof(ApiProblemDetails))]
    [ProducesResponseType(Status504GatewayTimeout, Type = typeof(ApiProblemDetails))]
    [ProducesResponseType(Status500InternalServerError, Type = typeof(ApiProblemDetails))]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class PrimeController(IMediator mediator) : RespondController
    {
        private readonly IMediator mediator = mediator;

        /// <summary>
        /// An endpoint to get the Nth prime number.
        /// </summary>
        /// <param name="n">The nth index to find the prime number for.</param>
        /// <returns>The value of the nth prime number.</returns>
        [HttpGet("nth/{n}")]
        [SwaggerOperation("An action used to retrieve the nth prime number.")]
        [ProducesResponseType(Status200OK, Type = typeof(long))]
        public async Task<IActionResult> GetNthPrimeAction(long n) =>
            Respond(await mediator.Send(new PrimeCommand { N = n }));
    }
}
