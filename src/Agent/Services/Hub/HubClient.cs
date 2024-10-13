
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
using AyBorg.Communication.Hub;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace AyBorg.Agent.Services.Hub;

public class HubClient : BaseHubClient, IHubClient
{
    private readonly ITokenGenerator _tokenGenerator;
    private readonly ServiceOptions _serviceOptions;
    private readonly string _version = Assembly.GetEntryAssembly()!.GetName().Version!.ToString();

    public HubClient(ILogger<HubClient> logger, ITokenGenerator tokenGenerator, IOptions<ServiceOptions> serviceOptions, IOptions<HubOptions> hubOptions) : base(logger)
    {
        _tokenGenerator = tokenGenerator;
        _serviceOptions = serviceOptions.Value;

        Connection = new HubConnectionBuilder()
                    .WithUrl(hubOptions.Value.Url, options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(_tokenGenerator.GenerateServiceToken(_serviceOptions.DisplayName, _serviceOptions.UniqueName, _version))!;
                    })
                    .WithAutomaticReconnect()
                    .Build();

        Connection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            Logger.LogInformation($"Received message from {user}: {message}");
        });
    }
}
