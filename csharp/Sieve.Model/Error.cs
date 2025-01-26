using System.Net;

namespace Sieve.Model
{
    /// <summary>
    /// A class used to contain errors that might have happened while processing.
    /// </summary>
    public class Error
    {
        /// <summary>
        /// An Id used to identify the request that the error occurred in.
        /// </summary>
        public Guid RequestId { get; set; }
        /// <summary>
        /// A collection used to contain the faults that may have occurred to create the error.
        /// </summary>
        public ICollection<Fault> Faults { get; set; } = [];
        /// <summary>
        /// The status code of the response from the error.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// A static helper method to build an error object.
        /// </summary>
        /// <param name="errorSource">The source of the error.</param>
        /// <param name="statusCode">The status code that should be associated with the error.</param>
        /// <param name="message">The message that should be associated with the error.</param>
        /// <param name="errorType">The type of error that occurred.</param>
        /// <returns>A new instance of the error class that can be used to describe the error.</returns>
        public static Error Create(
            ErrorSource errorSource,
            HttpStatusCode statusCode,
            string message,
            ErrorType errorType = ErrorType.Unspecified) =>
            new()
            {
                Faults =
                [
                    new()
                    {
                        ErrorSource = errorSource,
                        ErrorType = errorType,
                        Message = message,
                    }
                ],
                StatusCode = statusCode,
            };
    }


    /// <summary>
    /// A class used to define a fault that occurred.
    /// </summary>
    public partial class Fault
    {
        /// <summary>
        /// The field value which may have caused the fault.
        /// </summary>
        public string Field { get; set; } = string.Empty;
        /// <summary>
        /// The source of the fault.
        /// </summary>
        public ErrorSource ErrorSource { get; set; } = ErrorSource.Unspecified;
        /// <summary>
        /// The type of fault that occured.
        /// </summary>
        public ErrorType ErrorType { get; set; } = ErrorType.Unspecified;
        /// <summary>
        /// The error message that is associated with the fualt.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// An enumeration used to specify where an error originated.
    /// </summary>
    public enum ErrorSource
    {
        /// <summary>
        /// An unspecified error source.
        /// </summary>
        Unspecified,
        /// <summary>
        /// An error stemming from the SieveApi.
        /// </summary>
        SieveApi,
    }

    /// <summary>
    /// An enumeration used to specify the type of error that occurred.
    /// </summary>
    public enum ErrorType
    {
        /// <summary>
        /// An unspecified error type.
        /// </summary>
        Unspecified,
        /// <summary>
        /// An error that originates from business rules.
        /// </summary>
        BusinessRules,
        /// <summary>
        /// A validation error.
        /// </summary>
        Validation,
        /// <summary>
        /// An error stemming from security issue.
        /// </summary>
        Security,
        /// <summary>
        /// An error that originates from a system issue.
        /// </summary>
        System,
        /// <summary>
        /// An error stemming form an integration issue.
        /// </summary>
        Integrations,
    }

    /// <summary>
    /// An interface used to implement logging for HttpCalls
    /// </summary>
    public interface IHttpCallLogger
    {
        /// <summary>
        /// A method to log the HttpCall.
        /// </summary>
        /// <param name="requestResponse"></param>
        public void Log(HttpCall requestResponse);
        /// <summary>
        /// The sender of the logger.
        /// </summary>
        public string Sender { get; init; }
        /// <summary>
        /// The recipient of the logger.
        /// </summary>
        public string Recipient { get; init; }
        /// <summary>
        /// The key used for the correlationId header.
        /// </summary>
        public string CorrelationIdHeader { get; init; }
        /// <summary>
        /// Whether or not the logger is silent.
        /// </summary>
        public bool IsSilent { get; }
    }

    public class NullHttpCallLogger : IHttpCallLogger
    {
        /// <inheritdoc/>
        public void Log(HttpCall requestResponse) { }

        /// <inheritdoc/>
        public string Sender { get; init; } = string.Empty;

        /// <inheritdoc/>
        public string Recipient { get; init; } = string.Empty;

        /// <inheritdoc/>
        public string CorrelationIdHeader { get; init; } = string.Empty;

        /// <inheritdoc/>
        public bool IsSilent => true;
    }

    /// <summary>
    /// A class used to document HTTP calls and their properties.
    /// </summary>
    public class HttpCall
    {
        /// <summary>
        /// The start of the HttpCall.
        /// </summary>
        public DateTimeOffset RequestStart { get; set; }
        /// <summary>
        /// The end of the HttpCall.
        /// </summary>
        public DateTimeOffset RequestEnd { get; set; }
        /// <summary>
        /// The sender making the HttpCall.
        /// </summary>
        public string Sender { get; set; } = string.Empty;
        /// <summary>
        /// The destination of the HttpCall.
        /// </summary>
        public string Recipient { get; set; } = string.Empty;
        /// <summary>
        /// The location of the HttpCall.
        /// </summary>
        public string Uri { get; set; } = string.Empty;
        /// <summary>
        /// The Http method used in the HttpCall.
        /// </summary>
        public string Method { get; set; } = string.Empty;
        /// <summary>
        /// The headers that accompany the request.
        /// </summary>
        public string RequestHeaders { get; set; } = string.Empty;
        /// <summary>
        /// The body of the HttpCall that accompany the request.
        /// </summary>
        public string RequestBody { get; set; } = string.Empty;
        /// <summary>
        /// The int value of the status code that is sent with the response.
        /// </summary>
        public int StatusCode { get; set; }
        /// <summary>
        /// The headers that accompany the response.
        /// </summary>
        public string ResponseHeaders { get; set; } = string.Empty;
        /// <summary>
        /// The body that accompanies the response.
        /// </summary>
        public string ResponseBody { get; set; } = string.Empty;
        /// <summary>
        /// The duration of the HttpCall.
        /// </summary>
        public long Duration { get; set; }
        /// <summary>
        /// An identifier used to correlate the HttpCall with other requests.
        /// </summary>
        public string CorrelationId { get; set; } = string.Empty;
    }
}
