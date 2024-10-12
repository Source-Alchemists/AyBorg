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

using AyBorg.Data.Agent;
using AyBorg.Runtime.Projects;
using AyBorg.Types.Ports;

namespace AyBorg.Agent.Tests;

public class AuditMapperTests{

    [Fact]
    public void Test_MapProjectRecordToProjectAuditEntry()
    {
        // Arrange
        var project = new ProjectRecord {
            Meta = new ProjectMetaRecord {
                Id = Guid.NewGuid(),
                Name = "TestProject",
                State = ProjectState.Draft
            },
            Settings = new ProjectSettingsRecord {
                IsForceResultCommunicationEnabled = true
            },
            Steps = [
                new() {
                    Id = Guid.NewGuid(),
                    Name = "TestStep",
                    MetaInfo = new PluginMetaInfoRecord(),
                    X = 0,
                    Y = 0,
                    Ports = [
                        new StepPortRecord {
                            Id = Guid.NewGuid(),
                            Direction = PortDirection.Input,
                            Name = "TestPort",
                            Value = "TestValue",
                            Brand = PortBrand.String
                        }
                    ]
                }
            ]
        };

        // Act
        Ayborg.Gateway.Audit.V1.AgentProjectAuditEntry result = AuditMapper.Map(project);

        // Assert
        Assert.Equal(project.Meta.Id.ToString(), result.Id);
        Assert.Equal(project.Meta.Name, result.Name);
        Assert.Equal((int)project.Meta.State, result.State);
        Assert.True(result.Settings.IsForceResultCommunicationEnabled);
        Assert.Equal(project.Steps.Count, result.Steps.Count);
        Assert.Equal(project.Steps[0].Id.ToString(), result.Steps[0].Id);
        Assert.Equal(project.Steps[0].Name, result.Steps[0].Name);
        Assert.Equal(project.Steps[0].X, result.Steps[0].X);
        Assert.Equal(project.Steps[0].Y, result.Steps[0].Y);
        Assert.Equal(project.Steps[0].MetaInfo.AssemblyName, result.Steps[0].AssemblyName);
        Assert.Equal(project.Steps[0].MetaInfo.AssemblyVersion, result.Steps[0].AssemblyVersion);
        Assert.Equal(project.Steps[0].MetaInfo.TypeName, result.Steps[0].TypeName);
        Assert.Equal(project.Steps[0].Ports.Count, result.Steps[0].Ports.Count);
        Assert.Equal(project.Steps[0].Ports[0].Id.ToString(), result.Steps[0].Ports[0].Id);
        Assert.Equal(project.Steps[0].Ports[0].Name, result.Steps[0].Ports[0].Name);
        Assert.Equal(project.Steps[0].Ports[0].Value, result.Steps[0].Ports[0].Value);
        Assert.Equal((int)project.Steps[0].Ports[0].Brand, result.Steps[0].Ports[0].Brand);
        Assert.Equal((int)project.Steps[0].Ports[0].Direction, result.Steps[0].Ports[0].Direction);
    }
}
