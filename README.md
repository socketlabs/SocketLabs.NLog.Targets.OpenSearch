[![SocketLabs](https://static.socketlabs.com/logos/v3/logo-dark.svg)](https://www.socketlabs.com/developers)  

[![Twitter Follow](https://img.shields.io/twitter/follow/socketlabs.svg?style=social&label=Follow)](https://x.com/socketlabs) 
[![MIT licensed](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md) 
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](CONTRIBUTING.md)
[![GitHub contributors](https://img.shields.io/github/contributors/socketlabs/SocketLabs.NLog.Targets.OpenSearch.svg)](https://github.com/socketlabs/SocketLabs.NLog.Targets.OpenSearch/graphs/contributors)
![Build status][build-status]  

# SocketLabs.NLog.Targets.OpenSearch
> NLog Target for OpenSearch

This NLog target is designed to specifically work with OpenSearch Versions >=1.0.  It is focused on performance, and doesn't require any third party clients.

## Getting Started

Install the Nuget package

### dotnet cli:
```bash
dotnet add package SocketLabs.NLog.Targets.OpenSearch
```

### Visual Studio
```powershell
Install-Package SocketLabs.NLog.Targets.OpenSearch
```  

### Add NLog.config to your project

```xml
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
            xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
            xsi:schemaLocation="http://www.nlog-project.org/schemas/NLog.xsd NLog.xsd"
            autoReload="true"
            throwExceptions="false"
            internalLogLevel="Warn" internalLogFile="logs/nlog-internal.log">

    <extensions>
        <add assembly="SocketLabs.NLog.Targets.OpenSearch"/>
    </extensions>

    <targets>
        <target xsi:type="OpenSearch"
                name="opensearch"
                uris="http://localhost:9200"
                username="logstash"
                password="Pa$$w0rd!"
                includeEventProperties="true"
                includeCallSite="true"
                includeCallSiteStackTrace="true"
                includeScopeProperties="true"
                enableHttpCompression="false"
                validateCertificates="false">

            <contextproperty name="appDomain" layout="${appdomain:format={1\}}" />
            <contextproperty name="assemblyVersion" layout="${assembly-version}" />
            <contextproperty name="dotnet.version" layout="${environment:DOTNET_VERSION}" />
            <contextproperty name="gc.collectionCount0" layout="${gc:property=CollectionCount0}" />
            <contextproperty name="gc.collectionCount1" layout="${gc:property=CollectionCount1}" />
            <contextproperty name="gc.collectionCount2" layout="${gc:property=CollectionCount2}" />
            <contextproperty name="gc.maxGeneration" layout="${gc:property=MaxGeneration}" />
            <contextproperty name="gc.totalMemory" layout="${gc:property=TotalMemory}" />
            <contextproperty name="gc.totalMemoryForceCollection" layout="${gc:property=TotalMemoryForceCollection}" />
            <contextproperty name="hostname" layout="${hostname}" />
            <contextproperty name="process.handleCount" layout="${processinfo:property=HandleCount}" />
            <contextproperty name="process.maxWorkingSet" layout="${processinfo:property=MaxWorkingSet}" />
            <contextproperty name="process.minWorkingSet" layout="${processinfo:property=MinWorkingSet}" />
            <contextproperty name="process.name" layout="${processname}" />
            <contextproperty name="process.peakWorkingSet64" layout="${processinfo:property=PeakWorkingSet64}" />
            <contextproperty name="process.pid" layout="${processinfo:property=Id}" />
            <contextproperty name="process.privateMemorySize64" layout="${processinfo:property=PrivateMemorySize64}" />
            <contextproperty name="process.startTime" layout="${processinfo:property=StartTime}" />
            <contextproperty name="process.virtualMemorySize64" layout="${processinfo:property=VirtualMemorySize64}" />
            <contextproperty name="process.workingSet64" layout="${processinfo:property=WorkingSet64}" />
            <contextproperty name="threadId" layout="${threadid}" />
            <contextproperty name="system.environment" layout="${systemEnvironment}" />
            <contextproperty name="kubernetes.namespace" layout="${environment:KUBERNETES_NAMESPACE}" />
        </target>
    </targets>

    <rules>
        <logger name="*" minlevel="Info" writeTo="opensearch" />
    </rules>
</nlog>
```


## Building
### Prerequisites

* Visual Studio or VSCode
* .NET 7

### Release History
* 1.0.0 Initial Release

### License
This code base is licensed under the [MIT License](LICENSE.md).

<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[build-status]: https://dev.azure.com/socketlabs/Public%20Projects/_apis/build/status/SocketLabs.NLog.Targets.OpenSearch-CI?branchName=main
