using System.Net.Http.Headers;
using Autodroid.SDK.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Autodroid.Web.Services;

public class AuthorizationHeaderUtilService : IAuthorizationHeaderUtilService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtProviderService _jwtGeneratorService;
    private readonly UserManager<IdentityUser> _userManager;

    public AuthorizationHeaderUtilService(IJwtProviderService jwtGeneratorService, IHttpContextAccessor httpContextAccessor, UserManager<IdentityUser> userManager)
    {
        _jwtGeneratorService = jwtGeneratorService;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    public async Task<AuthenticationHeaderValue> GenerateAsync()
    {
        var contextUser = _httpContextAccessor.HttpContext!.User;
        var identityUser = await _userManager.FindByNameAsync(contextUser.Identity!.Name);
        var roles = await _userManager.GetRolesAsync(identityUser);
        return new AuthenticationHeaderValue("Bearer", _jwtGeneratorService.GenerateToken(identityUser!.UserName, roles));
    }
}