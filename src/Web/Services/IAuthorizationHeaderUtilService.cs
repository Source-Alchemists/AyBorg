using System.Net.Http.Headers;

namespace Atomy.Web.Services;

public interface IAuthorizationHeaderUtilService
{
    Task<AuthenticationHeaderValue> GenerateAsync();
}