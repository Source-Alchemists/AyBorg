using AyBorg.Agent.Services;
using AyBorg.SDK.Projects;

namespace AyBorg.Agent.Tests.Services;

public sealed class communicationStateProviderTest
{
    [Theory]
    [InlineData(ProjectState.Draft, false, false)]
    [InlineData(ProjectState.Draft, true, true)]
    [InlineData(ProjectState.Review, false, false)]
    [InlineData(ProjectState.Review, true, true)]
    [InlineData(ProjectState.Ready, false, true)]
    public void Test_Update(ProjectState projectState, bool forceResult, bool expectedResult)
    {
        // Arrange
        var communicationStateProvider = new CommunicationStateProvider();
        var project = new Project();
        project.Meta.State = projectState;
        project.Settings.IsForceResultCommunicationEnabled = forceResult;

        // Act
        communicationStateProvider.Update(project);

        // Assert
        Assert.Equal(expectedResult, communicationStateProvider.IsResultCommunicationEnabled);
    }
}
