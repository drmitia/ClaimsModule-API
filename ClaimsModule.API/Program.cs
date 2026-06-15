using ClaimsModule.Application;
using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.API.Middleware;
using ClaimsModule.API.Services;
using ClaimsModule.Infrastructure;
using ClaimsModule.Persistence;
using Hangfire;
using ClaimsModule.Infrastructure.Storage;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Layers
builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

// Current user service
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Hangfire
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

builder.Services.AddScoped<ICorrelationIdService, CorrelationIdService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins")
                    .Get<string[]>() ?? Array.Empty<string>())
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// API
builder.Services.AddControllers() //for enums
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("X-User", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "X-User",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Mock user header. Values: handler, supervisor, manager"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "X-User"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Static files for local storage
var storageOptions = new StorageOptions();
builder.Configuration.GetSection(StorageOptions.SectionName).Bind(storageOptions);

if (!app.Environment.IsProduction())
{
    var uploadsPath = storageOptions.GetResolvedLocalPath();

    if (!Directory.Exists(uploadsPath))
        Directory.CreateDirectory(uploadsPath);

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadsPath),
        RequestPath = "/files"
    });
}


app.UseSwagger();
app.UseSwaggerUI();


app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("AllowAngular");

app.UseHttpsRedirection();
app.UseAuthorization();

// Hangfire dashboard
app.UseHangfireDashboard("/hangfire");

// Register recurring SLA job
RecurringJob.AddOrUpdate<ClaimsModule.Infrastructure.Jobs.SlaMonitoringJob>(
    "sla-monitoring",
    job => job.ExecuteAsync(CancellationToken.None),
    "*/15 * * * *");

app.MapControllers();

app.Run();