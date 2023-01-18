using AyBorg.Web.Pages.Admin.Shared;

namespace AyBorg.Web.Tests.Pages.Admin.Shared;

public class RandomPasswordGeneratorTests
{
    [Fact]
    public void GeneratePassword_ReturnsPassword()
    {
        // Act
        string password = RandomPasswordGenerator.Generate();

        // Assert
        Assert.NotNull(password);
        Assert.NotEmpty(password);
        Assert.Equal(RandomPasswordGenerator.Length, password.Length);
    }
}
