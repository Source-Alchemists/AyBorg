using AyBorg.Agent.Services;
using AyBorg.SDK.Projects;

namespace AyBorg.Agent.Tests.Services;

public sealed class communicationStateProviderTest
{
    [Theory]
    [InlineData(ProjectState.Draft, false, false, false, true)]
    [InlineData(ProjectState.Draft, true, false, true, true)]
    [InlineData(ProjectState.Review, false, false, false, true)]
    [InlineData(ProjectState.Review, true, false, true, true)]
    [InlineData(ProjectState.Ready, false, false, true, false)]
    [InlineData(ProjectState.Ready, false, true, true, true)]
    public void Test_Update(ProjectState projectState, bool forceResult, bool forceUi, bool expectedResult, bool expectedWebUi)
    {
        // Arrange
        var communicationStateProvider = new CommunicationStateProvider();
        var project = new Project();
        project.Meta.State = projectState;
        project.Settings.IsResultCommunicationForced = forceResult;
        project.Settings.IsWebUiCommunicationForced = forceUi;

        // Act
        communicationStateProvider.Update(project);

        // Assert
        Assert.Equal(expectedResult, communicationStateProvider.IsResultCommunicationEnabled);
        Assert.Equal(expectedWebUi, communicationStateProvider.IsWebUiCommunicationEnabled);
    }
}
