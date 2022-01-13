<img src="https://static.socketlabs.com/logos/logo-dark-sans-tag-507x64.png" alt="SocketLabs Logo" width="253" height="32" />  

[![Build Status][build-status]](https://socketlabs.visualstudio.com/NLog.Targets.OpenSearch/_build/latest?definitionId=4&branchName=main)  

# NLog.Targets.OpenSearch
> NLog Target for OpenSearch

## Getting Started

Install the Nuget package

##### dotnet cli:
```bash
dotnet add package NLog.Targets.OpenSearch
```

##### Visual Studio
```powershell
Install-Package NLog.Targets.OpenSearch
```  

##### Add NLog.config to your project

```xml
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
			xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
			xsi:schemaLocation="http://www.nlog-project.org/schemas/NLog.xsd NLog.xsd"
			autoReload="true"
			throwExceptions="false"
			internalLogLevel="Warn" internalLogFile="logs\nlog-internal.log">

	<extensions>
		<add assembly="NLog.Targets.OpenSearch"/>
	</extensions>
	
	<targets>
		<target name="opensearch" xsi:type="BufferingWrapper" flushTimeout="30">
			<target xsi:type="OpenSearch"
					uris="https://localhost:9200/"
					username="logstash"
					password="logstash"
					index="nlog-selfservice-${date:format=yyyy.MM.dd}"
					retryCount ="10"
					batchSize="10"
					includeEventProperties="true"
					includeCallSite="true"
					IncludeCallSiteStackTrace="true"
					enableHttpCompression="true">

				<contextproperty name="MachineName" layout="${lowercase:machinename}" />
				<contextproperty name="ThreadId" layout="${threadid}" />
			</target>
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
* .NET 5

1. CI builds are configured in [Azure DevOps](https://socketlabs.visualstudio.com/NLog.Targets.OpenSearch/_build)

### Release History
* 1.0.0 Initial Release

### License
This code base is confidential and not for public distribution at this time.

<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[build-status]: https://socketlabs.visualstudio.com/NLog.Targets.OpenSearch/_apis/build/status/NLog.Targets.OpenSearch-Azure-CI?branchName=main%2Fazure-pipelines