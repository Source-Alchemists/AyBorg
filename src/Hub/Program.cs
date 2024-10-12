/*
 * AyBorg - The new software generation for machine vision, automation and industrial IoT
 * Copyright (C) 2024  Source Alchemists
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the,
 * GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.IdentityModel.Tokens.Jwt;
using AyBorg.Authorization;
using AyBorg.Hub;
using AyBorg.Hub.Database;
using AyBorg.Hub.Database.InMemory;
using AyBorg.Hub.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SecurityOptions>(builder.Configuration.GetSection("Security"));
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

builder.Services.AddSignalR();

builder.Services.AddAuthorization();

WebApplication app = builder.Build();

app.UseAuthorization();
app.UseJwtMiddleware();
app.MapGraphQL();
app.MapHub<AgentHub>("/hub/agent");
app.MapHub<FrontendHub>("/hub/frontend");


await app.RunAsync().ConfigureAwait(false);
