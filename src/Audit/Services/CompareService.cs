using System.Text.Json;
using AyBorg.Data.Audit.Models;
using AyBorg.Data.Audit.Models.Agent;

namespace AyBorg.Audit.Services;

public sealed class CompareService : ICompareService
{
    public IEnumerable<ChangeRecord> Compare(IEnumerable<ProjectAuditRecord> projectAuditRecords)
    {
        var changes = new List<ChangeRecord>();
        IEnumerable<IGrouping<Guid, ProjectAuditRecord>> groupedProjectAuditRecords = projectAuditRecords.GroupBy(r => r.ProjectId);

        foreach (IGrouping<Guid, ProjectAuditRecord> projectAuditGroup in groupedProjectAuditRecords)
        {
            if (projectAuditGroup.Count() == 1)
            {
                // No diff, just the whole project as changed
                changes.AddRange(Compare(projectAuditGroup.First()));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        return changes;
    }

    private static IEnumerable<ChangeRecord> Compare(ProjectAuditRecord projectAuditRecord)
    {
        var result = new List<ChangeRecord>
        {
            new ChangeRecord
            {
                ChangesetAId = Guid.Empty,
                ChangesetBId = projectAuditRecord.Id,
                Label = "Project/State",
                SubLabel = "Project",
                ValueA = string.Empty,
                ValueB = projectAuditRecord.ProjectState.ToString()
            },

            new ChangeRecord
            {
                ChangesetAId = Guid.Empty,
                ChangesetBId = projectAuditRecord.Id,
                Label = "Project/Settings/IsForceResultCommunicationEnabled",
                SubLabel = "Project settings",
                ValueA = string.Empty,
                ValueB = projectAuditRecord.Settings.IsForceResultCommunicationEnabled.ToString()
            }
        };

        foreach (StepAuditRecord stepRecord in projectAuditRecord.Steps)
        {
            IEnumerable<ChangeRecord> changes = AddNewStep(projectAuditRecord, stepRecord);
            foreach (ChangeRecord change in changes)
            {
                if(string.IsNullOrEmpty(change.ValueA) && string.IsNullOrEmpty(change.ValueB))
                {
                    // Nothing to report
                    continue;
                }
                result.Add(change);
            }
        }

        // foreach (LinkAuditRecord linkRecord in projectAuditRecord.Links)
        // {
        //     StepAuditRecord sourceStep = projectAuditRecord.Steps.First(s => s.Ports.Any(p => p.Id.Equals(linkRecord.SourceId)));
        //     StepAuditRecord targetStep = projectAuditRecord.Steps.First(s => s.Ports.Any(p => p.Id.Equals(linkRecord.TargetId)));
        //     string sourceStepName = sourceStep.Name;
        //     string targetStepName = targetStep.Name;
        //     PortAuditRecord sourcePort = sourceStep.Ports.First(p => p.Id.Equals(linkRecord.SourceId));
        //     PortAuditRecord targetPort = sourceStep.Ports.First(p => p.Id.Equals(linkRecord.TargetId));
        //     string sourcePortName = sourcePort.Name;
        //     string targetPortName = targetPort.Name;

        //     result.Add(new ChangeRecord
        //     {
        //         ChangesetAId = Guid.Empty,
        //         ChangesetBId = projectAuditRecord.Id,
        //         PropertyName = $"Project/Links/{linkRecord.Id}",
        //         ValueA = string.Empty,
        //         ValueB = $"{sourceStepName}.{sourcePortName} -> {targetStepName}.{targetPortName}"
        //     });
        // }

        return result;
    }

    private static IEnumerable<ChangeRecord> AddNewStep(ProjectAuditRecord projectAuditRecord, StepAuditRecord stepAuditRecord)
    {
        var result = new List<ChangeRecord>
        {
            new ChangeRecord {
                ChangesetAId = Guid.Empty,
                ChangesetBId = projectAuditRecord.Id,
                Label = $"Project/Steps/{stepAuditRecord.Name}",
                SubLabel = $"Step: {stepAuditRecord.Name} ({stepAuditRecord.AssemblyName}.{stepAuditRecord.TypeName})",
                ValueA = string.Empty,
                ValueB = JsonSerializer.Serialize(new StepInfo(stepAuditRecord), new JsonSerializerOptions { WriteIndented = true})
            }
        };

        foreach (PortAuditRecord portRecord in stepAuditRecord.Ports)
        {
            result.Add(new ChangeRecord
            {
                ChangesetAId = Guid.Empty,
                ChangesetBId = projectAuditRecord.Id,
                Label = $"Project/Steps/{stepAuditRecord.Name}/{portRecord.Name}/Value",
                SubLabel = $"Port: {portRecord.Name}, Brand: {portRecord.Brand}",
                ValueA = string.Empty,
                ValueB = portRecord.Value
            });
        }

        return result;
    }

    private record StepInfo
    {
        public string Name { get; }
        public string TypeName { get; }
        public string AssemblyName { get; }
        public string AssemblyVersion { get; }

        public StepInfo(StepAuditRecord record)
        {
            Name = record.Name;
            TypeName = record.TypeName;
            AssemblyName = record.AssemblyName;
            AssemblyVersion = record.AssemblyVersion;
        }
    }
}
