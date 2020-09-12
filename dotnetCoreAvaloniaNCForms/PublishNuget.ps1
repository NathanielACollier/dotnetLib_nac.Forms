$buildConfigurationName = "Debug"

$buildPath = [system.io.path]::Combine( (Get-Location).Path, "bin", $buildConfigurationName)
$nugetPackages = Get-ChildItem $buildPath -Filter *.nupkg
$package = $nugetPackages | Sort-Object -Descending -Property CreationTIme | Select-Object -First 1

& dotnet @("nuget", "push", $package.FullName, "--source", "github")
