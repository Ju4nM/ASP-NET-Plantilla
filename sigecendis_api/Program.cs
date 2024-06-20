using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using sigecendis_api.Data;
using sigecendis_api.Data.AuthorizationPolicies.AuthorizationHandlers;
using sigecendis_api.Data.AuthorizationPolicies.AuthorizationPolicyProviders;
using sigecendis_api.Data.Interfaces;
using sigecendis_api.Data.Repositories;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using sigecendis_api.Data.Filters;

var builder = WebApplication.CreateBuilder(args);

// Agregando la autenticacion de JWTBearer
builder.Services.AddAuthentication("Bearer").AddJwtBearer(options =>
{
  options.RequireHttpsMetadata = false;

  string jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
  SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

  options.TokenValidationParameters = new TokenValidationParameters()
  {
    ValidateAudience = false,
    ValidateIssuer = false,
    IssuerSigningKey = signingKey,
    ValidateLifetime = true,
    LifetimeValidator = (DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters) => {
      return expires.HasValue && expires > DateTime.UtcNow;
    }
  };

});

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(config =>
{

  // Configuracion de Swagger para poder inyectar en token en la UI que proporciona y asi acceder a rutas protegidas mediante esta
  config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    Description = "Autorizacion con JWT mediante el header Authorization Bearer.\n\"Authorization: Bearer {{token}}\"",
    Name = "Authorization",
    In = ParameterLocation.Header,
    Type = SecuritySchemeType.Http,
    Scheme = "Bearer",
  });

  config.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme() {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });

});

var PostgreSQLConnectionConfiguration = new PostgreSQLConnection(Environment.GetEnvironmentVariable("CONNECTION_STRING"));
builder.Services.AddSingleton(PostgreSQLConnectionConfiguration);

builder.Services.AddAuthorization();

// Manejador de los politicas con requerimientos de permisos
builder.Services.AddSingleton<IAuthorizationHandler, RoleHandler>();
// Manejador de Politicas de authorizacion (ayuda a manejar las politicas de autorizacion de permisos de manera mas dinamica)
builder.Services.AddSingleton<IAuthorizationPolicyProvider, RoleAuthorizationPolicyProvider>();

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<ExceptionFilter>();

builder.Services.AddCors(options => {
  options.AddPolicy("CORS_ENABLED", builder => {
    builder
      .WithOrigins(Environment.GetEnvironmentVariable("ALLOWED_ORIGIN"))
      .AllowAnyMethod()
      .AllowAnyHeader();
  });
});

var app = builder.Build();

app.UseCors("CORS_ENABLED");

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
