namespace AyBorg.Data.Agent.Tests;

public class EnumToRecordConverterTests
{
    [Fact]
    public void Test_Convert()
    {
        // Arrange
        var converter = new EnumToRecordConverter();

        // Act
        string result = converter.Convert(TestEnum.B, null!);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("{\"Name\":\"B\",\"Names\":[\"A\",\"B\"]}", result);
    }

    private enum TestEnum {
        A,
        B
    }
}
