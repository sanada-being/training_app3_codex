[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$OutputDirectory = "",
    [switch]$Clean
)

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryRoot = Split-Path -Parent $scriptRoot
$applicationOutput = Join-Path $repositoryRoot "Sales Management App\Sales Management App\bin\$Configuration"

if ([string]::IsNullOrWhiteSpace($OutputDirectory))
{
    $OutputDirectory = Join-Path $repositoryRoot "dist\SalesManagementApp"
}

$requiredApplicationFiles = @(
    "Sales Management App.exe",
    "Sales Management App.exe.config",
    "SalesManagementApp.Core.dll"
)

$distributionDataFiles = @(
    "products.csv",
    "inventory.csv",
    "inventory_history.csv",
    "週次売上集計ファイル.xlsx"
)

if (!(Test-Path $applicationOutput))
{
    throw "Build output not found: $applicationOutput . Build the WinForms project in Visual Studio with Configuration=$Configuration first."
}

$missingApplicationFiles = $requiredApplicationFiles | Where-Object {
    !(Test-Path (Join-Path $applicationOutput $_))
}

if ($missingApplicationFiles.Count -gt 0)
{
    $missingList = $missingApplicationFiles -join ", "
    throw "Distribution source files are missing from build output: $missingList"
}

if ($Clean -and (Test-Path $OutputDirectory))
{
    Remove-Item -Recurse -Force $OutputDirectory
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

foreach ($fileName in $requiredApplicationFiles)
{
    Copy-Item (Join-Path $applicationOutput $fileName) $OutputDirectory -Force
}

foreach ($fileName in $distributionDataFiles)
{
    $sourcePath = Join-Path $repositoryRoot $fileName
    if (Test-Path $sourcePath)
    {
        Copy-Item $sourcePath $OutputDirectory -Force
    }
}

$latestSalesFile = Get-ChildItem -Path $repositoryRoot -Filter "sales_*.csv" -File |
    Where-Object { $_.Name -match '^sales_\d{8}\.csv$' } |
    Sort-Object Name -Descending |
    Select-Object -First 1

if ($null -ne $latestSalesFile)
{
    Copy-Item $latestSalesFile.FullName $OutputDirectory -Force
}

$readmePath = Join-Path $OutputDirectory "README-distribution.txt"
$readmeLines = @(
    "Sales Management App distribution folder",
    "",
    "1. Install .NET Framework 4.7.2 or later on the target PC.",
    "2. Keep the exe, dll, config, and CSV files in this same folder.",
    "3. Start 'Sales Management App.exe'.",
    "",
    "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')",
    "Source build: $applicationOutput"
)
Set-Content -Path $readmePath -Value $readmeLines -Encoding UTF8

Write-Output "Distribution folder created: $OutputDirectory"
