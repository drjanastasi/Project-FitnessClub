using System.Reflection;
using System.Text.Json.Serialization;
using FitnessClub.Api.Repositories;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services ----------

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // Enum-и в JSON як рядки: "Monday", "Beginner" замість 1, 0.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Сховища в пам'яті. Singleton — щоб дані жили, поки запущений застосунок.
// Етап 2: замінити на реалізації з EF Core (AddScoped).
builder.Services.AddSingleton<IMembershipRepository, InMemoryMembershipRepository>();
builder.Services.AddSingleton<ITrainerRepository, InMemoryTrainerRepository>();
builder.Services.AddSingleton<IWorkoutRepository, InMemoryWorkoutRepository>();
builder.Services.AddSingleton<IMemberRepository, InMemoryMemberRepository>();
// Дозволяємо фронтенду (інший порт) звертатися до API.
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

    // XML-коментарі контролерів → описи в Swagger.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
});

var app = builder.Build();

// ---------- Pipeline ----------

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
