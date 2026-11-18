using System.Reflection;
using System.Text.Json.Serialization;
using FitnessClub.Api.Data;
using FitnessClub.Api.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

var connectionString = builder.Configuration.GetConnectionString("FitnessClub")
    ?? throw new InvalidOperationException("Не знайдено рядок підключення 'FitnessClub' в appsettings.json.");

builder.Services.AddDbContext<FitnessClubDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<IMembershipRepository, MembershipRepository>();
builder.Services.AddScoped<IWorkoutRepository, WorkoutRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Fitness Club API",
        Version = "v1",
        Description = "REST API вебсайту фітнес-клубу: абонементи, тренування, клієнти, тренери, клуб, записи."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
});

var app = builder.Build();

// Застосовуємо міграції під час запуску: якщо бази ще немає — вона створиться,
// якщо з'явилися нові міграції — структура оновиться.
// (Те саме робить команда `dotnet ef database update`.)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FitnessClubDbContext>();
    db.Database.Migrate();
}



// Swagger увімкнено завжди, щоб зручно демонструвати API.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Fitness Club API v1");
});

app.UseCors("Frontend");
app.UseAuthorization();
app.MapControllers();

// Корінь сайту одразу веде на Swagger.
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();
