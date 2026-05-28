using System.Text;
using EcoWatch.Infrastructure.Data;
using EcoWatch.Application.Services;
using EcoWatch.Infrastructure.Messaging;
using EcoWatch.Api.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using RabbitMQ.Client;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseOracle(
        builder.Configuration.GetConnectionString("OracleConnection"),
        oracleOptions => oracleOptions.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19)
    ));

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("A chave JWT não foi configurada nos secrets ou variáveis de ambiente.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EcoWatch API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Cole APENAS o seu token JWT aqui cru. O Swagger vai formatar sozinho.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddSingleton<MongoDbContext>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config["MongoDb:ConnectionString"];
    var databaseName = config["MongoDb:DatabaseName"];

    if (string.IsNullOrEmpty(connectionString))
        throw new ArgumentNullException("A Connection String do MongoDB não foi encontrada.");

    return new MongoDbContext(connectionString, databaseName);
});

builder.Services.AddScoped<IMessageBusService, RabbitMqService>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var oracleConn = builder.Configuration.GetConnectionString("OracleConnection");
var rabbitConn = builder.Configuration["RabbitMq:ConnectionString"];
var mongoConn = builder.Configuration["MongoDb:ConnectionString"];

builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory { Uri = new Uri(rabbitConn) };
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    return new MongoClient(mongoConn);
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>(
        name: "Oracle DB",
        tags: new[] { "db", "sql" })
    .AddRabbitMQ(
        name: "CloudAMQP RabbitMQ",
        tags: new[] { "queue", "amqp" })
    .AddMongoDb(
        name: "MongoDB Atlas",
        tags: new[] { "db", "nosql" });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            StatusGeral = report.Status.ToString(),
            TempoVerificacao = report.TotalDuration,
            Dependencias = report.Entries.Select(e => new
            {
                Servico = e.Key,
                Status = e.Value.Status.ToString(),
                Erro = e.Value.Exception?.Message
            })
        };

        var options = new JsonSerializerOptions { WriteIndented = true };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
});

app.Run();