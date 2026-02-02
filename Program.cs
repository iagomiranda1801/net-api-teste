using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using MinhaApi.Infrastructure.Data;
using MinhaApi.Domain.Interfaces;
using MinhaApi.Infrastructure.Repositories;
using MinhaApi.Application.Interfaces;
using MinhaApi.Application.Services;
using MinhaApi.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configurar Swagger com JWT
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "MinhaApi", 
        Version = "v1",
        Description = "API com autenticação JWT"
    });

    // Configuração de segurança JWT no Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando Bearer. Exemplo: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// Configurar PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key não configurado");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// Dependency Injection - Repository Pattern
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Configurar Kestrel para Railway
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(Environment.GetEnvironmentVariable("PORT") ?? "5138"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

// Middleware para logging de requisições (debug Railway)
app.Use(async (context, next) =>
{
    app.Logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path} from {context.Connection.RemoteIpAddress}");
    await next();
});

// NÃO usar HTTPS redirect no Railway
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Health check para Railway - DEVE ser o primeiro endpoint
app.MapGet("/", () => 
{
    return Results.Ok(new { 
        status = "online", 
        message = "MinhaAPI está rodando!",
        timestamp = DateTime.UtcNow 
    });
}).AllowAnonymous().WithName("Root");

app.MapGet("/health", () => 
{
    return Results.Ok(new { 
        status = "healthy", 
        timestamp = DateTime.UtcNow 
    });
}).AllowAnonymous().WithName("HealthCheck");

app.MapControllers();

// Aplicar migrations em background (não bloqueia o startup)
_ = Task.Run(async () =>
{
    await Task.Delay(1000); // Aguarda 1 segundo para garantir que o servidor HTTP está pronto
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        // Verificar se tem connection string configurada
        var connString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connString))
        {
            logger.LogWarning("Connection string não configurada. Verifique as variáveis de ambiente.");
        }
        else
        {
            try
            {
                // Converter formato URI para Npgsql format se necessário
                if (connString.StartsWith("postgresql://") || connString.StartsWith("postgres://"))
                {
                    var uri = new Uri(connString);
                    connString = $"Host={uri.Host};Port={uri.Port};Database={uri.LocalPath.TrimStart('/')};Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]}";
                    logger.LogInformation($"Connection string convertida para formato Npgsql: Host={uri.Host};Port={uri.Port};Database={uri.LocalPath.TrimStart('/')}");
                    
                    // Reconfigurar o DbContext com a nova connection string
                    var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                    optionsBuilder.UseNpgsql(connString);
                    context = new AppDbContext(optionsBuilder.Options);
                }
                
                logger.LogInformation("Aplicando migrations...");
                context.Database.Migrate();
                logger.LogInformation("Migrations aplicadas com sucesso");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao processar connection string ou aplicar migrations");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro ao aplicar migrations em background");
    }
});

app.Run();
