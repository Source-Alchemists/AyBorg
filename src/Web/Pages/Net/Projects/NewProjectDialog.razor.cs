using System.Collections.Immutable;
using AyBorg.SDK.Common;
using AyBorg.Web.Shared.Modals.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace AyBorg.Web.Pages.Net.Projects;

#nullable disable

public partial class NewProjectDialog : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    private string[] _projectTypes = Array.Empty<string>();
    private string _selectedProjectType = string.Empty;
    private ImmutableList<string> _addedTags = ImmutableList<string>.Empty;
    private string _tmpTag = string.Empty;
    private string _projectName = string.Empty;
    private MudTextField<string> _tagField;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _projectTypes = new[] { ProjectType.ObjectDetection.GetDescription() };
        _selectedProjectType = _projectTypes[0];
    }

    private async void OnTagAdornmentClicked()
    {
        if(string.IsNullOrEmpty(_tmpTag) || string.IsNullOrWhiteSpace(_tmpTag))
        {
            return;
        }

        string[] tags = _tmpTag.Split(',');

        foreach (string tag in tags)
        {
            string upperTag = tag.ToUpperInvariant().Trim();
            if (_addedTags.Exists(x => x.Equals(upperTag, StringComparison.InvariantCultureIgnoreCase)))
            {
                continue;
            }

            _addedTags = _addedTags.Add(upperTag);
        }

        _tmpTag = string.Empty;
    }

    private void TagRemoved(MudChip chip)
    {
        _addedTags = _addedTags.Remove(chip.Text);
    }

    private async void OnTagsKeyUp(KeyboardEventArgs args)
    {
        if(args.Code.Equals("Space", StringComparison.InvariantCultureIgnoreCase)
            || args.Code.Equals("Enter", StringComparison.InvariantCultureIgnoreCase)
            || args.Code.Equals("NumpadEnter", StringComparison.InvariantCultureIgnoreCase))
        {
            OnTagAdornmentClicked();
            await _tagField.BlurAsync();
            await _tagField.Clear();
            await _tagField.FocusAsync();
        }
    }

    private async void OnCreateClicked()
    {

    }

    private void OnCloseClicked()
    {
        MudDialog.Close();
    }
}