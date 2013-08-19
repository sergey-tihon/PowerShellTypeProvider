F# Type Provider for PowerShell
======================

Requirements:

- .NET 4.5
- PowerShell 3.0

Related posts:

- [PowerShell Type Provider Announcement](http://sergeytihon.wordpress.com/2013/08/04/powershell-type-provider/)

#### "SharePoint 2013 Management" Sample ####

Install using the [NuGet package](https://www.nuget.org/packages/PowerShellTypeProvider/).

```fsharp
#r "System.Management.Automation.dll"
#r "Microsoft.SharePoint.PowerShell.dll"
#r "System.ServiceModel.dll"
#r "Microsoft.Sharepoint.dll"
#r @"..\packages\PowerShellTypeProvider.0.2.1-alpha\lib\net45\PowerShellTypeProvider.dll"

type PS = FSharp.PowerShell.PowerShellTypeProvider<
					PSSnapIns="Microsoft.SharePoint.PowerShell", 
					Is64BitRequired=true >

let jobs      = PS.``Get-SPTimerJob``()
let databases = PS.``Get-SPDatabase``()
```


