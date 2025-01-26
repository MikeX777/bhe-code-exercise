namespace Sieve.Api.Configuration
{
    /// <summary>
    /// Configuration class for the API.
    /// </summary>
    public class Application(IConfiguration configuration)
    {
        /// <summary>
        /// The name of the API.
        /// </summary>
        public string Name { get; set; } = configuration.GetValue<string>("Name") ?? string.Empty;
    }
}
