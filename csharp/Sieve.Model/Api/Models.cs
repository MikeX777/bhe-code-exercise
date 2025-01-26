using FluentValidation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Sieve.Model.Constants;
using System.Net;

namespace Sieve.Model.Api
{
    public class ProblemDetails : Microsoft.AspNetCore.Mvc.ProblemDetails
    {
        /// <summary>
        /// A short Title of the problem details.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Order = 0)]
        public new string? Title { get; set; }
        /// <summary>
        /// A URI reference [RFC3986] that identifies the problem type.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Order = 1)]
        public new string? Type { get; set; }
        /// <summary>
        /// The int status code of the response.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Order = 2)]
        public new int? Status { get; set; }
        /// <summary>
        /// Human readable detail of the problem.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Order = 3)]
        public new string? Detail { get; set; }
        /// <summary>
        /// URI of the specific occurrence of the problem.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public new string? Instance { get; set; }
    }

    /// <summary>
    /// An API specific version of ProblemDetails with additional fields.
    /// </summary>
    public class ApiProblemDetails : ProblemDetails
    {
        /// <summary>
        /// A identifier for request.
        /// </summary>
        public string? RequestId { get; set; }
        /// <summary>
        /// A field to describe where in the API the this problem originated.
        /// </summary>
        public ApiProblemDetailsOrigination Origination { get; set; } = ApiProblemDetailsOrigination.Unspecified;
        /// <summary>
        /// A collection of errors that occurred with this problem.
        /// </summary>
        public ICollection<Fault> Errors { get; set; } = [];

        /// <summary>
        /// A static method to create a <see cref="ApiProblemDetails"/> instance.
        /// </summary>
        /// <param name="detail">A string detail that is used for the ApiProblemDetails.</param>
        /// <param name="status">A HttpStatusCode that is related to the problem.</param>
        /// <param name="message">A message with the error.</param>
        /// <param name="errorSource">The source of the error, such as which API had an issue.</param>
        /// <param name="errorType">A typing of the error.</param>
        /// <param name="requestId">The identifier used to specify a request.</param>
        /// <param name="origination">An origination of where in the API a this error occurred.</param>
        /// <returns>The instantiated <see cref="ApiProblemDetails"/>.</returns>
        public static ApiProblemDetails Create(string detail = "", int status = -1, string message = "", ErrorSource errorSource = ErrorSource.Unspecified,
            ErrorType errorType = ErrorType.Unspecified, string requestId = "",
            ApiProblemDetailsOrigination origination = ApiProblemDetailsOrigination.Unspecified) =>
            Create(detail, status, new List<Fault>
            {
                new()
                {
                    ErrorSource = errorSource,
                    ErrorType = errorType,
                    Message = message,
                }
            }, requestId, origination);

        /// <summary>
        /// A method used to instantiate a <see cref="ApiProblemDetails"/>.
        /// </summary>
        /// <param name="detail">A detail used to describe the problem.</param>
        /// <param name="status">An HttpStatusCode that pertains to the specific error.</param>
        /// <param name="errors">A collection of <see cref="Fault"/> that a specific to this problem.</param>
        /// <param name="requestId">An identifier used to identifier this specific request.</param>
        /// <param name="origination">An origination of where in the API this error occurred.</param>
        /// <returns>The instantiated <see cref="ApiProblemDetails"/>.</returns>
        public static ApiProblemDetails Create(string detail, int status, IEnumerable<Fault> errors, string requestId = "",
            ApiProblemDetailsOrigination origination = ApiProblemDetailsOrigination.Unspecified) =>
            new()
            {
                Detail = detail,
                Status = status,
                RequestId = requestId,
                Origination = origination,
                Errors = errors.ToList()
            };
    }

    /// <summary>
    /// A validation specific version of the ApiProblemDetails.
    /// </summary>
    public class ApiValidationProblemDetails : ApiProblemDetails
    {
        /// <summary>
        /// A constructor to create an ApiValidationProblemDetails
        /// </summary>
        /// <param name="dictionary">A dictionary of invalid model state.</param>
        /// <param name="errorSource">The source api of the error.</param>
        public ApiValidationProblemDetails(ModelStateDictionary dictionary, ErrorSource errorSource)
        {
            foreach (var d in dictionary.Where(kv => kv.Value?.ValidationState == ModelValidationState.Invalid))
            {
                foreach (var e in d.Value!.Errors)
                {
                    Errors.Add(new Fault
                    {
                        Field = d.Key,
                        ErrorSource = errorSource,
                        Message = e.ErrorMessage,
                        ErrorType = ErrorType.Validation,
                    });
                }
            }
            Title = ErrorConstants.ProblemDetails.ValidationError;
            Detail = ErrorConstants.ProblemDetails.ReferToErrorsPropertyForDetails;
            Status = (int)HttpStatusCode.BadRequest;
            Origination = ApiProblemDetailsOrigination.ValidationErrors;
        }

        /// <summary>
        /// Creates an instance of <see cref="ApiValidationProblemDetails"/> using <see cref="ValidationException"/>.
        /// </summary>
        /// <param name="validationException">The exception instance used to build the problem details.</param>
        /// <param name="errorSource">The source api of the error.</param>
        public ApiValidationProblemDetails(ValidationException validationException, ErrorSource errorSource)
        {
            foreach (var error in validationException.Errors)
            {
                Errors.Add(new Fault
                {
                    Field = error.PropertyName,
                    ErrorSource = errorSource,
                    Message = error.ErrorMessage,
                    ErrorType = ErrorType.Validation
                });
            }
            Title = ErrorConstants.ProblemDetails.ValidationError;
            Detail = ErrorConstants.ProblemDetails.ReferToErrorsPropertyForDetails;
            Status = (int)HttpStatusCode.BadRequest;
            Origination = ApiProblemDetailsOrigination.ValidationErrors;
        }
    }

    /// <summary>
    /// Specific origination of where in the API the problem occurred.
    /// </summary>
    public enum ApiProblemDetailsOrigination
    {
        /// <summary>
        /// An unspecified origination.
        /// </summary>
        Unspecified,
        /// <summary>
        /// Error the originated during validation.
        /// </summary>
        ValidationErrors,
    }
}
