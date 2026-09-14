# Build script for VirtualDimension DSP mod.
# Uses the portable Roslyn compiler in .ref/roslyn and references the local game + BepInEx profile.
param(
    [switch]$SkipDeploy
)

$ErrorActionPreference = "Stop"
$root    = "d:\trae\newcode\xukongweiduta"
$game    = "d:\steam\all\steamapps\common\Dyson Sphere Program"
$managed = Join-Path $game "DSPGAME_Data\Managed"
$profile = Join-Path $env:APPDATA "Thunderstore Mod Manager\DataFolder\DysonSphereProgram\profiles\Default\BepInEx"
$core    = Join-Path $profile "core"
$plugins = Join-Path $profile "plugins"
$csc     = Join-Path $root ".ref\roslyn\tasks\net472\csc.exe"
$refdir  = Join-Path $root ".ref\nslib\ref\netstandard2.1"

$src      = Join-Path $root "src\VirtualDimension"
$build    = Join-Path $root "build"
$dist     = Join-Path $root "dist"
$pkgPlug  = Join-Path $dist "BepInEx\plugins\VirtualDimension"
$livePlug = Join-Path $plugins "VirtualDimension"

$rsp = Join-Path $root ".ref\build.rsp"
$utf8Bom = New-Object System.Text.UTF8Encoding($true)
$lines = New-Object System.Collections.Generic.List[string]
function Add-Line([string]$text) {
    $lines.Add($text -replace '\\','/') | Out-Null
}
function Add-Path([string]$prefix, [string]$path) {
    $lines.Add($prefix + '"' + ($path -replace '\\','/') + '"') | Out-Null
}

$lines.Add("/target:library")
$lines.Add("/nostdlib+")
$lines.Add("/langversion:9.0")
$lines.Add("/optimize+")
$lines.Add("/debug-")
$lines.Add("/nologo")
$lines.Add("/warn:2")
Add-Path "/out:" (Join-Path $build "VirtualDimension.dll")

# netstandard 2.1 reference assemblies
Get-ChildItem $refdir -Filter *.dll | ForEach-Object {
    Add-Path "/r:" $_.FullName
}

# Game managed assemblies: only Unity/Assembly-CSharp (desktop System.* facades would
# duplicate types from the netstandard2.1 reference pack).
Get-ChildItem $managed -Filter *.dll |
    Where-Object { $_.Name -like "UnityEngine*" -or $_.Name -like "Assembly-*" -or $_.Name -eq "XGamingRuntime.dll" } |
    ForEach-Object { Add-Path "/r:" $_.FullName }

# BepInEx + modding libraries
foreach ($dll in @(
    (Join-Path $core "BepInEx.dll"),
    (Join-Path $core "0Harmony.dll"),
    (Join-Path $plugins "CommonAPI-CommonAPI\CommonAPI.dll"),
    (Join-Path $plugins "CommonAPI-DSPModSave\DSPModSave.dll"),
    (Join-Path $plugins "xiaoye97-LDBTool\LDBTool.dll")
)) {
    if (Test-Path $dll) { Add-Path "/r:" $dll }
    else { throw "Missing reference: $dll" }
}

Get-ChildItem $src -Recurse -Filter *.cs | ForEach-Object {
    Add-Path "" $_.FullName
}

[System.IO.File]::WriteAllLines($rsp, $lines, $utf8Bom)

New-Item -ItemType Directory -Force -Path $build | Out-Null
Write-Host "=== Compiling ==="
& $csc /noconfig "@$rsp"
if ($LASTEXITCODE -ne 0) { throw "Compilation failed ($LASTEXITCODE)" }

Write-Host "=== Generating icon ==="
$iconTool = Join-Path $root ".ref\MakeIcon.exe"
& $csc /nologo "/out:$iconTool" (Join-Path $root "tools\MakeIcon.cs")
if ($LASTEXITCODE -ne 0) { throw "Icon tool compilation failed" }
& $iconTool $pkgPlug (Join-Path $dist "icon.png")
if ($LASTEXITCODE -ne 0) { throw "Icon generation failed" }

Write-Host "=== Packaging ==="
New-Item -ItemType Directory -Force -Path $pkgPlug | Out-Null
Remove-Item -Force -ErrorAction SilentlyContinue (Join-Path $pkgPlug "virtual-dimension-tower.png")
Copy-Item -Force (Join-Path $build "VirtualDimension.dll") $pkgPlug

Write-Host "=== Packaging upload zip ==="
$zip = Join-Path $dist "VirtualDimension.zip"
if (Test-Path $zip) { Remove-Item $zip -Force }
Push-Location $dist
try {
    Compress-Archive -Path "manifest.json", "icon.png", "README.md", "BepInEx" -DestinationPath "VirtualDimension.zip" -Force
} finally {
    Pop-Location
}
Write-Host "Upload zip: $zip"

if (-not $SkipDeploy) {
    Write-Host "=== Deploying to Thunderstore profile ==="
    New-Item -ItemType Directory -Force -Path $livePlug | Out-Null
    Copy-Item -Force (Join-Path $pkgPlug "*") $livePlug
    Write-Host "Deployed to: $livePlug"
}

Write-Host "BUILD OK"
