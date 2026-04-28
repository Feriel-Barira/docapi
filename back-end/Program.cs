using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using DocApi.Services;
using DocApi.Services.Interfaces;
using DocApi.Repositories;
using DocApi.Repositories.Interfaces;
using MySql.Data.MySqlClient;
using System.Data;
using DocApi.Common;
using DocApi.Infrastructure;
using DocApi.Filters;
var builder = WebApplication.CreateBuilder(args);
Dapper.SqlMapper.AddTypeHandler(new GuidTypeHandler());

builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});
builder.Services.AddHttpContextAccessor();

// =====================================================
// Base de données
// =====================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"=== CONNEXION DB: {connectionString} ===");
builder.Services.AddScoped<IDbConnection>(sp => new MySqlConnection(connectionString));
builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

// =====================================================
// Repositories
// =====================================================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();   // ← AJOUTÉ
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();       // ← AJOUTÉ
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();      // ← AJOUTÉ
builder.Services.AddScoped<IProcessusRepository, ProcessusRepository>();
builder.Services.AddScoped<IProcedureRepository, ProcedureRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<INonConformiteRepository, NonConformiteRepository>();
builder.Services.AddScoped<IIndicateurRepository, IndicateurRepository>();
builder.Services.AddScoped<IActionCorrectiveRepository, ActionCorrectiveRepository>(); // ← AJOUTÉ
builder.Services.AddScoped<IPointControleRepository, PointControleRepository>();
builder.Services.AddScoped<IEnregistrementRepository, EnregistrementRepository>();
builder.Services.AddScoped<IEnregistrementService, EnregistrementService>();
// =====================================================
// Services
// =====================================================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();    // ← AJOUTÉ
builder.Services.AddScoped<IDashboardService, DashboardService>();   // ← AJOUTÉ
builder.Services.AddScoped<ISearchService, SearchService>();      // ← AJOUTÉ
builder.Services.AddScoped<IProcessusService, ProcessusService>();
builder.Services.AddScoped<IProcedureService, ProcedureService>();
builder.Services.AddScoped<IPointControleService, PointControleService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<INonConformiteService, NonConformiteService>();
builder.Services.AddScoped<IIndicateurService, IndicateurService>();
builder.Services.AddScoped<IActionCorrectiveService, ActionCorrectiveService>(); // ← AJOUTÉ

// =====================================================
// JWT
// =====================================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.SecretKey))
    throw new Exception("JWT Settings not loaded correctly. Check appsettings.json");

Console.WriteLine($"JWT SecretKey loaded: {jwtSettings.SecretKey.Substring(0, 10)}...");

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
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.FromHours(24)
    };
});

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddAuthorization();

// =====================================================
// Swagger
// =====================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DocApi", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// =====================================================
// CORS
// =====================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
//app.UseAuthentication();
//app.UseAuthorization();
app.MapControllers();

app.Run();
public class GuidTypeHandler : Dapper.SqlMapper.TypeHandler<Guid>
{
    public override void SetValue(System.Data.IDbDataParameter parameter, Guid value)
        => parameter.Value = value.ToString();

    public override Guid Parse(object value)
        => Guid.Parse(value.ToString()!);
}