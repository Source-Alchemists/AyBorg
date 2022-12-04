using AyBorg.Database.Data;
using AyBorg.Gateway.Mapper;
using AyBorg.Gateway.Services;
using AyBorg.SDK.Data.Mapper;
using AyBorg.SDK.System.Configuration;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
string? databaseProvider = builder.Configuration.GetValue("DatabaseProvider", "SqlLite");

builder.Services.AddDbContextFactory<RegistryContext>(options =>
    _ = databaseProvider switch
    {
        "SqlLite" => options.UseSqlite(builder.Configuration.GetConnectionString("SqlLiteConnection"),
                        x => x.MigrationsAssembly("AyBorg.Database.Migrations.SqlLite")),
        "PostgreSql" => options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection")!,
                        x => x.MigrationsAssembly("AyBorg.Database.Migrations.PostgreSql")),
        _ => throw new Exception("Invalid database provider")
    }
);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IRegistryConfiguration, RegistryConfiguration>();
builder.Services.AddSingleton<IDtoMapper, DtoMapper>();
builder.Services.AddSingleton<IDalMapper, DalMapper>();
builder.Services.AddSingleton<IKeeperService, KeeperService>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

// Create database if not exists
app.Services.GetService<IDbContextFactory<RegistryContext>>()!.CreateDbContext().Database.Migrate();

app.Run();
