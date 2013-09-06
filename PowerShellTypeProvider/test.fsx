#r @"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\System.Management.Automation\v4.0_3.0.0.0__31bf3856ad364e35\System.Management.Automation.dll"
#r @"bin\Debug\PowerShellTypeProvider.dll"

type PS = FSharp.PowerShell.PowerShellTypeProvider< PSSnapIns="", Is64BitRequired=false >

PS.``Get-Item`` [|@"C:\"|]
