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

using AyBorg.Authorization;
using Microsoft.AspNetCore.Identity;

namespace AyBorg.Web.Services;

public sealed class TokenProvider : ITokenProvider
{
    private readonly ILogger<TokenProvider> _logger;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITokenGenerator _tokenGenerator;

    public TokenProvider(ILogger<TokenProvider> logger, UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccessor, ITokenGenerator tokenGenerator)
    {
        _logger = logger;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _tokenGenerator = tokenGenerator;

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
        return _tokenGenerator.GenerateUserToken(identity.UserName!, roles);
    }
}
