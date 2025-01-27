namespace Sieve.Api.Exceptions
{
    /// <summary>
    /// An exception thrown from the middleware.
    /// </summary>
    [Serializable]
    public class MiddlewareException : Exception
    {
        /// <summary>
        /// Empty constructor for making a middlewareException.
        /// </summary>
        public MiddlewareException()
        {
        }

        /// <summary>
        /// A constructor with an error message to create a MiddlewareException.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        public MiddlewareException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// A constructor with a message and inner exception to create a MiddlewareException.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="innerException">An inner exception to the MiddlewareException.</param>
        public MiddlewareException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
