using AyBorg.SDK.Common.Models;

namespace AyBorg.Web.Pages.Agent.Editor;

/// <summary>
/// Work around class, because DataTransfer is not supported in Blazor.
/// </summary>
internal static class DragDropStateHandler
{
    public static Step? DraggedStep { get; set; }
}
