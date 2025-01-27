using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hellang.Middleware.ProblemDetails;
using Serilog;
using Serilog.Events;
using Sieve;
using Sieve.Api.Configuration;
using Sieve.Api.Exceptions;
using Sieve.Api.Middleware;
using Sieve.Interfaces;
using Sieve.Model;
using Sieve.Model.Api;
using Sieve.Model.Api.Requests;
using Sieve.Service.V1;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Data;
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
    cfg.RegisterServicesFromAssemblies([typeof(ValidationBehavior<,>).Assembly, typeof(SieveHandler).Assembly,]);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddControllers()
    .AddNewtonsoftJson()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblies([Assembly.GetExecutingAssembly(), typeof(PrimeCommand).Assembly]);
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

builder.Services.AddSwaggerGen(c =>
{
    c.CustomOperationIds(apiDesc => { return apiDesc.TryGetMethodInfo(out var methodInfo) ? methodInfo.Name : null; });
    c.UseAllOfToExtendReferenceSchemas();
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name!}.xml";
    var filePath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(filePath);

    c.CustomSchemaIds(x => x.FullName);
});


// Add host services.
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(ConfigureContainer);
builder.Host.UseSerilog();

var app = builder.Build();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseCors("AllowAll");
}
else
{
    app.UseExceptionHandler("/error");
    app.UseCors("default");
}

var provider = app.Services.GetService<IApiVersionDescriptionProvider>();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    if (provider != null)
    {
        foreach (var groupName in provider.ApiVersionDescriptions.Select(description => description.GroupName))
            c.SwaggerEndpoint($"/swagger/{groupName}/swagger.json", groupName.ToUpperInvariant());
    }
});

app.UseProblemDetails();
app.Use(CustomMiddleware);
app.UseAuthentication();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();

void ConfigureContainer(ContainerBuilder containerBuilder)
{
    containerBuilder.RegisterInstance(application);
    containerBuilder.RegisterInstance(Log.Logger);
    containerBuilder.Register<ISieve>((_, _) => new SieveImplementation());
}

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

    /// <summary>
    /// A method used to configure the problem details for the API.
    /// </summary>
    /// <param name="options">The options instance used to handle the controls.</param>
    /// <param name="applicationName">The name of the application.</param>
    /// <param name="environmentName">The environment the application is running in.</param>
    /// <param name="logger">The logger in use.</param>
    /// <param name="source">The source of the error.</param>
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

    /// <summary>
    /// Custom middleware to handle errors.
    /// </summary>
    /// <param name="context">The HttpContext.</param>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <returns>A Task.</returns>
    /// <exception cref="MiddlewareException">An exception that is thrown from this middleware if an error endpoint is browsed to.</exception>
    static Task CustomMiddleware(HttpContext context, Func<Task> next)
    {
        if (context.Request.Path.StartsWithSegments("/middleware", out _, out var remaining))
        {
            if (remaining.StartsWithSegments("/error"))
            {
                throw new MiddlewareException("This is an exception thrown from middleware.");
            }

            if (remaining.StartsWithSegments("/status", out _, out remaining))
            {
                var statusCodeString = remaining.Value?.Trim('/');

                if (int.TryParse(statusCodeString, out var statusCode))
                {
                    context.Response.StatusCode = statusCode;
                    return Task.CompletedTask;
                }

            }
        }

        return next();
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
