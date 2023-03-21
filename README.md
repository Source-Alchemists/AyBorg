# AyBorg

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Source-Alchemists_AyBorg&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Source-Alchemists_AyBorg)

:star:  We appreciate your star, it helps!

![Logo](docs/img/logo.svg)

Hey there! Are you tired of complex and expensive Industrial IoT solutions that only add to your headaches? Introducing AyBorg, the powerful and scalable no-code/low-code platform for your production processes.

Our platform is free, open-source and provides a service-oriented architecture that can be easily adapted to your needs. With AyBorg, you'll have the power to build and customize your own industrial applications without needing to write a single line of code. Simply drag and drop elements, connect the ports and voila! You've got a fully functioning application that can be easily scaled and customized as your business grows.

We've got your back when it comes to monitoring your production goods too. Our platform has a focus on using camera systems for image processing, making it easier for you to keep an eye on your production. And if you need to add some extra features, our platform can be easily extended with plugins.

With AyBorg, you don't have to worry about being locked into one vendor. Our platform is open-source, meaning you have the freedom to adapt and modify it as you see fit. And because it's free, you'll be saving money compared to closed source solutions. Plus, with our built-in auditing functionality, you'll have even more control over your processes and data, leading to improved efficiency and cost savings.

So what are you waiting for? Say goodbye to complex and expensive solutions, and hello to the power of AyBorg. Get started today!

**:bangbang: AyBorg is still in its early stages of development and our API is still fluid and may change. If you happen to come across any bugs, please don't hesitate to report them - we'd be grateful for your help!**

## Agents

AyBorg's **Agents** are specially designed services for automating processes. Using the **AyBorg.Gateway**, they can seamlessly communicate with other areas of the application. These powerful tools are ideal for optimizing your workflow and can help increase your efficiency.

### Editor

Our user-friendly editor makes it a breeze to program your own workflows using simple drag and drop of steps/plugins into the data flow.

![AgentEditor](docs/img/agent-editor.png)

### Work with multiple agents

Experience unparalleled flexibility in your workflow! Work with as many agents as you want and distribute them across different systems. Thanks to seamless communication via the MQTT protocol, your creativity knows no bounds. Whether it's simple or complex scenarios, our agents enable you to perfectly realize your ideas!

![AgentOverview](docs/img/agent-overview.png)

### Project overview

The project overview lays out all the projects and their current status. That way, you can see in a jiffy which projects have been given the green light for production.

![ProjectOverview](docs/img/agent-projects.png)

## Analytics

Get an overview of all events at a glance, whether it's user interaction, system event, or exception. It is important that you can understand and control your system at all times.

![Analytics](docs/img/analytics.png)

## Audit

In industries such as **medicine**, **pharmaceuticals**, and **food**, it is becoming increasingly important to conduct automated processes in an auditable system. Such a system is particularly essential in highly regulated areas such as **[FDA 21 CFR Part 11](https://www.accessdata.fda.gov/scripts/cdrh/cfdocs/cfcfr/cfrsearch.cfm)**. AyBorg has focused on ensuring high auditability and can therefore be used in these areas as well.

### Create new audit report

![AuditChangesets](docs/img/audit-changesets.png)

### Audit changes (diff)

![AuditDiff](docs/img/audit-diff.png)

### Save your reports

![AuditSavedReports](docs/img/audit-saved-reports.png)

## Administration

### Service overview

![ServiceOverview](docs/img/admin-service-overview.png)

### Usermanagement

![Usermanagement](docs/img/admin-usermanagement.png)

## Getting started

Because AyBorg is orchastrated into multipe services, you need to start each service separately.
In most cases the following setup makes sense:

1. A MQTT broker (e.g. [Eclipse-Mosquitto](https://mosquitto.org))
2. AyBorg.Gateway
3. AyBorg.Analytics
4. AyBorg.Audit
5. AyBorg.Web
6. One or more [AyBorg.Agent(s)](docs/agent/agent.md)

The default appsettings give you a good starting point and will also work locally, but for real scenarios, you will need to change the settings.

> AyBorg default user "**SystemAdmin**" with password "**SystemAdmin123!**".

> :warning: **The default password should be changed immediately!**

## Default Ports

| Service          | HTTP | HTTPS | gRPC |
| ---------------- | ---- | ----- | ---- |
| AyBorg.Gateway   |      |       | 5000 |
| AyBorg.Analytics |      |       | 5001 |
| AyBorg.Audit     |      |       | 5002 |
| AyBorg.Web       | 5010 | 5011  |      |
| AyBorg.Agent     |      |       | 5020 |

## Example setups

Services can be added any time. The AyBorg.Gateway will detect the new service and establish the communication.

![BlockDiagram](docs/img/block_diagram.png)

### Using AyBorg.Agent as MQTT Consumer/Producer

Typically, you will send the processed results to an MQTT broker so that other services (e.g. another AyBorg.Agent) can further process the data.
![BlockDiagram](docs/img/block_diagram2.png)

## Want to contribute?

We are happy to accept contributions from the community. Please read our [contributing guidelines](CONTRIBUTING.md) for more information.
