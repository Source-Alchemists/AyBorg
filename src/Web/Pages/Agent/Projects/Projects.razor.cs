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
using AyBorg.Web.Services;
using AyBorg.Web.Services.Agent;
using AyBorg.Web.Shared.Modals;
using AyBorg.Web.Shared.Models;
using AyBorg.Web.Shared.Models.Agent;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Agent.Projects;

public partial class Projects : ComponentBase
{
    private string _serviceName = string.Empty;
    private bool _hasServiceError = false;
    private bool _isLoading = true;
    private IEnumerable<Web.Shared.Models.Agent.ProjectMeta> _readyProjects = new List<Web.Shared.Models.Agent.ProjectMeta>();
    private IEnumerable<Web.Shared.Models.Agent.ProjectMeta> _reviewProjects = new List<Web.Shared.Models.Agent.ProjectMeta>();
    private IEnumerable<Web.Shared.Models.Agent.ProjectMeta> _draftProjects = new List<Web.Shared.Models.Agent.ProjectMeta>();

    [Parameter]
    public string ServiceId { get; set; } = string.Empty;

    [Inject] IRegistryService RegistryService { get; set; } = null!;
    [Inject] IProjectManagementService ProjectManagementService { get; set; } = null!;
    [Inject] IDialogService DialogService { get; set; } = null!;
    [Inject] IStateService StateService { get; set; } = null!;
    [Inject] ISnackbar Snackbar { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            IEnumerable<ServiceInfoEntry> services = await RegistryService.ReceiveServicesAsync();
            ServiceInfoEntry? service = services.FirstOrDefault(s => s.Id.ToString() == ServiceId);
            if (service == null)
            {
                _hasServiceError = true;
                return;
            }

            _serviceName = service.Name;

            await StateService.SetAgentStateAsync(new UiAgentState(service));

            await ReceiveProjectsAsync();
            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task ReceiveProjectsAsync()
    {
        IEnumerable<Web.Shared.Models.Agent.ProjectMeta> allProjectMetas = await ProjectManagementService.GetMetasAsync();
        _readyProjects = allProjectMetas.Where(p => p.State == ProjectState.Ready);
        _reviewProjects = allProjectMetas.Where(p => p.State == ProjectState.Review);
        _draftProjects = allProjectMetas.Where(p => p.State == ProjectState.Draft);
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnActivateProjectClicked(Web.Shared.Models.Agent.ProjectMeta project)
    {
        if (await ProjectManagementService.TryActivateAsync(project))
        {
            await ReceiveProjectsAsync();
        }
    }

    private async void OnProjectDeleteClicked(Web.Shared.Models.Agent.ProjectMeta project)
    {
        var options = new DialogOptions();
        var parameters = new DialogParameters
        {
            { "NeedPassword", true },
            { "ContentText", $"Are you sure you want to delete project '{project.Name}'? This action cannot be undone." }
        };
        IDialogReference dialog = await DialogService.ShowAsync<ConfirmDialog>("Delete Project", parameters, options);
        DialogResult result = await dialog.Result;
        if (!result.Canceled && await ProjectManagementService.TryDeleteAsync(project))
        {
            await ReceiveProjectsAsync();
        }
    }

    private async void OnSaveAsReviewClicked(Web.Shared.Models.Agent.ProjectMeta projectMeta)
    {
        IDialogReference dialog = await DialogService.ShowAsync<ConfirmDialog>($"Create Review for {projectMeta.Name}",
        new DialogParameters {
            { "ShowComment", true },
            { "Comment", projectMeta.Comment },
            { "ShowVersion", true },
            { "Version", projectMeta.VersionName },
            { "NeedPassword", true }
        },
        new DialogOptions {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        });
        DialogResult result = await dialog.Result;
        if (!result.Canceled)
        {
            ConfirmDialog.ConfirmResult dialogResult = (ConfirmDialog.ConfirmResult)result.Data;
            if (await ProjectManagementService.TrySaveAsync(projectMeta, new ProjectSaveInfo {
                State = projectMeta.State,
                VersionName = dialogResult.Version,
                Comment = dialogResult.Comment
            }))
            {
                await ReceiveProjectsAsync();
            }
            else
            {
                Snackbar.Add("Failed to save new version", Severity.Error);
            }
        }
    }

    private async void OnAbandonReviewClicked(Web.Shared.Models.Agent.ProjectMeta projectMeta)
    {
        var options = new DialogOptions();
        var parameters = new DialogParameters
        {
            { "ContentText", $"Are you sure you want to abandon review for project '{projectMeta.Name}'?" },
            { "NeedPassword", true }
        };
        IDialogReference dialog = await DialogService.ShowAsync<ConfirmDialog>("Abandon Review", parameters, options);
        DialogResult result = await dialog.Result;
        if (!result.Canceled)
        {
            if (await ProjectManagementService.TrySaveAsync(projectMeta, new ProjectSaveInfo
            {
                State = ProjectState.Draft,
                VersionName = projectMeta.VersionName,
                Comment = "Abandoned Review"
            }))
            {
                await ReceiveProjectsAsync();
            }
            else
            {
                Snackbar.Add("Failed to abandon review", Severity.Error);
            }
        }
    }

    private async void OnSaveAsReadyClicked(Web.Shared.Models.Agent.ProjectMeta projectMeta)
    {
        IDialogReference dialog = await DialogService.ShowAsync<ConfirmDialog>($"Approve Project {projectMeta.Name}",
        new DialogParameters {
            { "ShowComment", true },
            { "Comment", projectMeta.Comment },
            { "NeedPassword", true }
        },
        new DialogOptions {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        });
        DialogResult result = await dialog.Result;
        if (!result.Canceled)
        {
            var dialogResult = (ConfirmDialog.ConfirmResult)result.Data;
            if (await ProjectManagementService.TryApproveAsync(projectMeta, new ProjectSaveInfo
            {
                State = ProjectState.Draft,
                VersionName = projectMeta.VersionName,
                Comment = dialogResult.Comment,
                UserName = dialogResult.UserName
            }))
            {
                await ReceiveProjectsAsync();
            }
        }
    }

    private async Task OnNewProjectClicked()
    {
        IDialogReference dialog = await DialogService.ShowAsync<CreateNewProjectDialog>("New Project", new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        });
        DialogResult result = await dialog.Result;
        if (!result.Canceled)
        {
            await ReceiveProjectsAsync();
        }
    }
}
