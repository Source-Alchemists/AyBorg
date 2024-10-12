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

using AyBorg.Communication;
using AyBorg.Web.Services;

using Microsoft.AspNetCore.Components;

namespace AyBorg.Web.Shared;

public partial class AppBarMenu : ComponentBase
{
    [Inject] ILogger<AppBarMenu> Logger { get; set; } = null!;
    [Inject] IRegistryService RegistryService { get; set; } = null!;
    [Inject] NavigationManager NavigationManager { get; set; } = null!;

    private const string DASHBOARD_URI = "/dashboard";
    private const string AGENTS_URI = "/agents/overview";
    private const string AGENTS_BASE_URI = "/agents/";
    private const string COGNITIVE_URI = "/cognitive/projects";
    private const string COGNITIVE_BASE_URI = "/cognitive/";
    private const string OBSERVABILITY_URI = "/observability/services";
    private const string OBSERVABILITY_BASE_URI = "/observability/";
    private const string AUDIT_URI = "/audit/reports";
    private const string AUDIT_BASE_URI = "/audit/";

    private bool _isDashboardAvailable = false;
    private bool _isCognitiveAvailable = false;
    private bool _isAuditAvailable = false;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        NavigationManager.LocationChanged += (s, e) => Update();

        try
        {
            IEnumerable<Models.ServiceInfoEntry> services = await RegistryService.ReceiveServicesAsync();
            _isDashboardAvailable = services.Any(s => s.Type.Equals(ServiceTypes.Dashboard));
            _isCognitiveAvailable = services.Any(s => s.Type.Equals(ServiceTypes.Cognitive));
            _isAuditAvailable = services.Any(s => s.Type.Equals(ServiceTypes.Audit));
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not retrieve analytics services from registry");
        }
    }

    private string GetActiveClass(string page)
    {
        if (NavigationManager.Uri.Contains(page))
        {
            return "app-bar-nav-item mud-chip-text mud-chip-color-secondary ml-1 mr-1";
        }
        else
        {
            return "app-bar-nav-item ml-1 mr-1";
        }
    }

    private void Update()
    {
        StateHasChanged();
    }
}
