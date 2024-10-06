using System.IdentityModel.Tokens.Jwt;
using AyBorg.Hub;
using AyBorg.Hub.Database;
using AyBorg.Hub.Database.InMemory;
using AyBorg.SDK.Authorization;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SecurityConfiguration>(builder.Configuration.GetSection("Security"));
builder.Services.AddScoped<ITokenValidator<JwtSecurityToken>, JwtValidator>();

builder.Services.AddGraphQLServer()
                .AddDefaultTransactionScopeHandler()
                .AddQueryType<Query>()
                .AddMutationType<Mutation>();

IConfigurationSection databaseSection = builder.Configuration.GetSection("Database");
IConfigurationSection redisDatabaseSection = databaseSection.GetSection("Redis");
IConfigurationSection inMemoryDatabaseSection = databaseSection.GetSection("InMemory");
if (redisDatabaseSection.Exists() && redisDatabaseSection.GetValue("Enabled", false))
{
    //builder.Services.AddSingleton<IDatabase, RedisDatabase>();
}
else if (inMemoryDatabaseSection.Exists() && inMemoryDatabaseSection.GetValue("Enabled", false))
{
    builder.Services.AddSingleton<IServiceInfoRepository, InMemoryServiceInfoRepository>();
}
else
{
    throw new NotSupportedException("Database configuration not found. Please provide a configuration for either Redis or InMemory database.");
}

builder.Services.AddAuthorization();

WebApplication app = builder.Build();

app.UseAuthorization();
app.MapGraphQL();


await app.RunAsync().ConfigureAwait(false);
