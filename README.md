# AyBorg

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Source-Alchemists_AyBorg&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Source-Alchemists_AyBorg)

:star:  We appreciate your star, it helps!

![Title](docs/img/title.png)

With AyBorg you have the power to build and customise your own automation applications without writing a single line of code. Just drag and drop elements, connect the ports and voila!

![Workflow](docs/img/workflow.gif)

Our platform focuses on computer vision, making it easier for you to keep an eye on your automation process.

We offer seamless integration with third-party plug-ins, so you can add custom functionality.

With our built-in auditing functionality, you'll have more control over your processes and data, leading to improved efficiency.

**:bangbang: AyBorg is still in its early stages of development and our API is still fluid and may change. If you happen to come across any bugs, please don't hesitate to report them - we'd be grateful for your help!**

## Agents

AyBorg's **Agents** are specially designed services for automating processes. Using the **AyBorg.Gateway**, they can communicate with other areas of the application. These powerful tools are ideal for optimizing your workflow and can help increase your efficiency.

### Editor

Our editor makes it a breeze to program your own workflows using simple drag and drop of steps/plugins into the data flow.

![Agent editor](docs/img/agent-editor-comb.png)

## Getting started

### Git clone

`git clone https://github.com/Source-Alchemists/AyBorg.git --recursive` \
`cd AyBorg`

### Docker compose

We provide a ready to use Docker compose setup.

Just run `docker compose up` from the repository root directory. \
You can then open AyBorg at `https://localhost:6011`.

### (Optional) Manually

Because AyBorg is orchastrated into multipe services, you need to start each service separately.
In most cases the following setup makes sense:

1. A MQTT broker (e.g. [Eclipse-Mosquitto](https://mosquitto.org))
2. AyBorg.Gateway
3. AyBorg.Log
4. AyBorg.Web
5. One or more [AyBorg.Agent(s)](docs/agent/agent.md)

### Default credentials

The default appsettings give you a good starting point and will also work locally, but for real scenarios, you will need to change the settings.

> AyBorg default user "**SystemAdmin**" with password "**SystemAdmin123!**". \
> :warning: **The default password should be changed immediately!**

### Default Ports

| Service          | HTTP | HTTPS | gRPC |
| ---------------- | ---- | ----- | ---- |
| AyBorg.Gateway   |      |       | 6000 |
| AyBorg.Log |      |       | 6001 |
| AyBorg.Web       | 6010 | 6011  |      |
| AyBorg.Agent     |      |       | 6020 |

## Want to contribute?

We are happy to accept contributions from the community. Please read our [contributing guidelines](CONTRIBUTING.md) for more information.
