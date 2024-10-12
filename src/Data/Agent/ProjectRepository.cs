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

using AyBorg.Runtime.Projects;
using AyBorg.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AyBorg.Data.Agent;

public sealed class ProjectRepository : IProjectRepository
{
    private readonly ILogger<ProjectRepository> _logger;
    private readonly IDbContextFactory<ProjectContext> _contextFactory;

    public ProjectRepository(ILogger<ProjectRepository> logger, IDbContextFactory<ProjectContext> contextFactory)
    {
        _logger = logger;
        _contextFactory = contextFactory;
    }

    public async ValueTask<IEnumerable<ProjectMetaRecord>> GetAllMetasAsync()
    {
        using ProjectContext context = await _contextFactory.CreateDbContextAsync();
        return await context.AyBorgProjectMetas!.ToListAsync();
    }

    public async ValueTask<IEnumerable<ProjectMetaRecord>> GetAllMetasAsync(Guid projectMetaId)
    {
        using ProjectContext context = await _contextFactory.CreateDbContextAsync();
        return await context.AyBorgProjectMetas!.Where(pm => pm.Id.Equals(projectMetaId)).ToListAsync();
    }

    public async ValueTask<IEnumerable<ProjectMetaRecord>> GetAllMetasAsync(string serviceUniqueName)
    {
        using ProjectContext context = await _contextFactory.CreateDbContextAsync();
        return await context.AyBorgProjectMetas!.Where(pm => pm.ServiceUniqueName == serviceUniqueName).ToListAsync();
    }

    public async ValueTask<ProjectMetaRecord> FindMetaAsync(Guid projectMetaDbId)
    {
        using ProjectContext context = await _contextFactory.CreateDbContextAsync();
        ProjectMetaRecord? projectMeta = await context.AyBorgProjectMetas!.FindAsync(projectMetaDbId);
        if (projectMeta == null)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "No settings found for project {projectMetaDbId}.", projectMetaDbId);

        }

        return projectMeta!;
    }

    public async ValueTask<ProjectRecord> CreateAsync(string projectName, string serviceUniqueName)
    {
        using ProjectContext context = await _contextFactory.CreateDbContextAsync();
        var emptyProject = new ProjectRecord
        {
            Meta = new ProjectMetaRecord
            {
                Id = Guid.NewGuid(),
                Name = projectName,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                State = ProjectState.Draft,
                ServiceUniqueName = serviceUniqueName
            }
        };

        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ProjectRecord> result = await context.AyBorgProjects!.AddAsync(emptyProject);
        await context.SaveChangesAsync();
        _logger.LogInformation(new EventId((int)EventLogType.ProjectSaved), "Created new project [{projectName}].", projectName);
        return result.Entity;
    }

    public async ValueTask<ProjectRecord> FindAsync(Guid projectMetaId)
    {
        using ProjectContext context = await _contextFactory.CreateDbContextAsync();
        IQueryable<ProjectRecord> queryProject = CreateFullProjectQuery(context);
        ProjectRecord fullProjectRecord = await queryProject.FirstAsync(x => x.Meta.DbId.Equals(projectMetaId));
        return fullProjectRecord;
    }

    public async ValueTask<ProjectRecord> AddAsync(ProjectRecord project)
    {
        using ProjectContext context = await _contextFactory.CreateDbContextAsync();
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ProjectRecord> result = await context.AyBorgProjects!.AddAsync(project);
        await context.SaveChangesAsync();
        return result.Entity;
    }

    public async ValueTask<bool> TrySave(ProjectRecord project)
    {
        try
        {
            using ProjectContext context = await _contextFactory.CreateDbContextAsync();
            await context.AyBorgProjects!.AddAsync(project);
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ProjectMetaRecord> newMeta = await context.AyBorgProjectMetas!.AddAsync(project.Meta);
            await context.AyBorgProjectSettings!.AddAsync(project.Settings);
            await context.SaveChangesAsync();
            project.Meta.DbId = newMeta.Entity.DbId;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "Failed to save project {projectName}. {message}", project.Meta.Name, ex.Message);
            return false;
        }
    }

    public async ValueTask<bool> TryDeleteAsync(Guid projectMetaId)
    {
        using ProjectContext context = await _contextFactory.CreateDbContextAsync();
        List<ProjectRecord> projects = await context.AyBorgProjects!.Include(p => p.Meta).Include(p => p.Settings).Where(p => p.Meta.Id.Equals(projectMetaId)).ToListAsync();
        if (!projects.Any())
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "No project found with id {projectMetaId}.", projectMetaId);
            return false;
        }
        IEnumerable<ProjectMetaRecord> metas = projects.Select(p => p.Meta);
        IEnumerable<ProjectSettingsRecord> settings = projects.Select(p => p.Settings);
        context.AyBorgProjects!.RemoveRange(projects);
        context.AyBorgProjectMetas!.RemoveRange(metas);
        context.AyBorgProjectSettings!.RemoveRange(settings);
        await context.SaveChangesAsync();
        return true;
    }

    public async ValueTask<bool> TryUpdateAsync(ProjectMetaRecord projectMeta)
    {
        using ProjectContext context = await _contextFactory.CreateDbContextAsync();
        ProjectMetaRecord? databaseProjectMeta = await context.AyBorgProjectMetas!.FindAsync(projectMeta.DbId);
        if (databaseProjectMeta == null)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "No project found for database id {projectMetaDbId}.", projectMeta.DbId);
            return false;
        }

        databaseProjectMeta.ApprovedBy = projectMeta.ApprovedBy;
        databaseProjectMeta.Comment = projectMeta.Comment;
        databaseProjectMeta.IsActive = projectMeta.IsActive;
        databaseProjectMeta.State = projectMeta.State;
        databaseProjectMeta.VersionName = projectMeta.VersionName;
        databaseProjectMeta.VersionIteration = projectMeta.VersionIteration;
        databaseProjectMeta.UpdatedDate = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return true;
    }

    public async ValueTask<bool> TryRemoveRangeAsync(IEnumerable<ProjectMetaRecord> projectMetas)
    {
        try
        {
            using ProjectContext context = await _contextFactory.CreateDbContextAsync();
            context.AyBorgProjectMetas!.RemoveRange(projectMetas);
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "Failed to remove project metas. {message}", ex.Message);
            return false;
        }
    }

    public async ValueTask<ProjectSettingsRecord> GetSettingAsync(Guid projectMetaDbId)
    {
        using ProjectContext context = await _contextFactory.CreateDbContextAsync();
        ProjectRecord? projectRecord = await context.AyBorgProjects!.Include(x => x.Settings).FirstOrDefaultAsync(x => x.Meta.DbId.Equals(projectMetaDbId));
        if (projectRecord == null)
        {
            _logger.LogWarning(new EventId((int)EventLogType.ProjectState), "No project found with id [{projectDatabaseId}].", projectMetaDbId);
            return null!;
        }

        return projectRecord.Settings;
    }

    private static IQueryable<ProjectRecord> CreateFullProjectQuery(ProjectContext context)
    {
        return context.AyBorgProjects!.Include(x => x.Meta)
                                        .Include(x => x.Settings)
                                        .Include(x => x.Steps)
                                        .ThenInclude(x => x.MetaInfo)
                                        .Include(x => x.Steps)
                                        .ThenInclude(x => x.Ports)
                                        .Include(x => x.Links)
                                        .AsSplitQuery();
    }
}
