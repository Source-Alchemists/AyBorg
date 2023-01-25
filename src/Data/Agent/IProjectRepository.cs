namespace AyBorg.Data.Agent;

public interface IProjectRepository
{
    ValueTask<IEnumerable<ProjectMetaRecord>> GetAllMetasAsync();
    ValueTask<IEnumerable<ProjectMetaRecord>> GetAllMetasAsync(Guid projectMetaId);
    ValueTask<IEnumerable<ProjectMetaRecord>> GetAllMetasAsync(string serviceUniqueName);
    ValueTask<ProjectMetaRecord> FindMetaAsync(Guid projectMetaDbId);
    ValueTask<ProjectRecord> CreateAsync(string projectName, string serviceUniqueName);
    ValueTask<ProjectRecord> FindAsync(Guid projectMetaId);
    ValueTask<ProjectRecord> AddAsync(ProjectRecord project);
    ValueTask<bool> TrySave(ProjectRecord project);
    ValueTask<bool> TryDeleteAsync(Guid projectMetaId);
    ValueTask<bool> TryUpdateAsync(ProjectMetaRecord projectMeta);
    ValueTask<bool> TryRemoveRangeAsync(IEnumerable<ProjectMetaRecord> projectMetas);
    ValueTask<ProjectSettingsRecord> GetSettingAsync(Guid projectMetaDbId);
}
