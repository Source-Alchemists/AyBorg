using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AyBorg.Web.Shared.Utils;

public static class ElementUtils
{
    public static async ValueTask<BoundingClientRect> GetBoundingClientRectangleAsync(IJSRuntime jsruntime, ElementReference elementReference)
    {
        return await jsruntime.InvokeAsync<BoundingClientRect>("getElementBoundingClientRect", elementReference);
    }
}
