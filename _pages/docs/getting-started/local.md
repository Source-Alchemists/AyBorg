---
title: Running locally
description: How to run AyBorg locally.
sidebar: getting_started
permalink: "/docs/getting-started/local.html"
---

## 1. Git clone

``` bash
git clone https://github.com/Source-Alchemists/AyBorg.git --recursive 
cd AyBorg
```

## 2. Docker compose

We provide a ready to use Docker compose setup.

Just run `docker compose up` from the repository root directory.
You can then open AyBorg at <https://localhost:6011>.

## (Optional) Manually

Because AyBorg is orchastrated into multipe services, you need to start each service separately. In most cases the following setup makes sense:

- A MQTT broker (e.g. [Eclipse-Mosquitto](https://mosquitto.org/))
- AyBorg.Gateway
- AyBorg.Analytics
- AyBorg.Audit
- AyBorg.Web
- One or more AyBorg.Agent(s)

To start a service, navigate to its source directory and run `dotnet run -c release`.

The default appsettings give you a good starting point and will also work locally, but for real scenarios, you will need to change the settings.

**Example (AyBorg.Gateway):**

``` bash
cd src/Gateway
dotnet run -c release
```

## Next steps

- [First start]({{site.baseurl}}/docs/getting-started/first-start)
- [Change your password]({{site.baseurl}}/docs/getting-started/change-password)
