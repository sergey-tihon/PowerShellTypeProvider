@echo off
cls
".nuget\nuget.exe" "install" "FAKE" "-Pre" "-OutputDirectory" "build" "-ExcludeVersion"
"build\FAKE\tools\Fake.exe" build.fsx
pause