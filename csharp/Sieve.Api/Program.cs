using Asp.Versioning;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;
using Serilog.Events;
using Sieve.Api.Configuration;
using Sieve.Model;
using Sieve.Model.Api;
using System.Reflection;
using System.Text.Json.Serialization;
using static LanguageExt.Prelude;

var builder = WebApplication.CreateBuilder(args);

var configuration = new ConfigurationBuilder()
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var application = new Application(configuration);

var logger = configuration.GetSection("Log").Get<ApplicationLogger>() ?? new ApplicationLogger();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.ControlledBy(new LevelSwitch(logger.MinimumLevel))
    .Enrich.FromLogContext()
    .Enrich.WithThreadName()
    .Enrich.WithThreadId()
    .WriteTo.Conditional(_ => logger.Console.Enabled, c => c.Async(la => la.Console(restrictedToMinimumLevel: Level(logger.Console.MinimumLevel))))
    .CreateLogger();
AppDomain.CurrentDomain.ProcessExit += (_, _) => Log.CloseAndFlush();

builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
builder.Logging.AddSerilog();

builder.Services.AddLazyCache();
builder.Services.AddMediatR(cfg =>
{
    // TODO: Add configuration
});

builder.Services.AddControllers()
    .AddNewtonsoftJson()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblies([Assembly.GetExecutingAssembly(),]);
builder.Services.AddHttpContextAccessor();
builder.Services.AddApiVersioning(
    options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddCors(options =>
{

    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });

    options.AddPolicy("default", policy =>
    {
        var corsOrigins = configuration.GetSection("AllowedHosts")
            .AsEnumerable().Where(s => !string.IsNullOrEmpty(s.Value)).Select(s => s.Value!).ToArray();

        if (corsOrigins.Any())
        {
            policy.WithOrigins(corsOrigins)
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
        else
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

builder.Services.AddProblemDetails(options => ConfigureProblemDetails(options, application.Name, builder.Environment.EnvironmentName, Log.Logger, ErrorSource.SieveApi));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

/// <summary>
/// Added functionality for the Program class.
/// </summary>
public partial class Program
{
    /// <summary>
    /// Returns a <see cref="LogEventLevel"/> based on the string value passed in. If unable to parse the value, the logging level is set to Error.
    /// </summary>
    /// <param name="value">The string value of a Log Event Level</param>
    /// <returns>A <see cref="LogEventLevel"/>.</returns>
    static LogEventLevel Level(string value) => parseEnum<LogEventLevel>(value).IfNone(LogEventLevel.Error);

    static void ConfigureProblemDetails(Hellang.Middleware.ProblemDetails.ProblemDetailsOptions options,
        string applicationName,
        string environmentName,
        Serilog.ILogger logger,
        ErrorSource source)
    {
        options.IncludeExceptionDetails = (_, exception) =>
        {
            logger.Error("Exception thrown while processing... ExceptionTime: {DateTime}, Application: {Application}, " +
                "ExceptionType: {Type}, ExceptionMessage: {Message}, StackTrace: {StackTrace}",
                DateTimeOffset.UtcNow, applicationName, exception.GetType(), exception.Message, exception.StackTrace);
            return new List<string> { "dev", "qa" }.Contains(environmentName.ToLower());
        };
        options.Map<ValidationException>(e => new ApiValidationProblemDetails(e, source));

        options.Rethrow<NotSupportedException>();
        options.MapToStatusCode<NotImplementedException>(StatusCodes.Status501NotImplemented);
        options.MapToStatusCode<HttpRequestException>(StatusCodes.Status503ServiceUnavailable);
        options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
    }
}


/// <summary>
/// A class used to control the level switch for Serilog.
/// </summary>
public class LevelSwitch : Serilog.Core.LoggingLevelSwitch
{
    /// <summary>
    /// Controls the logging based on the string value that is passed in. If unable to parse the value, the logging level is set to Error.
    /// </summary>
    /// <param name="value">The logging level for Serilog to use.</param>
    public LevelSwitch(string value) => MinimumLevel = parseEnum<LogEventLevel>(value).IfNone(LogEventLevel.Error);
}
