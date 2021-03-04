$buildConfigurationName = "Debug"


$scriptParentFolderPath = [System.IO.Path]::GetDirectoryName( $MyInvocation.InvocationName )

$buildPath = [system.io.path]::Combine( $scriptParentFolderPath, "bin", $buildConfigurationName)
$nugetPackages = Get-ChildItem $buildPath -Filter *.nupkg
$package = $nugetPackages | Sort-Object -Descending -Property CreationTIme | Select-Object -First 1

$settings = (Get-Content '~/settings.json' | Out-String | ConvertFrom-Json)

<#
& nuget @("nuget" "push", $package.FullName, "--source", "github")
#>

<#
until the dotnet nuget client works use GPR

gpr push --api-key $GITHUB_ACCESS_TOKEN "YourPackage.1.2.3.nupkg"
#>
& gpr @("push", "--api-key", $settings.githubToken, $package.FullName)