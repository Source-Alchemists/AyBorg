using System.Net.Http.Headers;

namespace AyBorg.Web.Services;

public interface IAuthorizationHeaderUtilService
{
    Task<AuthenticationHeaderValue> GenerateAsync();
}