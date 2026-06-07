<#
  MES-Framework AI API Test
  Tests AI-P1, AI-P2, AI-P3 endpoints
#>
$ErrorActionPreference = "Stop"
$baseUrl = "http://localhost:5180"
$token = ""
$report = New-Object System.Collections.ArrayList
$failures = @()

function Write-Log {
    param([string]$Step, [int]$Status, [string]$Body, [string]$Desc)
    $null = $report.Add([PSCustomObject]@{ Step = $Step; Status = $Status; Body = $Body; Desc = $Desc })
    if ($Status -ge 200 -and $Status -lt 400) {
        Write-Host "[PASS] $Step (HTTP $Status)" -ForegroundColor Green
    } else {
        Write-Host "[FAIL] $Step (HTTP $Status)" -ForegroundColor Red
        $global:failures += $Step
    }
    if ($Desc) { Write-Host "  -> $Desc" -ForegroundColor Gray }
    if ($Body.Length -gt 400) { Write-Host "  $($Body.Substring(0,400))..." -ForegroundColor DarkGray }
    else { Write-Host "  $Body" -ForegroundColor DarkGray }
}

function Invoke-Api {
    param($Method = "GET", $Uri, $Body, $SkipAuth = $false)
    $headers = @{ "Content-Type" = "application/json" }
    if (-not $SkipAuth -and $token) { $headers["Authorization"] = "Bearer $token" }
    try {
        if ($Method -eq "GET") {
            $resp = Invoke-WebRequest -Uri $Uri -Method $Method -Headers $headers -UseBasicParsing
        } elseif ($Method -eq "DELETE") {
            $resp = Invoke-WebRequest -Uri $Uri -Method $Method -Headers $headers -UseBasicParsing
        } else {
            $resp = Invoke-WebRequest -Uri $Uri -Method $Method -Headers $headers -Body $Body -UseBasicParsing
        }
        return @{ Status = [int]$resp.StatusCode; Body = if ($resp.Content) { $resp.Content } else { "" } }
    } catch {
        $ex = $_.Exception
        if ($ex.Response) {
            $reader = New-Object System.IO.StreamReader($ex.Response.GetResponseStream())
            $errBody = $reader.ReadToEnd()
            $reader.Close()
            return @{ Status = [int]$ex.Response.StatusCode; Body = $errBody }
        }
        return @{ Status = 999; Body = "Exception: $($ex.Message)" }
    }
}

try {
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host "  MES-Framework AI API Test" -ForegroundColor White
    Write-Host "============================================" -ForegroundColor Cyan

    # STEP 1: Login
    Write-Host "`n=== STEP 1: AUTH ===" -ForegroundColor Cyan
    $r = Invoke-Api -Method Post -Uri "$baseUrl/api/v1/auth/login" -Body '{"username":"admin","password":"Admin@2026!"}' -SkipAuth $true
    Write-Log -Step "1a.AdminLogin" -Status $r.Status -Body $r.Body
    $tokenData = $r.Body | ConvertFrom-Json
    if ($tokenData.code -eq 0) { $token = $tokenData.data.token } else { throw "Login failed" }

    # STEP 2: Get test data
    Write-Host "`n=== STEP 2: PREPARE TEST DATA ===" -ForegroundColor Cyan

    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/work-orders?page=1&page_size=10"
    Write-Log -Step "2a.GetWorkOrders" -Status $r.Status -Body $r.Body
    $orders = ($r.Body | ConvertFrom-Json).data
    $workOrderId = if ($orders.Count -gt 0) { $orders[0].id } else { 1 }

    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/production-lines"
    Write-Log -Step "2b.GetLines" -Status $r.Status -Body $r.Body
    $lines = ($r.Body | ConvertFrom-Json).data
    $lineId = if ($lines.Count -gt 0) { $lines[0].id } else { 1 }

    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/equipment"
    Write-Log -Step "2c.GetEquipment" -Status $r.Status -Body $r.Body
    $equipment = ($r.Body | ConvertFrom-Json).data
    $equipmentId = if ($equipment.Count -gt 0) { $equipment[0].id } else { 1 }

    # STEP 3: AI-P1 Quality Alerts Tests
    Write-Host "`n=== STEP 3: AI-P1 QUALITY ALERTS ===" -ForegroundColor Cyan

    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/ai/quality/alerts"
    Write-Log -Step "3a.GetActiveAlerts" -Status $r.Status -Body $r.Body

    $r = Invoke-Api -Method Post -Uri "$baseUrl/api/v1/ai/quality/analyze" -Body '{}'
    Write-Log -Step "3b.AnalyzeQuality_All" -Status $r.Status -Body $r.Body

    $r = Invoke-Api -Method Post -Uri "$baseUrl/api/v1/ai/quality/analyze" -Body "{$workOrderId}"
    Write-Log -Step "3c.AnalyzeQuality_WorkOrder" -Status $r.Status -Body $r.Body

    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/ai/quality/history?page=1&page_size=20"
    Write-Log -Step "3d.GetAlertHistory" -Status $r.Status -Body $r.Body

    $r = Invoke-Api -Method Post -Uri "$baseUrl/api/v1/ai/quality/alerts/1/process" -Body '"admin"'
    Write-Log -Step "3e.ProcessAlert" -Status $r.Status -Body $r.Body

    # STEP 4: AI-P2 Scheduling Tests
    Write-Host "`n=== STEP 4: AI-P2 SCHEDULING ===" -ForegroundColor Cyan

    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/ai/scheduling/recommend/$workOrderId"
    Write-Log -Step "4a.GetSchedulingRecommendation" -Status $r.Status -Body $r.Body

    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/ai/scheduling/recommend/99999"
    Write-Log -Step "4b.GetRecommendation_NotFound" -Status $r.Status -Body $r.Body

    # STEP 5: AI-P3 Equipment Health Tests
    Write-Host "`n=== STEP 5: AI-P3 EQUIPMENT HEALTH ===" -ForegroundColor Cyan

    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/ai/equipment/health"
    Write-Log -Step "5a.GetAllEquipmentHealth" -Status $r.Status -Body $r.Body

    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/ai/equipment/health/$equipmentId"
    Write-Log -Step "5b.GetEquipmentHealth" -Status $r.Status -Body $r.Body

    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/ai/equipment/high-risk"
    Write-Log -Step "5c.GetHighRiskEquipment" -Status $r.Status -Body $r.Body

    # Summary
    Write-Host "`n============================================" -ForegroundColor Cyan
    Write-Host "  TEST SUMMARY" -ForegroundColor White
    Write-Host "============================================" -ForegroundColor Cyan

    $passed = ($report | Where-Object { $_.Status -ge 200 -and $_.Status -lt 400 }).Count
    $total = $report.Count
    Write-Host "  Total: $total | Passed: $passed | Failed: $($failures.Count)" -ForegroundColor $(if ($failures.Count -eq 0) { "Green" } else { "Yellow" })

    if ($failures.Count -gt 0) {
        Write-Host "`n  Failed tests:" -ForegroundColor Red
        $failures | ForEach-Object { Write-Host "    - $_" -ForegroundColor Red }
    }

    $report | Export-Csv -Path "$PSScriptRoot\ai-test-report.csv" -NoTypeInformation -Encoding UTF8
    Write-Host "`n  Report saved to: $PSScriptRoot\ai-test-report.csv" -ForegroundColor Gray

    if ($failures.Count -gt 0) { exit 1 } else { exit 0 }

} catch {
    Write-Host "[ERROR] $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}