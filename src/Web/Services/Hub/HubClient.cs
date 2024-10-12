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

using System.Reflection;
using AyBorg.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace AyBorg.Web.Services.Hub;

public class HubClient : IHubClient
{
    private readonly ILogger<HubClient> _logger;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IOptions<HubOptions> _hubOptions;
    private readonly ServiceOptions _serviceOptions;
    private readonly string _version = Assembly.GetEntryAssembly()!.GetName().Version!.ToString();
    private HubConnection? _connection;


    public HubClient(ILogger<HubClient> logger, ITokenGenerator tokenGenerator, IOptions<ServiceOptions> serviceOptions, IOptions<HubOptions> hubOptions)
    {
        _logger = logger;
        _tokenGenerator = tokenGenerator;
        _serviceOptions = serviceOptions.Value;
        _hubOptions = hubOptions;
    }

    public async ValueTask StartAsync()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(_hubOptions.Value.Url, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(_tokenGenerator.GenerateServiceToken(_serviceOptions.DisplayName, _serviceOptions.UniqueName, _version))!;
            })
            .WithAutomaticReconnect()
            .Build();

        _connection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            _logger.LogInformation($"Received message from {user}: {message}");
        });

        await _connection.StartAsync();
    }

    public async Task SendMessageAsync(string user, string message)
    {
        await _connection!.SendAsync("SendMessage", user, message);
    }
}
