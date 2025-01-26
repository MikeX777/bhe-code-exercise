namespace Sieve.Api.Configuration
{
    /// <summary>
    /// A class used set the configuration for logging for the application.
    /// </summary>
    public class ApplicationLogger
    {
        /// <summary>
        /// A console sink used to log to the console.
        /// </summary>
        public Sink Console { get; set; } = new Sink();

        /// <summary>
        /// The minimum level of logging to be output.
        /// </summary>
        public string MinimumLevel { get; set; } = "Error";
    }

    /// <summary>
    /// A class used to define controls for logging sinks.
    /// </summary>
    public class Sink
    {
        /// <summary>
        /// Whether or not the sink is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// The minimum level of logging to be output to the sink.
        /// </summary>
        public string MinimumLevel { get; set; } = "Error";
    }
}
