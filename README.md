F# Type Provider for PowerShell
======================

Requirements

- .NET 4.0+
- PowerShell 3.0

Related posts:

- [PowerShell Type Provider Announcement](http://sergeytihon.wordpress.com/2013/08/04/powershell-type-provider/)

#### "SharePoint 2013 Management" Sample ####

```fsharp
#r "System.Management.Automation.dll"
#r "Microsoft.SharePoint.PowerShell.dll"
#r "System.ServiceModel.dll"
#r "PowerShellTypeProvider.dll"

type PS = FSharp.PowerShell.PowerShellTypeProvider<
				PSSnapIns="Microsoft.SharePoint.PowerShell", 
				Is64BitRequired=true >

PS.``Get-SPTimerJob``()
```


