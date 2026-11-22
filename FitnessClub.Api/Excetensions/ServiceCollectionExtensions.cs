using System.Reflection;
using System.Security.Claims;
using System.Text;
using FitnessClub.Api.Auth;
using FitnessClub.Api.Data;
using FitnessClub.Api.Diagnostics;
using FitnessClub.Api.Models;
using FitnessClub.Api.Repositories;
using FitnessClub.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
namespace FitnessClub.Api.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FitnessClub")
            ?? throw new InvalidOperationException("Не знайдено рядок підключення 'FitnessClub' в appsettings.json.");
        services.AddDbContext<FitnessClubDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IMembershipRepository, MembershipRepository>();
        services.AddScoped<IWorkoutRepository, WorkoutRepository>();
        services.AddScoped<IClubRepository, ClubRepository>();
        services.AddSingleton<ITrainerRepository, InMemoryTrainerRepository>();
        services.AddSingleton<IMemberRepository, InMemoryMemberRepository>();
        services.AddScoped<IMembershipService, MembershipService>();
        services.AddScoped<IWorkoutService, WorkoutService>();
        services.AddScoped<IClubService, ClubService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddTransient<IWorkoutScheduleValidator, WorkoutScheduleValidator>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddTransient<ITransientProbe, LifetimeProbe>();
        services.AddScoped<IScopedProbe, LifetimeProbe>();
        services.AddSingleton<ISingletonProbe, LifetimeProbe>();
        return services;
    }
    public static IServiceCollection AddIdentityAndJwt(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<AdminUserSettings>()
            .Bind(configuration.GetSection(AdminUserSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services
            .AddIdentityCore<AppUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<FitnessClubDbContext>()
            .AddErrorDescriber<UkrainianIdentityErrorDescriber>();
        var jwt = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("Не знайдено розділ 'Jwt' в appsettings.json.");
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role
                };
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        await WriteProblemAsync(context.HttpContext, StatusCodes.Status401Unauthorized,
                            "Потрібна автентифікація: передайте дійсний JWT у заголовку Authorization: Bearer <токен>.");
                    },
                    OnForbidden = context => WriteProblemAsync(context.HttpContext, StatusCodes.Status403Forbidden,
                        "Недостатньо прав: ця операція доступна лише адміністратору.")
                };
            });
        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.AdminOnly, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(Roles.Administrator))
            .AddPolicy(Policies.MemberOrAdmin, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(Roles.User, Roles.Administrator));

        return services;
    }
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Fitness Club API",
                Version = "v1",
                Description = "REST API вебсайту фітнес-клубу: абонементи, тренування, клієнти, тренери, клуб, записи. " +
                              "Щоб виконувати захищені запити: POST /api/auth/login → скопіюйте token → кнопка Authorize."
            });
            var bearerScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Вставте JWT з відповіді POST /api/auth/login (без слова Bearer).",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            };
            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, bearerScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { bearerScheme, Array.Empty<string>() }
            });
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
        });

        return services;
    }
    private static Task WriteProblemAsync(HttpContext context, int statusCode, string title)
    {
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Instance = context.Request.Path
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
    }
}
