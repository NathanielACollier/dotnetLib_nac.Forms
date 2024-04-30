
$buildConfigurationName = "Release"

$projectFileInfo = Get-ChildItem -Path $PSScriptRoot | where { $_.Extension -eq ".csproj"} | select -First 1
Write-Host "Project file " $projectFileInfo.FullName
& dotnet @("publish", $projectFileInfo.FullName,"-c", $buildConfigurationName)

$package = Get-ChildItem "$PSScriptRoot\bin\$buildConfigurationName\" -Filter *.nupkg | sort-object -Descending -Property LastWriteTime | select -First 1
Write-Host "Package found " $package.FullName
$settings = (Get-Content '~/settings.json' | Out-String | ConvertFrom-Json)

<#
Example from: https://docs.microsoft.com/en-us/nuget/nuget-org/publish-a-package
dotnet nuget push AppLogger.1.0.0.nupkg --api-key <put key here> --source https://api.nuget.org/v3/index.json
#>

& dotnet @("nuget","push", $package.FullName, "--api-key", $settings.nugetOrgAPIKey, "--source",
"https://api.nuget.org/v3/index.json"
)