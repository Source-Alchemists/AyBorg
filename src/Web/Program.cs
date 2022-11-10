using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Services;
using Blazored.LocalStorage;
using Autodroid.Web.Areas.Identity;
using Autodroid.Web.Services.Agent;
using Autodroid.Web.Services;
using Autodroid.Database.Data;
using Autodroid.SDK.Communication.MQTT;
using Autodroid.SDK.Data.Mapper;
using Autodroid.Web;
using Autodroid.SDK.Authorization;
using Autodroid.SDK.System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var databaseProvider = builder.Configuration.GetValue("DatabaseProvider", "SqlLite");

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    _ = databaseProvider switch
    {
        "SqlLite" => options.UseSqlite(builder.Configuration.GetConnectionString("SqlLiteConnection"),
                        x => x.MigrationsAssembly("Autodroid.Database.Migrations.SqlLite")),
        "PostgreSql" => options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection"),
                        x => x.MigrationsAssembly("Autodroid.Database.Migrations.PostgreSql")),
        _ => throw new Exception("Invalid database provider")
    }
);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add authentication services.
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddRoleManager<RoleManager<IdentityRole>>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});
builder.Services.AddAuthorization(options => {
    options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole(Roles.Administrator));
    options.AddPolicy("RequireEngineerRole", policy => policy.RequireRole(Roles.Engineer));
    options.AddPolicy("RequireAuditorRole", policy => policy.RequireRole(Roles.Auditor));
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
});
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddHttpClient("Autodroid.Web.RegistryService");
builder.Services.AddHttpClient("Autodroid.Web.Services.RegistryService>");
builder.Services.AddHttpClient<ProjectManagementService>();
builder.Services.AddHttpClient<PluginsService>();
builder.Services.AddHttpClient<IFlowService, FlowService>();
builder.Services.AddHttpClient<IRuntimeService>();

builder.Services.AddHostedService<Autodroid.SDK.System.Services.RegistryService>();

builder.Services.AddSingleton<IServiceConfiguration, ServiceConfiguration>();
builder.Services.AddSingleton<IRegistryService, RegistryService>();
builder.Services.AddSingleton<IAgentCacheService, AgentCacheService>();
builder.Services.AddSingleton<IDtoMapper, DtoMapper>();
builder.Services.AddSingleton<IMqttClientProvider, MqttClientProvider>();

builder.Services.AddScoped<IAuthorizationHeaderUtilService, AuthorizationHeaderUtilService>();
builder.Services.AddScoped<IJwtProviderService, JwtProviderService>();
builder.Services.AddScoped<IProjectManagementService, ProjectManagementService>();
builder.Services.AddScoped<PluginsService>();
builder.Services.AddScoped<IFlowService, FlowService>();
builder.Services.AddScoped<IRuntimeService, RuntimeService>();
builder.Services.AddScoped<IAgentOverviewService, AgentOverviewService>();
builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.AddScoped<IStateService, StateService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// Create database if not exists
app.Services.GetService<IDbContextFactory<ApplicationDbContext>>()!.CreateDbContext().Database.Migrate();

// Initialize identity
var scopedServiceProvider = app.Services.CreateScope().ServiceProvider;
IdentityInitializer.InitializeAsync(scopedServiceProvider.GetRequiredService<UserManager<IdentityUser>>(), scopedServiceProvider.GetRequiredService<RoleManager<IdentityRole>>()).Wait();

app.Services.GetService<IMqttClientProvider>()?.ConnectAsync().Wait();

app.Run();
