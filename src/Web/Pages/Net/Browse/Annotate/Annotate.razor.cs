using System.Collections.Immutable;
using AyBorg.SDK.Common.Models;
using AyBorg.Web.Services;
using AyBorg.Web.Services.Net;
using AyBorg.Web.Shared;
using AyBorg.Web.Shared.Display;
using AyBorg.Web.Shared.Models.Net;
using Grpc.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace AyBorg.Web.Pages.Net.Browse.Annotate;

public partial class Annotate : ComponentBase
{
    [Parameter] public string ProjectId { get; init; } = string.Empty;
    [Parameter] public string ImageName { get; init; } = string.Empty;

    [Inject] IProjectManagerService ProjectManagerService { get; init; } = null!;
    [Inject] IFileManagerService FileManagerService { get; init; } = null!;
    [Inject] IAnnotationManagerService AnnotationManagerService { get; init; } = null!;
    [Inject] IStateService StateService { get; init; } = null!;
    [Inject] NavigationManager NavigationManager { get; init; } = null!;
    [Inject] ISnackbar Snackbar { get; init; } = null!;
    [Inject] IDialogService DialogService { get; init; } = null!;

    private string _projectName = string.Empty;
    private bool _isLoading = true;
    private IEnumerable<string> _selectedImageNames = Array.Empty<string>();
    private int _selectedImageNumber = 1;
    private string _lastImageName = string.Empty;
    private FileManagerService.ImageContainer _imageContainer = null!;
    private MudTextField<string> _addClassFieldRef = null!;
    private string _newClassName = string.Empty;
    private ValueStore _initialValues = new();
    private ValueStore _tempValues = new();
    private DrawMode _drawMode = DrawMode.None;
    private string _drawStatusBarText = string.Empty;
    private ClassLabel _drawingClassLabel = null!;
    private IEnumerable<LabelRectangle> _shapes => _tempValues.Layers.Select(l => l.Shape);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            if (StateService.NetState != null)
            {
                _selectedImageNames = StateService.NetState.Annotation.SelectedImageNames;
                _selectedImageNumber = StateService.NetState.Annotation.SelectedImageIndex + 1;
                _projectName = StateService.NetState.ProjectName;
            }

            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        if (_lastImageName.Equals(ImageName, StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }

        try
        {
            IEnumerable<ProjectMeta> projectMetas = await ProjectManagerService.GetMetasAsync();
            ProjectMeta? targetProjectMeta = projectMetas.FirstOrDefault(p => p.Id.Equals(ProjectId, StringComparison.InvariantCultureIgnoreCase));
            if (targetProjectMeta != null)
            {
                _projectName = targetProjectMeta.Name;
                _initialValues = new ValueStore();
                _initialValues = _initialValues with { ClassLabels = _initialValues.ClassLabels.AddRange(targetProjectMeta.Classes) };
            }

            _imageContainer = await FileManagerService.DownloadImageAsync(new FileManagerService.DownloadImageParameters(ProjectId, ImageName, false));

            AnnotationManagerService.Meta imageAnnotationMeta = await AnnotationManagerService.GetMetaAsync(new AnnotationManagerService.GetMetaParameters(ProjectId, ImageName));

            ImmutableList<RectangleLayer> layers = ImmutableList<RectangleLayer>.Empty;
            foreach (string id in imageAnnotationMeta.LayerIds)
            {
                RectangleLayer layer = await AnnotationManagerService.GetRectangleLayer(new Services.Net.AnnotationManagerService.GetRectangleLayerParameters(ProjectId, ImageName, ProjectType.ObjectDetection, id));
                ClassLabel classObj = targetProjectMeta!.Classes.First(c => c.Index == layer.ClassIndex);
                layer = layer with { Shape = layer.Shape with { Color = classObj.ColorCode } };
                layers = layers.Add(layer);
            }

            _initialValues = _initialValues with
            {
                Tags = _initialValues.Tags.AddRange(imageAnnotationMeta.Tags),
                Layers = layers
            };
            _tempValues = _initialValues with { };

        }
        catch (RpcException)
        {
            Snackbar.Add("Failed to get meta information!", Severity.Warning);
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task SelectedImageNumberChanged(int value)
    {
        int index = value - 1;
        _selectedImageNumber = value;
        await StateService.SetNetStateAsync(StateService.NetState with { Annotation = StateService.NetState.Annotation with { SelectedImageIndex = index } });
        NavigationManager.NavigateTo($"net/browse/{ProjectId}/annotate/{_selectedImageNames.ElementAt(index)}");
    }

    private void BackClicked()
    {
        NavigationManager.NavigateTo($"net/browse/{ProjectId}");
    }

    private void AddClassChanged(string value)
    {
        _newClassName = value;
    }

    private async Task AddClassKeyUp(KeyboardEventArgs args)
    {
        if (args.Code.Equals("Space", StringComparison.InvariantCultureIgnoreCase)
            || args.Code.Equals("Enter", StringComparison.InvariantCultureIgnoreCase)
            || args.Code.Equals("NumpadEnter", StringComparison.InvariantCultureIgnoreCase)
            || args.Code.Equals("Tab", StringComparison.InvariantCultureIgnoreCase))
        {
            await AddClassClicked();
        }
    }

    private async Task AddClassClicked()
    {
        if (string.IsNullOrEmpty(_newClassName) || string.IsNullOrWhiteSpace(_newClassName))
        {
            return;
        }

        TempAddClass(_newClassName);
        _newClassName = string.Empty;
        await _addClassFieldRef.BlurAsync();
        await _addClassFieldRef.Clear();
        await _addClassFieldRef.FocusAsync();
        await InvokeAsync(StateHasChanged);
    }

    private void TempAddClass(string value)
    {
        if (_tempValues.ClassLabels.Exists(c => c.Name.Equals(value, StringComparison.InvariantCultureIgnoreCase)))
        {
            return;
        }

        int lastIndex = 0;
        foreach (ClassLabel cl in _tempValues.ClassLabels)
        {
            bool match = (lastIndex - cl.Index) == 0;
            if (!match)
            {
                break;
            }

            lastIndex++;
        }

        _tempValues = _tempValues with { ClassLabels = _tempValues.ClassLabels.Add(new ClassLabel { Name = value, Index = lastIndex }) };
    }

    private bool IsClassDeleteable(ClassLabel classLabel)
    {
        return _initialValues.ClassLabels.Exists(c => c.Name.Equals(classLabel.Name, StringComparison.InvariantCultureIgnoreCase));
    }

    private async Task ClassDeleteClicked(ClassLabel classLabel)
    {
        ImmutableList<RectangleLayer> updatedLayers = ImmutableList<RectangleLayer>.Empty;
        updatedLayers = updatedLayers.AddRange(_tempValues.Layers.Where(l => l.ClassIndex != classLabel.Index));
        _tempValues = _tempValues with
        {
            ClassLabels = _tempValues.ClassLabels.Remove(classLabel),
            Layers = updatedLayers
        };
        await InvokeAsync(StateHasChanged);
    }

    private async Task ClassEditClicked(ClassLabel classLabel)
    {
        IDialogReference dialogReference = DialogService.Show<ClassEditDialog>($"Edit {classLabel.Name}",
            new DialogParameters
            {
                { "ColorValue", classLabel.ColorCode }
            },
            new DialogOptions
            {
                CloseButton = true
            }
        );

        DialogResult result = await dialogReference.Result;
        if (result.Canceled)
        {
            return;
        }

        string colorValue = (string)result.Data;
        ClassLabel newClassLabel = classLabel with { ColorCode = colorValue };
        ImmutableList<RectangleLayer> updatedLayers = ImmutableList<RectangleLayer>.Empty;
        foreach (RectangleLayer layer in _tempValues.Layers)
        {
            if (layer.ClassIndex.Equals(classLabel.Index))
            {
                updatedLayers = updatedLayers.Add(layer with
                {
                    Shape = layer.Shape with { Color = colorValue }
                });
            }
            else
            {
                updatedLayers = updatedLayers.Add(layer);
            }
        }
        _tempValues = _tempValues with
        {
            ClassLabels = _tempValues.ClassLabels.Replace(classLabel, newClassLabel),
            Layers = updatedLayers
        };

        await InvokeAsync(StateHasChanged);
    }

    private void ClassDrawClicked(ClassLabel classLabel)
    {
        _drawingClassLabel = classLabel;
        _drawMode = DrawMode.Rectangle;
        _drawStatusBarText = $"Draw: {classLabel.Name}";
    }

    private void ShapeDrawed(SDK.Common.Models.Rectangle value)
    {
        _tempValues = _tempValues with
        {
            Layers = _tempValues.Layers.Add(new RectangleLayer
            {
                ClassIndex = _drawingClassLabel.Index,
                Shape = new LabelRectangle(value.X, value.Y, value.Width, value.Height)
                {
                    Color = _drawingClassLabel.ColorCode
                }
            })
        };
    }

    private void LayerDeleteClicked(RectangleLayer value)
    {
        _tempValues = _tempValues with
        {
            Layers = _tempValues.Layers.Remove(value)
        };
    }

    private async Task SaveClicked()
    {
        try
        {
            await SaveClassLabelsAsync();
            await SaveAnnotationsAsync();
            _initialValues = _tempValues with { };
        }
        catch (RpcException)
        {
            Snackbar.Add("Failed to save annotation!", Severity.Warning);
        }
    }

    private bool IsSaveEnabled()
    {
        return !_initialValues.Equals(_tempValues);
    }

    private async ValueTask SaveClassLabelsAsync()
    {
        foreach (ClassLabel cl in _tempValues.ClassLabels)
        {
            if (!_initialValues.ClassLabels.Contains(cl))
            {
                await ProjectManagerService.AddOrUpdateAsync(new ProjectManagerService.AddOrUpdateClassLabelParameters(ProjectId, cl));
                continue;
            }

            ClassLabel initialCl = _initialValues.ClassLabels.First(c => c.Index.Equals(cl.Index));
            if (!initialCl.Equals(cl))
            {
                await ProjectManagerService.AddOrUpdateAsync(new ProjectManagerService.AddOrUpdateClassLabelParameters(ProjectId, cl));
            }
        }
    }

    private async ValueTask SaveAnnotationsAsync()
    {
        // Remove layer
        foreach (RectangleLayer layer in _initialValues.Layers.Where(l => !_tempValues.Layers.Contains(l)))
        {
            await AnnotationManagerService.RemoveAsync(new Services.Net.AnnotationManagerService.RemoveParameters(
                ProjectId,
                ImageName,
                layer.Id
            ));
        }

        // Add layer
        foreach (RectangleLayer layer in _tempValues.Layers.Where(l => !_initialValues.Layers.Contains(l)))
        {
            await AnnotationManagerService.AddAsync(new AnnotationManagerService.AddParameters(
                    ProjectId,
                    ImageName,
                    ProjectType.ObjectDetection,
                    layer.Id,
                    layer.ClassIndex,
                    new Point[] {
                            new() { X = (int)layer.Shape.X, Y = (int)layer.Shape.Y  },
                            new() { X = (int)layer.Shape.Width, Y = (int)layer.Shape.Height }
                    }));
        }
    }

    private sealed record ValueStore
    {
        public ImmutableList<ClassLabel> ClassLabels { get; init; } = ImmutableList<ClassLabel>.Empty;
        public ImmutableList<string> Tags { get; init; } = ImmutableList<string>.Empty;
        public ImmutableList<RectangleLayer> Layers { get; init; } = ImmutableList<RectangleLayer>.Empty;
    }
}