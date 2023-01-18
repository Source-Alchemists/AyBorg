using Blazored.LocalStorage;

namespace AyBorg.Web.Services.AppState;

public class AutomationFlowState
{
    private readonly ILocalStorageService _localStorageService;

    public double Zoom { get; set; } = 1.0;
    public double OffsetX { get; set; } = 0.0;
    public double OffsetY { get; set; } = 0.0;

    public AutomationFlowState(ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }

    public async ValueTask SetZoomAsync(double zoom)
    {
        Zoom = zoom;
        await _localStorageService.SetItemAsync("Agent_AF_Zoom", zoom);
    }

    public async ValueTask<double> UpdateZoomAsync()
    {
        double result = await _localStorageService.GetItemAsync<double>("Agent_AF_Zoom");
        if (result != 0)
        {
            Zoom = result;
            return result;
        }
        result = 1.0;
        return result;
    }

    public async ValueTask SetOffsetAsync(double offsetX, double offsetY)
    {
        OffsetX = offsetX;
        OffsetY = offsetY;
        await _localStorageService.SetItemAsync("Agent_AF_OffsetX", offsetX);
        await _localStorageService.SetItemAsync("Agent_AF_OffsetY", offsetY);
    }

    public async ValueTask<(double offsetX, double offsetY)> UpdateOffsetAsync()
    {
        double offsetX = await _localStorageService.GetItemAsync<double>("Agent_AF_OffsetX");
        double offsetY = await _localStorageService.GetItemAsync<double>("Agent_AF_OffsetY");
        OffsetX = offsetX;
        OffsetY = offsetY;
        return (offsetX, offsetY);
    }
}
