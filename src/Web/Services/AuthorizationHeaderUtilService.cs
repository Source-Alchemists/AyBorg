using System.Net.Http.Headers;
using AyBorg.SDK.Authorization;
using Microsoft.AspNetCore.Identity;

namespace AyBorg.Web.Services;

public class AuthorizationHeaderUtilService : IAuthorizationHeaderUtilService
{
    private readonly ILogger<IAuthorizationHeaderUtilService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtProviderService _jwtGeneratorService;
    private readonly UserManager<IdentityUser> _userManager;

    public AuthorizationHeaderUtilService(ILogger<IAuthorizationHeaderUtilService> logger, IJwtProviderService jwtGeneratorService, IHttpContextAccessor httpContextAccessor, UserManager<IdentityUser> userManager)
    {
        _logger = logger;
        _jwtGeneratorService = jwtGeneratorService;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    public async Task<AuthenticationHeaderValue> GenerateAsync()
    {
        var contextUser = _httpContextAccessor.HttpContext!.User;
        var identityUser = await _userManager.FindByNameAsync(contextUser.Identity!.Name!);
        if (identityUser == null)
        {
            _logger.LogWarning("User not found");
            return new AuthenticationHeaderValue("Bearer", string.Empty);
        }
        var roles = await _userManager.GetRolesAsync(identityUser);
        return new AuthenticationHeaderValue("Bearer", _jwtGeneratorService.GenerateToken(identityUser!.UserName!, roles));
    }
}