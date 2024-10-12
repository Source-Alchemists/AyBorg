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

using AyBorg.Communication.gRPC;
using AyBorg.Web.Services;

namespace AyBorg.Web.Tests.Services;

public class NotifyServiceTests
{
    private readonly NotifyService _service = new();

    [Fact]
    public void Test_SubscribeAndUnsubscribe()
    {
        NotifyService.Subscription subscription = _service.Subscribe("Test", NotifyType.AgentAutomationFlowChanged);
        Assert.Single(_service.Subscriptions);
        _service.Unsubscribe(subscription);
        Assert.Empty(_service.Subscriptions);
    }
}
