using AyBorg.SDK.Authorization;
using Microsoft.AspNetCore.Identity;

namespace AyBorg.Web.Services;

public sealed class TokenProvider : ITokenProvider
{
    private readonly ILogger<TokenProvider> _logger;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtGenerator _jwtGenerator;

    public TokenProvider(ILogger<TokenProvider> logger, UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccessor, IJwtGenerator jwtGenerator)
    {
        _logger = logger;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _jwtGenerator = jwtGenerator;

    }

    public async ValueTask<string> GenerateTokenAsync()
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        if(httpContext == null)
        {
            return string.Empty;
        }

        System.Security.Claims.ClaimsPrincipal? user = httpContext.User;
        if(user == null)
        {
            _logger.LogWarning("User not found");
            return string.Empty;
        }

        IdentityUser? identity = await _userManager.FindByNameAsync(user.Identity!.Name!);
        if(identity == null)
        {
            _logger.LogWarning("Identity not found");
            return string.Empty;
        }

        IList<string> roles = await _userManager.GetRolesAsync(identity);
        return _jwtGenerator.GenerateToken(identity.UserName!, roles);
    }
}
