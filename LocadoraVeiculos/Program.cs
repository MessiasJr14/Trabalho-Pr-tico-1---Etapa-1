using System.Reflection;
using System.Text.Json.Serialization;
using LocadoraVeiculos.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Enums trafegam como texto no JSON (ex.: "Disponivel" em vez de 1)
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Entity Framework Core + SQL Server: a connection string fica no appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada no appsettings.json.");

builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseSqlServer(connectionString));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Locadora de Veículos API",
        Version = "v1",
        Description = "Trabalho Prático 1 - Sistema de aluguel de veículos (ASP.NET Core + Entity Framework Core + SQL Server Express)."
    });

    // Leva os comentários XML (/// <summary>) do código para a documentação do Swagger
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
});

var app = builder.Build();

// Cria o banco de dados e aplica as migrations pendentes ao iniciar a aplicação
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

    try
    {
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Não foi possível conectar/atualizar o banco de dados. Confira a connection string 'DefaultConnection' no appsettings.json.");
    }
}

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
