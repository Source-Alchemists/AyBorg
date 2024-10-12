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
using Grpc.Core;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AyBorg.Web.Pages.Cognitive.Datasets;

public partial class GenerateDatasetDialog : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; init; } = null!;
    [Inject] IDatasetManagerService DatasetManagerService { get; init; } = null!;
    [Inject] ILogger<GenerateDatasetDialog> Logger { get; init; } = null!;
    [Inject] ISnackbar Snackbar { get; init; } = null!;
    [Parameter] public string ProjectId { get; init; } = string.Empty;
    [Parameter] public string DatasetId { get; init; } = string.Empty;

    private readonly MaxSizeObject _maxSizeValue = new() { IsActive = true };
    private readonly SampleRateObject _sampleRateValue = new();
    private readonly ConfigItem.ProbabilityObject _flipHorizontalValue = new();
    private readonly ConfigItem.ProbabilityObject _flipVerticalValue = new();
    private readonly ConfigItem.ProbabilityObject _rotate90Value = new();
    private readonly ConfigItem.ProbabilityObject _scaleValue = new();
    private readonly ConfigItem.ProbabilityObject _pixelDropoutValue = new();
    private readonly ConfigItem.ProbabilityObject _channelShuffelValue = new();
    private readonly ConfigItem.ProbabilityObject _isoNoiseValue = new();
    private readonly ConfigItem.ProbabilityObject _gaussNoiseValue = new();
    private readonly ConfigItem.ProbabilityObject _brightnessAndContrastValue = new();
    private bool _isGenerating = false;

    private void CancelClicked()
    {
        MudDialog.Cancel();
    }

    private async Task GenerateClicked()
    {
        try
        {
            _isGenerating = true;
            await DatasetManagerService.GenerateAsync(
                new DatasetManagerService.GenerateParameters(ProjectId, DatasetId, new DatasetManagerService.GenerateOptions {
                    MaxSize = _maxSizeValue.IsActive ? _maxSizeValue.Value : 0,
                    FlipHorizontalProbability = _flipHorizontalValue.IsActive ? _flipHorizontalValue.Value : 0f,
                    FlipVerticalProbability = _flipVerticalValue.IsActive ? _flipVerticalValue.Value : 0f,
                    Rotate90Probability = _rotate90Value.IsActive ? _rotate90Value.Value : 0f,
                    ScaleProbability = _scaleValue.IsActive ? _scaleValue.Value : 0f,
                    PixelDropoutProbability = _pixelDropoutValue.IsActive ? _pixelDropoutValue.Value : 0f,
                    ChannelShuffleProbability = _channelShuffelValue.IsActive ? _channelShuffelValue.Value : 0f,
                    IsoNoiseProbability = _isoNoiseValue.IsActive ? _isoNoiseValue.Value : 0f,
                    GaussNoiseProbability = _gaussNoiseValue.IsActive ? _gaussNoiseValue.Value : 0f,
                    BrightnessAndContrastProbability = _brightnessAndContrastValue.IsActive ? _brightnessAndContrastValue.Value : 0f,
                    SampleRate = _sampleRateValue.IsActive ? _sampleRateValue.Value : 0
                })
            );

            Snackbar.Add("Dataset generated", Severity.Success);
            MudDialog.Close(DialogResult.Ok(true));
        }
        catch (RpcException ex)
        {
            Logger.LogWarning((int)EventLogType.UserInteraction, ex, "Failed to generate dataset!");
            Snackbar.Add("Failed to generate dataset!", Severity.Warning);
        }

        _isGenerating = false;
        await InvokeAsync(StateHasChanged);
    }

    private void ConfigItemActiveChanged(bool value)
    {
        UpdateSampleRate();
    }

    private void UpdateSampleRate()
    {
        bool result = _flipHorizontalValue.IsActive
            || _flipVerticalValue.IsActive
            || _rotate90Value.IsActive
            || _scaleValue.IsActive
            || _pixelDropoutValue.IsActive
            || _channelShuffelValue.IsActive
            || _isoNoiseValue.IsActive
            || _gaussNoiseValue.IsActive
            || _brightnessAndContrastValue.IsActive;
        _sampleRateValue.IsActive = result;
    }

    private sealed record MaxSizeObject : ConfigItem.ProbabilityObject {
        public string[] TickLabels { get; } = new string[] { "512px", "1024px", "1536px", "2048px" };
        public int Min { get; } =  512;
        public int Max { get; } =  2048;
        public new int Value { get; set; } = 1024;
    }

    private sealed record SampleRateObject : ConfigItem.ProbabilityObject {
        public int Min { get; } = 5;
        public int Max { get; } = 100;
        public new int Value { get; set; } = 10;
    }
}
