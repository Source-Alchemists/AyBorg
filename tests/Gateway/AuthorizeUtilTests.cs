using System.Security.Claims;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Moq;

namespace AyBorg.Gateway.Tests;

public class AuthorizeUtilTests
{
    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void Test_Protect(bool authorized, bool hasRoles)
    {
        // Arrange
        var allowedRoles = new List<string>();
        if (hasRoles)
        {
            allowedRoles.Add("Admin");
        }

        var context = new DefaultHttpContext();
        var mockUser = new Mock<ClaimsPrincipal>();
        context.User = mockUser.Object;

        if (authorized)
        {
            context.Request.Headers.Add("Authorization", "TokenValue");
            mockUser.Setup(u => u.Claims).Returns(new List<Claim> { new Claim("role", "Admin") });
        }

        // Act
        Metadata resultData = null!;
        if (!authorized && hasRoles)
        {
            Assert.Throws<UnauthorizedAccessException>(() => AuthorizeUtil.Protect(context, allowedRoles));
        }
        else if (authorized)
        {
            // Assert
            resultData = AuthorizeUtil.Protect(context, allowedRoles);
            Assert.NotNull(resultData);
            Assert.Equal("TokenValue", resultData.First().Value);
        }
    }
}
