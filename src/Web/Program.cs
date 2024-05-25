using AyBorg.Data.Identity;
using AyBorg.SDK.Authorization;
using AyBorg.SDK.Communication.gRPC;
using AyBorg.SDK.Communication.gRPC.Registry;
using AyBorg.SDK.Logging.Analytics;
using AyBorg.SDK.System.Configuration;
using AyBorg.Web;
using AyBorg.Web.Components.Account;
using AyBorg.Web.Services;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Services.Analytics;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Elastic.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Services;
using MudExtensions.Services;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Toolbelt.Blazor.Extensions.DependencyInjection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string serviceUniqueName = builder.Configuration.GetValue("AyBorg:Service:UniqueName", "AyBorg.Web")!;
int maximumReceiveMessageSize = builder.Configuration.GetValue("MaximumReceiveMessageSize", 256);
bool isOpenTelemetryEnabled = builder.Configuration.GetValue("OpenTelemetry:Enabled", false)!;
bool isElasticApmEnabled = builder.Configuration.GetValue("ElasticApm:Enabled", false)!;

// Add services to the container.
string? databaseProvider = builder.Configuration.GetValue("DatabaseProvider", "SqlLite");

if (isOpenTelemetryEnabled)
{
    builder.Services.AddOpenTelemetry().WithTracing(builder => builder.AddAspNetCoreInstrumentation())
        .ConfigureResource(resource => resource.AddService(serviceUniqueName))
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation())
        .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation());
}

if (isElasticApmEnabled)
{
    builder.Services.AddAllElasticApm();
    builder.Logging.AddElasticsearch();
}

builder.Services.AddRazorComponents().AddInteractiveServerComponents().AddHubOptions(options => options.MaximumReceiveMessageSize = maximumReceiveMessageSize * 1024);

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole(Roles.Administrator));
    options.AddPolicy("RequireEngineerRole", policy => policy.RequireRole(Roles.Engineer));
    options.AddPolicy("RequireAuditorRole", policy => policy.RequireRole(Roles.Auditor));
    options.AddPolicy("RequireReviewerRole", policy => policy.RequireRole(Roles.Reviewer));
});

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    _ = databaseProvider switch
    {
        "SqlLite" => options.UseSqlite(builder.Configuration.GetConnectionString("SqlLiteConnection"),
                        x => x.MigrationsAssembly("AyBorg.Data.Identity.Migrations.SqlLite")),
        "PostgreSql" => options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection")!,
                        x => x.MigrationsAssembly("AyBorg.Data.Identity..Migrations.PostgreSql")),
        _ => throw new Exception("Invalid database provider")
    }
);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add authentication services.
builder.Services.AddIdentityCore<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddRoleManager<RoleManager<IdentityRole>>()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<IdentityUser>, IdentityNoOpEmailSender>();

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
});
builder.Services.AddMudExtensions();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddHotKeys2();

builder.RegisterGrpcClients();

builder.AddAyBorgAnalyticsLogger();

builder.Services.AddHostedService<RegistryBackgroundService>();
builder.Services.AddHostedService<NotifyBackgroundService>();

builder.Services.AddSingleton<IServiceConfiguration, ServiceConfiguration>();
builder.Services.AddSingleton<IRegistryService, RegistryService>();
builder.Services.AddSingleton<INotifyService, NotifyService>();
builder.Services.AddSingleton<IRpcMapper, RpcMapper>();

builder.Services.AddScoped<IJwtGenerator, JwtGenerator>();
builder.Services.AddScoped<IStateService, StateService>();
builder.Services.AddScoped<IEventLogService, EventLogService>();

// AyBorg.Agent
builder.Services.AddScoped<IProjectManagementService, ProjectManagementService>();
builder.Services.AddScoped<IProjectSettingsService, ProjectSettingsService>();
builder.Services.AddScoped<PluginsService>();
builder.Services.AddScoped<IFlowService, FlowService>();
builder.Services.AddScoped<IRuntimeService, RuntimeService>();
builder.Services.AddScoped<IAgentOverviewService, AgentsOverviewService>();
builder.Services.AddScoped<IDeviceManagerService, DeviceManagerService>();

// AyBorg.NET
builder.Services.AddScoped<AyBorg.Web.Services.Cognitive.IProjectManagerService, AyBorg.Web.Services.Cognitive.ProjectManagerService>();
builder.Services.AddScoped<AyBorg.Web.Services.Cognitive.IFileManagerService, AyBorg.Web.Services.Cognitive.FileManagerService>();
builder.Services.AddScoped<AyBorg.Web.Services.Cognitive.IAnnotationManagerService, AyBorg.Web.Services.Cognitive.AnnotationManagerService>();
builder.Services.AddScoped<AyBorg.Web.Services.Cognitive.IDatasetManagerService, AyBorg.Web.Services.Cognitive.DatasetManagerService>();
builder.Services.AddScoped<AyBorg.Web.Services.Cognitive.IJobManagerService, AyBorg.Web.Services.Cognitive.JobManagerService>();

builder.Services.AddTransient<ITokenProvider, TokenProvider>();
builder.Services.AddTransient<IStorageService, StorageService>();
builder.Services.AddTransient<IAuditService, AuditService>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapAdditionalIdentityEndpoints();

app.MapGet("/Logout", async (HttpContext context, string returnUrl = "/") => {
    await context.SignOutAsync(IdentityConstants.ApplicationScheme);
    context.Response.Redirect(returnUrl);
}).RequireAuthorization();

// Create database if not exists
await (await app.Services.GetService<IDbContextFactory<ApplicationDbContext>>()!.CreateDbContextAsync()).Database.MigrateAsync();

// Initialize identity
IServiceProvider scopedServiceProvider = app.Services.CreateScope().ServiceProvider;
await IdentityInitializer.InitializeAsync(scopedServiceProvider.GetRequiredService<UserManager<IdentityUser>>(), scopedServiceProvider.GetRequiredService<RoleManager<IdentityRole>>()).AsTask();

await app.RunAsync();
