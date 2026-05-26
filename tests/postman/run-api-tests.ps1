<#
    MES Framework API Test Runner
    Uses Newman (Postman CLI) to run API tests
#>
$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $PSScriptRoot
$postmanDir = Join-Path $projectRoot "tests\postman"
$collectionFile = Join-Path $postmanDir "MES-Framework.postman_collection.json"
$environmentFile = Join-Path $postmanDir "mes-local.postman_environment.json"
$reportFile = Join-Path $postmanDir "report.html"

# Check if newman is installed
function Test-NewmanInstalled {
    try {
        $null = Get-Command newman -ErrorAction Stop
        return $true
    } catch {
        return $false
    }
}

# Ensure backend is running
function Test-BackendRunning {
    param([string]$baseUrl = "http://localhost:5180")
    try {
        $response = Invoke-WebRequest -Uri $baseUrl -Method GET -UseBasicParsing -TimeoutSec 5
        return $true
    } catch {
        return $false
    }
}

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  MES Framework API Test Runner" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan

# Check prerequisites
Write-Host "`n=== Checking Prerequisites ===" -ForegroundColor Yellow

# Check newman
if (-not (Test-NewmanInstalled)) {
    Write-Host "[ERROR] Newman is not installed." -ForegroundColor Red
    Write-Host "Please install newman first:" -ForegroundColor Yellow
    Write-Host "  npm install -g newman" -ForegroundColor White
    Write-Host "  # OR" -ForegroundColor White
    Write-Host "  choco install newman" -ForegroundColor White
    exit 1
}
Write-Host "[OK] Newman is installed" -ForegroundColor Green

# Check files exist
if (-not (Test-Path $collectionFile)) {
    Write-Host "[ERROR] Collection file not found: $collectionFile" -ForegroundColor Red
    exit 1
}
Write-Host "[OK] Collection file found: MES-Framework.postman_collection.json" -ForegroundColor Green

if (-not (Test-Path $environmentFile)) {
    Write-Host "[ERROR] Environment file not found: $environmentFile" -ForegroundColor Red
    exit 1
}
Write-Host "[OK] Environment file found: mes-local.postman_environment.json" -ForegroundColor Green

# Check backend
Write-Host "`n=== Checking Backend Service ===" -ForegroundColor Yellow
$baseUrl = "http://localhost:5180"
if (-not (Test-BackendRunning -baseUrl $baseUrl)) {
    Write-Host "[ERROR] Backend is not running at $baseUrl" -ForegroundColor Red
    Write-Host "Please start the backend first:" -ForegroundColor Yellow
    Write-Host "  cd src\MES.Api; dotnet run" -ForegroundColor White
    Write-Host "  # OR use Docker:" -ForegroundColor White
    Write-Host "  docker-compose up -d" -ForegroundColor White
    exit 1
}
Write-Host "[OK] Backend is running at $baseUrl" -ForegroundColor Green

# Run tests
Write-Host "`n=== Running API Tests ===" -ForegroundColor Yellow
Write-Host "Collection: MES-Framework.postman_collection.json" -ForegroundColor White
Write-Host "Environment: mes-local.postman_environment.json" -ForegroundColor White
Write-Host "Report: tests\postman\report.html" -ForegroundColor White
Write-Host ""

# Run newman with HTML reporter
$newmanArgs = @(
    "run", $collectionFile,
    "-e", $environmentFile,
    "--delay-request", "500",
    "-r", "cli,html",
    "--reporter-html-export", $reportFile,
    "--verbose"
)

try {
    & newman @newmanArgs

    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n============================================" -ForegroundColor Green
        Write-Host "  ALL TESTS PASSED!" -ForegroundColor Green
        Write-Host "============================================" -ForegroundColor Green
        Write-Host "Report saved to: tests\postman\report.html" -ForegroundColor White
    } else {
        Write-Host "`n============================================" -ForegroundColor Yellow
        Write-Host "  SOME TESTS FAILED" -ForegroundColor Yellow
        Write-Host "============================================" -ForegroundColor Yellow
        Write-Host "Exit code: $LASTEXITCODE" -ForegroundColor Red
    }
} catch {
    Write-Host "`n[ERROR] Failed to run newman: $_" -ForegroundColor Red
    exit 1
}

exit $LASTEXITCODE
