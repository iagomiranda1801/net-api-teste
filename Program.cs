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

// Configurar porta do Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// Aplicar migrations automaticamente ao iniciar
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        
        // Verificar se tem connection string configurada
        var connString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connString))
        {
            app.Logger.LogWarning("Connection string não configurada. Verifique as variáveis de ambiente.");
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
                    app.Logger.LogInformation($"Connection string convertida para formato Npgsql: Host={uri.Host};Port={uri.Port};Database={uri.LocalPath.TrimStart('/')}");
                    
                    // Reconfigurar o DbContext com a nova connection string
                    var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                    optionsBuilder.UseNpgsql(connString);
                    context = new AppDbContext(optionsBuilder.Options);
                }
                
                app.Logger.LogInformation("Aplicando migrations...");
                context.Database.Migrate(); // Aplica migrations pendentes
                app.Logger.LogInformation("Migrations aplicadas com sucesso");
            }
            catch (Exception ex)
            {
                app.Logger.LogError(ex, "Erro ao processar connection string ou aplicar migrations");
            }
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Erro ao aplicar migrations");
    }
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

// NÃO usar HTTPS redirect no Railway
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Health check para Railway
app.MapGet("/", () => Results.Ok(new { 
    status = "online", 
    message = "MinhaAPI está rodando!",
    timestamp = DateTime.UtcNow 
})).AllowAnonymous();

app.MapGet("/health", () => Results.Ok(new { 
    status = "healthy", 
    timestamp = DateTime.UtcNow 
})).AllowAnonymous();

app.MapControllers();

app.Run();
