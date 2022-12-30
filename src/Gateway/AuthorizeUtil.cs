using System.Runtime.CompilerServices;
using AyBorg.SDK.Authorization;
using Grpc.Core;

namespace AyBorg.Gateway;

public static class AuthorizeUtil
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Metadata Protect(HttpContext httpContext, IEnumerable<string> roles)
    {
        AuthorizeGuard.ThrowIfNotAuthorized(httpContext, roles);
        string token = httpContext.Request.Headers["Authorization"].First()!;

        return new Metadata
        {
            { "Authorization", token! }
        };
    }
}
