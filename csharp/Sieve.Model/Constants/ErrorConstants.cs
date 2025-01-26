namespace Sieve.Model.Constants
{
    /// <summary>
    /// Constants used for errors.
    /// </summary>
    public static class ErrorConstants
    {
        /// <summary>
        /// Generic Error constants.
        /// </summary>
        public static class Generic
        {
            /// <summary>
            /// constant for out of range error.
            /// </summary>
            public const string QuantityOutOfRange = "QuantityOutOfRange";
            /// <summary>
            /// constant for nonnegative amount expected.
            /// </summary>
            public const string NonNegativeAmountExpected = "NonNegativeAmountExpected";
            /// <summary>
            /// constant for nonnegative integer expected.
            /// </summary>
            public const string NonNegativeIntegerExpected = "NonNegativeIntegerExpected";
            /// <summary>
            /// constant for value required.
            /// </summary>
            public const string ValueRequired = "ValueRequired";
            /// <summary>
            /// constant for invalid value.
            /// </summary>
            public const string InvalidValue = "InvalidValue";
            /// <summary>
            /// constant for invalid email.
            /// </summary>
            public const string InvalidEmail = "InvalidEmail";
        }

        /// <summary>
        /// Problem Details error constants.
        /// </summary>
        public static class ProblemDetails
        {
            /// <summary>
            /// Constant for a validation error.
            /// </summary>
            public const string ValidationError = "One or more validation error(s) occurred.";
            /// <summary>
            /// Constant for error processing request.
            /// </summary>
            public const string ErrorProcessingRequest = "Error(s) processing request";
            /// <summary>
            /// Constant for a system error.
            /// </summary>
            public const string SystemError = "System error";
            /// <summary>
            /// Constant for not found error.
            /// </summary>
            public const string NotFound = "Not found";
            /// <summary>
            /// Constant to recommend referring to errors for details on the error.
            /// </summary>
            public const string ReferToErrorsPropertyForDetails = "Please refer to the errors property for additional details";
        }

        /// <summary>
        /// Validation Error constants.
        /// </summary>
        public static class Validation
        {
        }
    }
}
