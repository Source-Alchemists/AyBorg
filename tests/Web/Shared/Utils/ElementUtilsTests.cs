using AyBorg.Web.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Moq;

namespace AyBorg.Web.Shared.Utils.Tests;

public class ElementUtilsTests {

    [Fact]
    public async Task GetBoundingClientRectangle()
    {
        // Arrange
        var mockJsRuntime = new Mock<IJSRuntime>();
        mockJsRuntime.Setup(m => m.InvokeAsync<BoundingClientRect>("getElementBoundingClientRect", It.IsAny<object[]>())).ReturnsAsync(new BoundingClientRect { Width = 1, Height = 2, X = 3, Y = 4, Top = 5, Right = 6, Bottom = 7, Left = 8});

        // Act
        BoundingClientRect result =  await ElementUtils.GetBoundingClientRectangleAsync(mockJsRuntime.Object, It.IsAny<ElementReference>());

        // Assert
        Assert.Equal(1, result.Width);
        Assert.Equal(2, result.Height);
        Assert.Equal(3, result.X);
        Assert.Equal(4, result.Y);
        Assert.Equal(5, result.Top);
        Assert.Equal(6, result.Right);
        Assert.Equal(7, result.Bottom);
        Assert.Equal(8, result.Left);
    }
}
