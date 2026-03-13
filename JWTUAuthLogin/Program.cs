
using JWTUAuthLogin.DBModel;
using JWTUAuthLogin.DBModels.DB_UnitOfWork;
using JWTUAuthLogin.Infrastructure.Repository.Login_Module;
using JWTUAuthLogin.Infrastructure.Repository.System_Module;
using JWTUAuthLogin.Infrastructure.Repository.Token_Module;
using JWTUAuthLogin.Shared.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
IConfiguration? configuration = builder.Configuration;

#region Services

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JWTUAuthLogin API",
        Version = "v1"
    });


    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "EJWT Authorization hrader using the Bearer scheme. \r\n\r\n Enter 'Bearer [Space] and then your token in the input below..\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
    });

    option.AddSecurityRequirement(new OpenApiSecurityRequirement
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

#endregion

#region Database

builder.Services.AddDbContext<MBDatabaseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Connection"))
);
//Service Registration for Token
//builder.Services.AddInfraStructure();

var jwt_Section = builder.Configuration.GetSection("JwtAuth");
var secureConfig = new SecureConfigurationHelper(configuration);

string jwtKey = secureConfig.GetSectionValue("JwtAuth", "Key");
builder.Services.Configure<JwtAuth>(jwt_Section);
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // Validate the server that generates the token.
            ValidateAudience = true, // Validate the recipient of the token is authorized to receive.
            ValidateLifetime = true, // Check if the token is not expired and the signing key of the issuer is valid
            ValidateIssuerSigningKey = true, //Validate signature of the token
            ValidIssuer = builder.Configuration["JwtAuth:Issuer"],
            ValidAudience = builder.Configuration["JwtAuth:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            ),
            ClockSkew = TimeSpan.Zero
        };
        // This is the key part: Customize the challenge response for 401
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                // This correctly handles 401 Unauthorized for authentication failures
                context.HandleResponse(); // <-- Keep this for OnChallenge

                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";

                var errorResponse = new
                {
                    Status = 401,
                    Success = false,
                    Message = "Authentication required or credentials invalid.",
                    Description = context.ErrorDescription
                        ?? "No valid authentication token provided."
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
            },
            OnForbidden = async context =>
            {
                // CORRECTED: Remove context.HandleResponse() here.
                // You just need to set the status code and write the response.
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";

                var errorResponse = new
                {
                    Status = 403,
                    Success = false,
                    Message = "Access denied.",
                    Description = "You do not have the necessary permissions to access this resource."
                };
                await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
            }
        };
    });
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProgramAccessChecker, ProgramAccessChecker>();
builder.Services.AddScoped<ITokenManager, TokenManagerController>();

#endregion

var app = builder.Build();

#region Middleware

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "JWTUAuthLogin API V1");
    });
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

#endregion

app.Run();