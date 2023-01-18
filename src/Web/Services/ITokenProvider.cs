namespace AyBorg.Web.Services;

public interface ITokenProvider
{
    ValueTask<string> GenerateTokenAsync();
}
