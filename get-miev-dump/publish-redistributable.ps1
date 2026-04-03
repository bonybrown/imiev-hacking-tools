$ErrorActionPreference = "Stop"

$projectPath = Join-Path $PSScriptRoot "get-miev-dump.csproj"
$publishProfile = "WinX86SelfContained"
$distDir = Join-Path $PSScriptRoot "dist"
$publishDir = Join-Path $distDir "win-x86"
$zipPath = Join-Path $distDir "get-miev-dump-win-x86.zip"

if (Test-Path $publishDir)
{
    Remove-Item -Path $publishDir -Recurse -Force
}

if (Test-Path $zipPath)
{
    Remove-Item -Path $zipPath -Force
}

dotnet publish $projectPath -c Release /p:PublishProfile=$publishProfile

Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zipPath -Force

Write-Host "Publish output: $publishDir"
Write-Host "Redistributable zip: $zipPath"