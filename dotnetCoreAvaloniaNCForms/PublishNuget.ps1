$buildConfigurationName = "Debug"

$package = Get-ChildItem "$PSScriptRoot\bin\$buildConfigurationName\" -Filter *.nupkg | sort -Descending -Property CreationTime | select -First 1

& dotnet @("nuget", "push", $package.FullName, "--source", "github")
