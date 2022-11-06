using System.Net.Http.Headers;

namespace Autodroid.Web.Services;

public interface IAuthorizationHeaderUtilService
{
    Task<AuthenticationHeaderValue> GenerateAsync();
}