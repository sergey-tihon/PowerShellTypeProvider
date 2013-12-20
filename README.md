F# Type Provider for PowerShell
======================
**
This project is depreciated. Source code were merged into [Fsharp.Management](https://github.com/forki/FSharp.Management)
**

Related posts:

- [PowerShell Type Provider Announcement](http://sergeytihon.wordpress.com/2013/08/04/powershell-type-provider/)

#### "SharePoint 2013 Management" Sample ####

Install using the [NuGet package](https://www.nuget.org/packages/PowerShellTypeProvider/).

```fsharp
#r @"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.Management.Automation\v4.0_3.0.0.0__31bf3856ad364e35\System.Management.Automation.dll"
#r "Microsoft.SharePoint.PowerShell.dll"
#r "System.ServiceModel.dll"
#r "Microsoft.Sharepoint.dll"
#r @"..\packages\PowerShellTypeProvider.0.3.0\lib\net45\PowerShellTypeProvider.dll"

type PS = FSharp.PowerShell.PowerShellTypeProvider<
					PSSnapIns="Microsoft.SharePoint.PowerShell", 
					Is64BitRequired=true >

let jobs      = PS.``Get-SPTimerJob``()
let databases = PS.``Get-SPDatabase``()
```


