using AyBorg.Database.Data;
using AyBorg.SDK.Authorization;
using AyBorg.SDK.Communication.gRPC.Registry;
using AyBorg.SDK.System.Configuration;
using AyBorg.Web;
using AyBorg.Web.Areas.Identity;
using AyBorg.Web.BuilderTools;
using AyBorg.Web.Services;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Services.AppState;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
string? databaseProvider = builder.Configuration.GetValue("DatabaseProvider", "SqlLite");

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    _ = databaseProvider switch
    {
        "SqlLite" => options.UseSqlite(builder.Configuration.GetConnectionString("SqlLiteConnection"),
                        x => x.MigrationsAssembly("AyBorg.Database.Migrations.SqlLite")),
        "PostgreSql" => options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection")!,
                        x => x.MigrationsAssembly("AyBorg.Database.Migrations.PostgreSql")),
        _ => throw new Exception("Invalid database provider")
    }
);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add authentication services.
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
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
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole(Roles.Administrator));
    options.AddPolicy("RequireEngineerRole", policy => policy.RequireRole(Roles.Engineer));
    options.AddPolicy("RequireAuditorRole", policy => policy.RequireRole(Roles.Auditor));
    options.AddPolicy("RequireReviewerRole", policy => policy.RequireRole(Roles.Reviewer));
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
});
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredSessionStorage();

GrpcClientRegisterTool.Register(builder);

builder.Services.AddHostedService<RegistryBackgroundService>();
builder.Services.AddHostedService<NotifyBackgroundService>();

builder.Services.AddSingleton<IServiceConfiguration, ServiceConfiguration>();
builder.Services.AddSingleton<IRegistryService, RegistryService>();
builder.Services.AddSingleton<INotifyService, NotifyService>();

builder.Services.AddScoped<IAuthorizationHeaderUtilService, AuthorizationHeaderUtilService>();
builder.Services.AddScoped<IJwtProviderService, JwtProviderService>();
builder.Services.AddScoped<IProjectManagementService, ProjectManagementService>();
builder.Services.AddScoped<IProjectSettingsService, ProjectSettingsService>();
builder.Services.AddScoped<PluginsService>();
builder.Services.AddScoped<IFlowService, FlowService>();
builder.Services.AddScoped<IRuntimeService, RuntimeService>();
builder.Services.AddScoped<IAgentOverviewService, AgentOverviewService>();
builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.AddScoped<IStateService, StateService>();

WebApplication app = builder.Build();

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
IServiceProvider scopedServiceProvider = app.Services.CreateScope().ServiceProvider;
await IdentityInitializer.InitializeAsync(scopedServiceProvider.GetRequiredService<UserManager<IdentityUser>>(), scopedServiceProvider.GetRequiredService<RoleManager<IdentityRole>>()).AsTask();

app.Run();
