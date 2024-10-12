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

using AyBorg.Types;
using AyBorg.Web.Services.Cognitive;
using AyBorg.Web.Shared.Models.Cognitive;
using Grpc.Core;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Cognitive.Shared;

public partial class StartModelTrainingDialog : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; init; } = null!;
    [Parameter] public string Name { get; set; } = string.Empty;
    [Parameter] public string ProjectId { get; set; } = string.Empty;
    [Parameter] public bool ShowDatasetSelection { get; init; } = false;
    [Inject] ILogger<StartModelTrainingDialog> Logger { get; init; } = null!;
    [Inject] ISnackbar Snackbar { get; init; } = null!;
    [Inject] IDatasetManagerService DatasetManagerService { get; init; } = null!;
    private bool _isStartDisabled => string.IsNullOrEmpty(Name) || string.IsNullOrWhiteSpace(Name) || _selectedDatasetMeta == null;
    private int _iterations = 10;
    private IEnumerable<DatasetMeta> _datasetMetas = Array.Empty<DatasetMeta>();
    private DatasetMeta _selectedDatasetMeta = null!;
    private string _datasetName => _selectedDatasetMeta == null ? string.Empty : _selectedDatasetMeta.Name;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender && ShowDatasetSelection)
        {
            try
            {
                _datasetMetas = await DatasetManagerService.GetMetasAsync(new DatasetManagerService.GetMetasParameters(ProjectId));
                _datasetMetas = _datasetMetas.Where(d => !d.IsActive).OrderByDescending(d => d.CreationDate);
                _selectedDatasetMeta = _datasetMetas.FirstOrDefault()!;
            }
            catch (RpcException ex)
            {
                Logger.LogWarning((int)EventLogType.UserInteraction, ex, "Failed to get meta informations!");
                Snackbar.Add("Failed to get meta informations!", Severity.Warning);
            }
            await InvokeAsync(StateHasChanged);
        }
    }

    private void CancelClicked()
    {
        MudDialog.Cancel();
    }

    private void StartClicked()
    {
        MudDialog.Close(DialogResult.Ok(new TrainParameters(
            ModelName: Name,
            Iterations: _iterations,
            DatasetId: _selectedDatasetMeta.Id
        )));
    }

    public sealed record TrainParameters(string ModelName, int Iterations, string DatasetId);
}
