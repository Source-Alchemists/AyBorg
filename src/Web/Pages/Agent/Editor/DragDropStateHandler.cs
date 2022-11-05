using Atomy.SDK.Data.DTOs;

/// <summary>
/// Work around class, because DataTransfer is not supported in Blazor.
/// </summary>
internal static class DragDropStateHandler
{
    public static StepDto? DraggedStep { get; set; }
}