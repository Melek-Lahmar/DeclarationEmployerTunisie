param(
    [string]$Configuration = "Release",
    [string]$OutputRoot = "C:\DET2025_DEV\release"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$apiProject = Join-Path $repoRoot "src\DeclarationEmployer.Api\DeclarationEmployer.Api.csproj"
$desktopProject = Join-Path $repoRoot "src\DeclarationEmployer.Desktop\DeclarationEmployer.Desktop.csproj"

$apiOut = Join-Path $OutputRoot "api"
$desktopOut = Join-Path $OutputRoot "desktop"

New-Item -ItemType Directory -Force -Path $apiOut, $desktopOut | Out-Null

dotnet restore (Join-Path $repoRoot "DeclarationEmployerTunisie.sln")
dotnet publish $apiProject -c $Configuration -o $apiOut --no-restore
dotnet publish $desktopProject -c $Configuration -o $desktopOut --no-restore

Write-Host "Release publiee dans $OutputRoot"
Write-Host "API     : $apiOut"
Write-Host "Desktop : $desktopOut"
