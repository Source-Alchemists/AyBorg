---
title: Reconfigure Elastic fleet output
description: How to reconfigure Elastic fleet output to troubleshoot Docker environment.
sidebar: getting_started
permalink: "/docs/getting-started/reconfigure-elastic-fleet-output.html"
---

If you are running the Elastic Fleet server inside Docker, you will notice that the CPU and Memory usage fields are not being updated. Likewise, if you click on the Host link, the logs are not being populated either.

This is because, by default, the Elastic Agent is attempting to log data to a local Elasticsearch instance, which is not correct for the Docker environment.

## 1. Copy the CA certificate from the cluster

`docker cp es01:/usr/share/elasticsearch/config/certs/ca/ca.crt /tmp/.`

## 2. Get the fingerprint of the certificate

For this, we can use an OpenSSL command:

`openssl x509 -fingerprint -sha256 -noout -in /tmp/ca.crt | awk -F"=" {' print $2 '} | sed s/://g`

This will produce something similar to:

`0ED5EE052D3612064B8AC2260AD87ABE515396694E66A9D9D53A20BCFBB12B6D`

## 3. Get the content of the certificate

For this, we jsut neet to `cat` the `ca.crt` file:

`cat /tmp/ca.crt`

``` bash
-----BEGIN CERTIFICATE-----
MIIDSTCCAjGgAwIBAgIUN4teP7Njh5uMAVA3SBGVyzmNnmwwDQYJKoZIhvcNAQEL
BQAwNDEyMDAGA1UEAxMpRWxhc3RpYyBDZXJ0aWZpY2F0ZSBUb29sIEF1dG9nZW5l
cmF0ZWQgQ0EwHhcNMjMxMTEzMTg0ODA4WhcNMjYxMTEyMTg0ODA4WjA0MTIwMAYD
VQQDEylFbGFzdGljIENlcnRpZmljYXRlIFRvb2wgQXV0b2dlbmVyYXRlZCBDQTCC
ASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBALUBkAom2+Fk0SGneJhh7Ewf
ExctEEUnuW9bifoSVhU8n6YG2YivKQsizh3rarJGP//CpiN2DW4rlh1UUYtTgv55
1Ag2hwsoXhkpaMoIrFzmZQ7cgz4Ukve9/jdwfAtITeop/4pFlDpmUe/nLVzE3IDK
jFMPsDOZmliVvddFoHh0bQmoWGQUP7Yp8TmIF+FGIDi0xriZ0+2+PSENTV+XXreK
VQM5LMQl3CJB4rjzlP26az7+iGgf+QiMsun/gXSOp6Hllo1ktdHKEQ88JSFIZYgP
zg6OZwCUgNDoJe4Yl2D7J8PTEt5uueCiq4wieZuYnIxSYEU/y8FwGTfM+AZWYt0C
AwEAAaNTMFEwHQYDVR0OBBYEFEyHuybIj6A6J5/GqacL1mzo0HVcMB8GA1UdIwQY
MBaAFEyHuybIj6A6J5/GqacL1mzo0HVcMA8GA1UdEwEB/wQFMAMBAf8wDQYJKoZI
hvcNAQELBQADggEBAGm2yBpSeECV9VMT7qtpj5GCgl0jE37CcKETKfPqGbCo576/
J/C0YC9SmXTmNMyZzhqiEuqAsIKHYndr9GZuRFbQhnUKEgSy7WStd8xsq2nsxL+a
k502pFkqEGNUhidD/1ciZwCd6yUBckC2InLNHhw6+3rfLBmiRSvmoAdU/6axHMcG
wdu/6l9DTdZ0D50MmfwFiw03h+CcafrEfPIb10khR1Bkvq+t3vgocplce79JuvBc
+Rez26b4CqBitvSB4RKG7zf+SoKzAmCgzpxYMHn2fRRZebnqNByngOIQlIFloUaQ
xtOiqxFB1gTw0Vd60INJVCq8wSVGNtLgO3CFrhg=
-----END CERTIFICATE-----
```

## 4. Open the Fleet output settings

Open up the burger menu in Kibana and navigate to Fleet settings.

![Kibana burger menu]({{site.baseurl}}/assets/img/docs/kibana-menu-fleet.png)

![Kibana Fleet settings]({{site.baseurl}}/assets/img/docs/kibana-fleet-settings.png)

![Kibana Fleet output navigation]({{site.baseurl}}/assets/img/docs/kibana-fleet-output-nav.png)

## 5. Edit the output settings

Change the hosts to `https://es01:9200`.

Add the fingerprint from step 2.

Add the certificate text to the "Advanced YAML configuration". It should look similiar to:

``` yaml
ssl:
    certificate_authorities:
    - |
        -----BEGIN CERTIFICATE-----
        MIIDSTCCAjGgAwIBAgIUN4teP7Njh5uMAVA3SBGVyzmNnmwwDQYJKoZIhvcNAQEL
        BQAwNDEyMDAGA1UEAxMpRWxhc3RpYyBDZXJ0aWZpY2F0ZSBUb29sIEF1dG9nZW5l
        cmF0ZWQgQ0EwHhcNMjMxMTEzMTg0ODA4WhcNMjYxMTEyMTg0ODA4WjA0MTIwMAYD
        VQQDEylFbGFzdGljIENlcnRpZmljYXRlIFRvb2wgQXV0b2dlbmVyYXRlZCBDQTCC
        ASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBALUBkAom2+Fk0SGneJhh7Ewf
        ExctEEUnuW9bifoSVhU8n6YG2YivKQsizh3rarJGP//CpiN2DW4rlh1UUYtTgv55
        1Ag2hwsoXhkpaMoIrFzmZQ7cgz4Ukve9/jdwfAtITeop/4pFlDpmUe/nLVzE3IDK
        jFMPsDOZmliVvddFoHh0bQmoWGQUP7Yp8TmIF+FGIDi0xriZ0+2+PSENTV+XXreK
        VQM5LMQl3CJB4rjzlP26az7+iGgf+QiMsun/gXSOp6Hllo1ktdHKEQ88JSFIZYgP
        zg6OZwCUgNDoJe4Yl2D7J8PTEt5uueCiq4wieZuYnIxSYEU/y8FwGTfM+AZWYt0C
        AwEAAaNTMFEwHQYDVR0OBBYEFEyHuybIj6A6J5/GqacL1mzo0HVcMB8GA1UdIwQY
        MBaAFEyHuybIj6A6J5/GqacL1mzo0HVcMA8GA1UdEwEB/wQFMAMBAf8wDQYJKoZI
        hvcNAQELBQADggEBAGm2yBpSeECV9VMT7qtpj5GCgl0jE37CcKETKfPqGbCo576/
        J/C0YC9SmXTmNMyZzhqiEuqAsIKHYndr9GZuRFbQhnUKEgSy7WStd8xsq2nsxL+a
        k502pFkqEGNUhidD/1ciZwCd6yUBckC2InLNHhw6+3rfLBmiRSvmoAdU/6axHMcG
        wdu/6l9DTdZ0D50MmfwFiw03h+CcafrEfPIb10khR1Bkvq+t3vgocplce79JuvBc
        +Rez26b4CqBitvSB4RKG7zf+SoKzAmCgzpxYMHn2fRRZebnqNByngOIQlIFloUaQ
        xtOiqxFB1gTw0Vd60INJVCq8wSVGNtLgO3CFrhg=
        -----END CERTIFICATE-----
```

### Example

![Kibana Fleet output example]({{site.baseurl}}/assets/img/docs/kibana-fleet-output-settings-example.png)

## 6. Save and deploy settings

Don't forget to click "Save and apply settings" -> "Save and deploy".

## 7. Review Elastic Agent data

After save and deploy is complete, head back to the agent tab, click on your agent name, and you should see CPU and Memory properly displayed, as well as Logs being populated.
