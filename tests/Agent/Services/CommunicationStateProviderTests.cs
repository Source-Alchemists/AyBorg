/*
 * AyBorg - The new software generation for machine vision, automation and industrial IoT
 * Copyright (C) 2024  Source Alchemists
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the,
 * GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using AyBorg.Agent.Services;
using AyBorg.Runtime.Projects;

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
