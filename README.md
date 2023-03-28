# AyBorg

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Source-Alchemists_AyBorg&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Source-Alchemists_AyBorg)

:star:  We appreciate your star, it helps!

![Logo](docs/img/logo.svg)

With AyBorg, you'll have the power to build and customize your own industrial applications without needing to write a single line of code. Simply drag and drop elements, connect the ports and voila!

We've got your back when it comes to monitoring your production goods too. Our platform has a focus on image processing, making it easier for you to keep an eye on your production. And if you need to add some extra features, our platform can be easily extended with plugins.

With our built-in auditing functionality, you'll have even more control over your processes and data, leading to improved efficiency and cost savings.

**:bangbang: AyBorg is still in its early stages of development and our API is still fluid and may change. If you happen to come across any bugs, please don't hesitate to report them - we'd be grateful for your help!**

## Agents

AyBorg's **Agents** are specially designed services for automating processes. Using the **AyBorg.Gateway**, they can seamlessly communicate with other areas of the application. These powerful tools are ideal for optimizing your workflow and can help increase your efficiency.

### Editor

Our user-friendly editor makes it a breeze to program your own workflows using simple drag and drop of steps/plugins into the data flow.

![Agent editor dark](docs/img/agent-editor.png)
![Agent editor light](docs/img/agent-editor-alt.png)

### Work with multiple agents

Experience unparalleled flexibility in your workflow! Work with as many agents as you want and distribute them across different systems. Thanks to seamless communication via the MQTT protocol, your creativity knows no bounds. Whether it's simple or complex scenarios, our agents enable you to perfectly realize your ideas!

![Agent overview dark](docs/img/agent-overview.png)
![Agent overview light](docs/img/agent-overview-alt.png)

### Project overview

The project overview lays out all the projects and their current status. That way, you can see in a jiffy which projects have been given the green light for production.

![Project overview dark](docs/img/agent-projects.png)
![Project overview light](docs/img/agent-projects-alt.png)

## Analytics

Get an overview of all events at a glance, whether it's user interaction, system event, or exception. It is important that you can understand and control your system at all times.

![Analytics dark](docs/img/analytics.png)
![Analytics light](docs/img/analytics-alt.png)

## Audit

In industries such as **medicine**, **pharmaceuticals**, and **food**, it is becoming increasingly important to conduct automated processes in an auditable system. Such a system is particularly essential in highly regulated areas such as **[FDA 21 CFR Part 11](https://www.accessdata.fda.gov/scripts/cdrh/cfdocs/cfcfr/cfrsearch.cfm)**. AyBorg has focused on ensuring high auditability and can therefore be used in these areas as well.

### Create new audit report

![Audit changesets dark](docs/img/audit-changesets.png)
![Audit changesets light](docs/img/audit-changesets-alt.png)

### Audit changes (diff)

![Audit diff dark](docs/img/audit-diff.png)
![Audit diff light](docs/img/audit-diff-alt.png)

### Save your reports

AyBorg provides the ability to store and access **audit reports** at any time. By storing these reports, administrators and auditors can review past events and ensure that the system has been functioning properly.

The ability to store and access audit reports offers a valuable way to monitor the system and **ensure compliance with requirements and standards**. Additionally, this can aid in troubleshooting by making it easier to identify the exact timing and cause of a problem.

![Audit saved reports dark](docs/img/audit-saved-reports.png)
![Audit saved reports light](docs/img/audit-saved-reports-alt.png)

## Administration

### Service overview

![Service overview](docs/img/admin-service-overview.png)
![Service overview](docs/img/admin-service-overview-alt.png)

### Usermanagement

AyBorg has a **role-based user management system** that allows for precise control and management of access rights and functionalities within the application. With this approach, users can be organized into different roles, each with distinct permissions and access rights to various areas of the application.

![Usermanagement dark](docs/img/admin-usermanagement.png)
![Usermanagement light](docs/img/admin-usermanagement-alt.png)

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

> AyBorg default user "**SystemAdmin**" with password "**SystemAdmin123!**". \
> :warning: **The default password should be changed immediately!**

### Default Ports

| Service          | HTTP | HTTPS | gRPC |
| ---------------- | ---- | ----- | ---- |
| AyBorg.Gateway   |      |       | 6000 |
| AyBorg.Analytics |      |       | 6001 |
| AyBorg.Audit     |      |       | 6002 |
| AyBorg.Web       | 6010 | 6011  |      |
| AyBorg.Agent     |      |       | 6020 |

## Docker compose

We provide a ready to use Docker compose setup for demo purpose.

Just run `docker compose up` from the repository root directory. \
You can then open AyBorg at `https://localhost:6011`.

## Want to contribute?

We are happy to accept contributions from the community. Please read our [contributing guidelines](CONTRIBUTING.md) for more information.
