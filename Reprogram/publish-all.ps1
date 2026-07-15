$ErrorActionPreference = "Stop"

$repoRoot = $PSScriptRoot
$distDir = Join-Path $repoRoot "dist"
$zipPath = Join-Path $distDir "imiev-reprogram-tools-win-x86.zip"

$projects = @(
    @{ Name = "download-miev-firmware"; Dir = Join-Path $repoRoot "download-miev-firmware"; Aot = $true },
    @{ Name = "upload-miev-firmware";   Dir = Join-Path $repoRoot "upload-miev-firmware";   Aot = $true },
    @{ Name = "memory-explorer";        Dir = Join-Path $repoRoot "memory-explorer";        Aot = $false }
)

# Clean dist folder
if (Test-Path $distDir)
{
    Remove-Item -Path $distDir -Recurse -Force
}
New-Item -ItemType Directory -Path $distDir -Force | Out-Null

$publishDir = Join-Path $distDir "win-x86"
New-Item -ItemType Directory -Path $publishDir -Force | Out-Null

foreach ($project in $projects)
{
    $csproj = Join-Path $project.Dir "$($project.Name).csproj"
    Write-Host ""
    Write-Host "Publishing $($project.Name)..." -ForegroundColor Cyan

    if ($project.Aot)
    {
        dotnet publish $csproj -c Release `
            -r win-x86 `
            --self-contained true `
            /p:PublishAot=true `
            /p:DebugType=None `
            /p:DebugSymbols=false `
            -o $publishDir
    }
    else
    {
        # WinForms does not support trimming/AOT — use single-file self-contained instead
        dotnet publish $csproj -c Release `
            -r win-x86 `
            --self-contained true `
            /p:PublishSingleFile=true `
            /p:IncludeNativeLibrariesForSelfExtract=true `
            /p:DebugType=None `
            /p:DebugSymbols=false `
            -o $publishDir
    }

    if ($LASTEXITCODE -ne 0)
    {
        Write-Error "Failed to publish $($project.Name)"
        exit $LASTEXITCODE
    }
}

Write-Host ""
Write-Host "Creating zip..." -ForegroundColor Cyan
Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zipPath -Force

Write-Host ""
Write-Host "Publish output:  $publishDir" -ForegroundColor Green
Write-Host "Redistributable: $zipPath" -ForegroundColor Green
