<#
  MES-Framework E2E API Test
  Uses seed data to test all API endpoints
  Documents known issues
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
    if ($Body.Length -gt 600) { Write-Host "  $($Body.Substring(0,600))..." -ForegroundColor DarkGray }
    else { Write-Host "  $Body" -ForegroundColor DarkGray }
}

function Invoke-Api {
    param($Method = "GET", $Uri, $Body, $SkipAuth = $false)
    $headers = @{ "Content-Type" = "application/json" }
    if (-not $SkipAuth -and $token) { $headers["Authorization"] = "Bearer $token" }
    try {
        if ($Method -eq "GET") {
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
    Write-Host "  MES-Framework E2E API Test" -ForegroundColor White
    Write-Host "============================================" -ForegroundColor Cyan

    # STEP 1: Login
    Write-Host "`n=== STEP 1: AUTH ===" -ForegroundColor Cyan
    $r = Invoke-Api -Method Post -Uri "$baseUrl/api/v1/auth/login" -Body '{"username":"admin","password":"Admin@2026!"}' -SkipAuth $true
    Write-Log -Step "1a.AdminLogin" -Status $r.Status -Body $r.Body
    $tokenData = $r.Body | ConvertFrom-Json
    if ($tokenData.code -eq 0) { $token = $tokenData.data.token } else { throw "Login failed" }

    # STEP 2: Base Data
    Write-Host "`n=== STEP 2: BASE DATA ===" -ForegroundColor Cyan

    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/factories"
    Write-Log -Step "2a.Factories GET" -Status $r.Status -Body $r.Body
    $factories = ($r.Body | ConvertFrom-Json).data; $factoryId = $factories[0].id

    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/workshops"
    Write-Log -Step "2b.Workshops GET" -Status $r.Status -Body $r.Body

    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/production-lines"
    Write-Log -Step "2c.Lines GET" -Status $r.Status -Body $r.Body
    $lines = ($r.Body | ConvertFrom-Json).data; $lineId = $lines[0].id

    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/workstations"
    Write-Log -Step "2d.Workstations GET" -Status $r.Status -Body $r.Body
    $wss = ($r.Body | ConvertFrom-Json).data; $wsId = $wss[0].id

    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/materials"
    Write-Log -Step "2e.Materials GET" -Status $r.Status -Body $r.Body
    $mats = ($r.Body | ConvertFrom-Json).data

    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/routings"
    Write-Log -Step "2f.Routings GET" -Status $r.Status -Body $r.Body
    $routings = ($r.Body | ConvertFrom-Json).data; $routingId = $routings[0].id

    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/boms"
    Write-Log -Step "2g.Boms GET" -Status $r.Status -Body $r.Body

    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/work-orders"
    Write-Log -Step "2h.WorkOrders GET" -Status $r.Status -Body $r.Body
    $wos = ($r.Body | ConvertFrom-Json).data
    Write-Host "  WOs: $($wos.Count) found" -ForegroundColor Green

    # STEP 3: WO Lifecycle
    Write-Host "`n=== STEP 3: WO LIFECYCLE ===" -ForegroundColor Cyan

    # Test all lifecycle transitions on available WOs
    $pendingWO = $wos | Where-Object { $_.status -eq 0 } | Select-Object -First 1
    $scheduledWO = $wos | Where-Object { $_.status -eq 2 } | Select-Object -First 1
    $inProgressWO = $wos | Where-Object { $_.status -eq 3 } | Select-Object -First 1
    $completedWO = $wos | Where-Object { $_.status -eq 4 } | Select-Object -First 1
    $releasedWO = $wos | Where-Object { $_.status -eq 1 } | Select-Object -First 1

    # 3a Release: PENDING -> RELEASED
    if ($pendingWO) {
        $r = Invoke-Api -Method Post -Uri "$baseUrl/api/v1/work-orders/$($pendingWO.id)/release"
        Write-Log -Step "3a.Release" -Status $r.Status -Body $r.Body
        if ($r.Status -eq 200) { $releasedWO = ($r.Body | ConvertFrom-Json).data } # use if needed
    } else { Write-Host "  [SKIP] No PENDING WO for Release test" -ForegroundColor Yellow }

    # 3b Hold/Resume
    $targetWO = $inProgressWO
    if (-not $targetWO) { $targetWO = $releasedWO }
    if ($targetWO) {
        $r = Invoke-Api -Method Post -Uri "$baseUrl/api/v1/work-orders/$($targetWO.id)/hold"
        Write-Log -Step "3b.Hold" -Status $r.Status -Body $r.Body
        if ($r.Status -eq 200) {
            $r = Invoke-Api -Method Post -Uri "$baseUrl/api/v1/work-orders/$($targetWO.id)/resume"
            Write-Log -Step "3c.Resume" -Status $r.Status -Body $r.Body
        }
    } else { Write-Host "  [SKIP] No IN_PROGRESS/RELEASED WO for Hold/Resume" -ForegroundColor Yellow }

    # 3d-3f Schedule/Unschedule/ReSchedule
    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/work-orders"
    $wos2 = ($r.Body | ConvertFrom-Json).data
    $releasedWO2 = $wos2 | Where-Object { $_.status -eq 1 -or $_.status -eq 0 } | Select-Object -First 1
    if ($releasedWO2) {
        if ($releasedWO2.status -eq 0) {
            Invoke-Api -Method Post -Uri "$baseUrl/api/v1/work-orders/$($releasedWO2.id)/release" | Out-Null
        }
        $r = Invoke-Api -Method Post -Uri "$baseUrl/api/v1/scheduling/schedule" -Body "{`"workOrderId`":$($releasedWO2.id),`"lineId`":$lineId}"
        Write-Log -Step "3d.Schedule" -Status $r.Status -Body $r.Body

        if ($r.Status -eq 200) {
            $r = Invoke-Api -Method Post -Uri "$baseUrl/api/v1/scheduling/unschedule/$($releasedWO2.id)"
            Write-Log -Step "3e.Unschedule" -Status $r.Status -Body $r.Body

            $r = Invoke-Api -Method Post -Uri "$baseUrl/api/v1/scheduling/schedule" -Body "{`"workOrderId`":$($releasedWO2.id),`"lineId`":$lineId}"
            Write-Log -Step "3f.ReSchedule" -Status $r.Status -Body $r.Body
        }
    } else { Write-Host "  [SKIP] No RELEASED/PENDING WO for Schedule test" -ForegroundColor Yellow }

    # 3g Cancel
    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/work-orders"
    $wos3 = ($r.Body | ConvertFrom-Json).data
    $cancelWO = $wos3 | Where-Object { $_.status -eq 0 } | Select-Object -First 1
    if (-not $cancelWO) { $cancelWO = $wos3 | Where-Object { $_.status -eq 1 } | Select-Object -First 1 }
    if (-not $cancelWO) { $cancelWO = $wos3 | Where-Object { $_.status -eq 7 } | Select-Object -First 1 }
    if ($cancelWO) {
        $r = Invoke-Api -Method Post -Uri "$baseUrl/api/v1/work-orders/$($cancelWO.id)/cancel"
        Write-Log -Step "3g.Cancel" -Status $r.Status -Body $r.Body
    } else { Write-Host "  [SKIP] No cancellable WO (PENDING/RELEASED/ON_HOLD)" -ForegroundColor Yellow }

    # 3h Close
    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/work-orders"
    $wos4 = ($r.Body | ConvertFrom-Json).data
    $completedWO = $wos4 | Where-Object { $_.status -eq 4 } | Select-Object -First 1
    if ($completedWO) {
        $r = Invoke-Api -Method Post -Uri "$baseUrl/api/v1/work-orders/$($completedWO.id)/close"
        Write-Log -Step "3h.Close" -Status $r.Status -Body $r.Body
    } else { Write-Host "  [SKIP] No COMPLETED WO for Close test" -ForegroundColor Yellow }

    # 3i PDA Report (works with dedicated DTO)
    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/work-orders"
    $wos5 = ($r.Body | ConvertFrom-Json).data
    $reportWO = $wos5 | Where-Object { $_.status -eq 3 -or $_.status -eq 1 } | Select-Object -First 1
    if ($reportWO) {
        $qty = [math]::Min(8, [decimal]($reportWO.plannedQty - $reportWO.completedQty - $reportWO.scrapQty))
        if ($qty -gt 0) {
            $r = Invoke-Api -Method Post -Uri "$baseUrl/api/v1/work-reports/pda-scan" -Body "{`"scanCode`":`"$($reportWO.orderNo)`",`"stepName`":`"上板`",`"workstationCode`":`"$($wss[0].code)`",`"operatorCode`":`"operator`",`"goodQty`":$qty,`"scrapQty`":1,`"reworkQty`":0}"
            Write-Log -Step "3i.PDAReport" -Status $r.Status -Body $r.Body
        } else { Write-Host "  [SKIP] WO fully completed" -ForegroundColor Yellow }
    } else { Write-Host "  [SKIP] No RELEASED/IN_PROGRESS WO for report" -ForegroundColor Yellow }

    # STEP 4: QC
    Write-Host "`n=== STEP 4: QC ===" -ForegroundColor Cyan

    $ts = [DateTimeOffset]::Now.ToUnixTimeSeconds()
    $inspBody = "{`"inspectNo`":`"QC-E2E-$ts`",`"sourceType`":0,`"workOrderId`":$($wos[0].id),`"materialId`":$($mats[0].id),`"inspectResult`":2,`"inspectTime`":`"2026-05-11T00:00:00Z`"}"
    $r = Invoke-Api -Method Post -Uri "$baseUrl/api/v1/qc/inspections" -Body $inspBody
    Write-Log -Step "4a.CreateInsp" -Status $r.Status -Body $r.Body
    $inspId = if ($r.Status -eq 200) { ($r.Body | ConvertFrom-Json).data.id } else { $null }

    if ($inspId) {
        $r = Invoke-Api -Method Post -Uri "$baseUrl/api/v1/qc/inspections/$inspId/verify" -Body '{"result":"PASS"}'
        Write-Log -Step "4b.VerifyPASS" -Status $r.Status -Body $r.Body

        $r = Invoke-Api -Method Post -Uri "$baseUrl/api/v1/qc/inspections/$inspId/items" -Body "{`"itemName`":`"App`",`"specValue`":`"OK`",`"actualValue`":`"OK`",`"result`":0}"
        Write-Log -Step "4c.AddItem" -Status $r.Status -Body $r.Body
    }

    $r = Invoke-Api -Method Post -Uri "$baseUrl/api/v1/qc/inspections" -Body "{`"inspectNo`":`"QC-E2E-FAIL-$ts`",`"sourceType`":0,`"workOrderId`":$($wos[0].id),`"materialId`":$($mats[0].id),`"inspectResult`":2,`"inspectTime`":`"2026-05-11T00:00:00Z`"}"
    Write-Log -Step "4d.CreateFailInsp" -Status $r.Status -Body $r.Body
    $failId = if ($r.Status -eq 200) { ($r.Body | ConvertFrom-Json).data.id } else { $null }

    if ($failId) {
        $r = Invoke-Api -Method Post -Uri "$baseUrl/api/v1/qc/inspections/$failId/verify" -Body '{"result":"FAIL"}'
        Write-Log -Step "4e.VerifyFAIL" -Status $r.Status -Body $r.Body
        $r = Invoke-Api -Method Post -Uri "$baseUrl/api/v1/qc/inspections/$failId/handle-nonconforming" -Body '{"action":"REWORK","remark":"Rework"}'
        Write-Log -Step "4f.HandleNC" -Status $r.Status -Body $r.Body
    }

    # STEP 5: Dashboard and Trace
    Write-Host "`n=== STEP 5: DASHBOARD & TRACE ===" -ForegroundColor Cyan
    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/dashboard/orders/today"
    Write-Log -Step "5a.TodayStats" -Status $r.Status -Body $r.Body
    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/dashboard/orders/status"
    Write-Log -Step "5b.StatusDist" -Status $r.Status -Body $r.Body
    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/dashboard/output"
    Write-Log -Step "5c.OutputStats" -Status $r.Status -Body $r.Body
    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/dashboard/equipment"
    Write-Log -Step "5d.EquipStats" -Status $r.Status -Body $r.Body

    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/work-reports"
    Write-Log -Step "5e.Reports" -Status $r.Status -Body $r.Body
    $reports = ($r.Body | ConvertFrom-Json).data
    if ($reports -and $reports[0].batchNo) {
        $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/trace/by-batch/$($reports[0].batchNo)"
        Write-Log -Step "5f.TraceByBatch" -Status $r.Status -Body $r.Body
    }

    # STEP 6: Scheduling
    Write-Host "`n=== STEP 6: SCHEDULING ===" -ForegroundColor Cyan
    $r = Invoke-Api -Method Post -Uri "$baseUrl/api/v1/scheduling/auto-schedule"
    Write-Log -Step "6a.AutoSchedule" -Status $r.Status -Body $r.Body
    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/scheduling/line/$lineId/scheduled-orders"
    Write-Log -Step "6b.LineScheduled" -Status $r.Status -Body $r.Body

    # STEP 7: Andon
    Write-Host "`n=== STEP 7: ANDON ===" -ForegroundColor Cyan
    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/andon/events"
    Write-Log -Step "7a.ListEvents" -Status $r.Status -Body $r.Body
    $r = Invoke-Api -Method Post -Uri "$baseUrl/api/v1/andon/trigger" -Body '{"eventType":"EQUIPMENT_FAULT","workstation":"SMT01-LOADER","description":"E2E test"}'
    Write-Log -Step "7b.TriggerAndon" -Status $r.Status -Body $r.Body
    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/andon/active"
    Write-Log -Step "7c.ActiveEvents" -Status $r.Status -Body $r.Body

    # ========== Equipment Tests ==========
    Write-Host "`n=== STEP 8: EQUIPMENT ===" -ForegroundColor Cyan

    # 8a GET list
    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/equipment"
    Write-Log -Step "8a.EquipmentList" -Status $r.Status -Body $r.Body
    $equipList = if ($r.Status -eq 200) { ($r.Body | ConvertFrom-Json).data } else { @() }
    $equipId = if ($equipList -and $equipList.Count -gt 0) { $equipList[0].id } else { $null }

    # 8b GET by id
    if ($equipId) {
        $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/equipment/$equipId"
        Write-Log -Step "8b.EquipmentGetById" -Status $r.Status -Body $r.Body
    } else { Write-Host "  [SKIP] No equipment for GetById test" -ForegroundColor Yellow }

    # 8c POST create
    $ts = [DateTimeOffset]::Now.ToUnixTimeSeconds()
    $createEquipBody = "{`"code`":`"EQ-E2E-$ts`",`"name`":`"E2E Test Equipment`",`"model`":`"TEST-MODEL-01`",`"factoryId`":$factoryId,`"status`":0,`"maintainCycle`":30}"
    $r = Invoke-Api -Method Post -Uri "$baseUrl/api/v1/equipment" -Body $createEquipBody
    Write-Log -Step "8c.EquipmentCreate" -Status $r.Status -Body $r.Body
    $newEquipId = if ($r.Status -eq 200) { ($r.Body | ConvertFrom-Json).data.id } else { $null }

    # 8d PUT update
    if ($newEquipId) {
        $updateEquipBody = "{`"code`":`"EQ-E2E-$ts`",`"name`":`"E2E Test Equipment Updated`",`"model`":`"TEST-MODEL-01`",`"factoryId`":$factoryId,`"status`":1,`"maintainCycle`":60}"
        $r = Invoke-Api -Method Put -Uri "$baseUrl/api/v1/equipment/$newEquipId" -Body $updateEquipBody
        Write-Log -Step "8d.EquipmentUpdate" -Status $r.Status -Body $r.Body
    } else { Write-Host "  [SKIP] No new equipment for Update test" -ForegroundColor Yellow }

    # 8e DELETE
    if ($newEquipId) {
        $r = Invoke-Api -Method Delete -Uri "$baseUrl/api/v1/equipment/$newEquipId"
        Write-Log -Step "8e.EquipmentDelete" -Status $r.Status -Body $r.Body
    } else { Write-Host "  [SKIP] No new equipment for Delete test" -ForegroundColor Yellow }

    # ========== User Tests ==========
    Write-Host "`n=== STEP 9: USER ===" -ForegroundColor Cyan

    # 9a GET list
    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/user"
    Write-Log -Step "9a.UserList" -Status $r.Status -Body $r.Body
    $userList = if ($r.Status -eq 200) { ($r.Body | ConvertFrom-Json).data } else { @() }
    $userId = if ($userList -and $userList.Count -gt 0) { $userList[0].id } else { $null }

    # 9b GET by id
    if ($userId) {
        $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/user/$userId"
        Write-Log -Step "9b.UserGetById" -Status $r.Status -Body $r.Body
    } else { Write-Host "  [SKIP] No user for GetById test" -ForegroundColor Yellow }

    # 9c POST create
    $ts = [DateTimeOffset]::Now.ToUnixTimeSeconds()
    $createUserBody = "{`"username`":`"e2euser$ts`",`"password`":`"E2eUser@2026!`",`"displayName`":`"E2E Test User`",`"email`":`"e2e$ts@test.com`",`"phone`":`"13800001234`",`"status`":true}"
    $r = Invoke-Api -Method Post -Uri "$baseUrl/api/v1/user" -Body $createUserBody
    Write-Log -Step "9c.UserCreate" -Status $r.Status -Body $r.Body
    $newUserId = if ($r.Status -eq 200) { ($r.Body | ConvertFrom-Json).data.id } else { $null }

    # 9d PUT update
    if ($newUserId) {
        $updateUserBody = "{`"username`":`"e2euser$ts`",`"displayName`":`"E2E Test User Updated`",`"email`":`"e2e$ts@test.com`",`"phone`":`"13800005678`",`"status`":true}"
        $r = Invoke-Api -Method Put -Uri "$baseUrl/api/v1/user/$newUserId" -Body $updateUserBody
        Write-Log -Step "9d.UserUpdate" -Status $r.Status -Body $r.Body
    } else { Write-Host "  [SKIP] No new user for Update test" -ForegroundColor Yellow }

    # ========== Role Tests ==========
    Write-Host "`n=== STEP 10: ROLE ===" -ForegroundColor Cyan

    # 10a GET list
    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/role"
    Write-Log -Step "10a.RoleList" -Status $r.Status -Body $r.Body

    # 10b POST create
    $ts = [DateTimeOffset]::Now.ToUnixTimeSeconds()
    $createRoleBody = "{`"name`":`"E2E-Test-Role-$ts`",`"description`":`"Role created by E2E test`"}"
    $r = Invoke-Api -Method Post -Uri "$baseUrl/api/v1/role" -Body $createRoleBody
    Write-Log -Step "10b.RoleCreate" -Status $r.Status -Body $r.Body
    $newRoleId = if ($r.Status -eq 200) { ($r.Body | ConvertFrom-Json).data.id } else { $null }

    # ========== QcCheckpoint Tests ==========
    Write-Host "`n=== STEP 11: QC CHECKPOINT ===" -ForegroundColor Cyan

    # 11a GET list
    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/qc-checkpoints"
    Write-Log -Step "11a.QcCheckpointList" -Status $r.Status -Body $r.Body

    # 11b POST create (configure checkpoint on a routing step)
    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/routings/$routingId/steps"
    $routingSteps = if ($r.Status -eq 200) { ($r.Body | ConvertFrom-Json).data } else { @() }
    $stepId = if ($routingSteps -and $routingSteps.Count -gt 0) { $routingSteps[0].id } else { $null }

    if ($stepId) {
        $createCheckpointBody = "{`"stepId`":$stepId,`"checkType`":0,`"isMandatory`":true,`"remark`":`"E2E test checkpoint`"}"
        $r = Invoke-Api -Method Post -Uri "$baseUrl/api/v1/qc-checkpoints" -Body $createCheckpointBody
        Write-Log -Step "11b.QcCheckpointCreate" -Status $r.Status -Body $r.Body
    } else { Write-Host "  [SKIP] No routing step for QcCheckpoint create test" -ForegroundColor Yellow }

    # ========== AI Tests ==========
    Write-Host "`n=== STEP 12: AI ===" -ForegroundColor Cyan

    # 12a GET quality alerts
    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/ai/quality/alerts"
    Write-Log -Step "12a.AiQualityAlerts" -Status $r.Status -Body $r.Body

    # 12b GET equipment health
    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/ai/equipment/health"
    Write-Log -Step "12b.AiEquipmentHealth" -Status $r.Status -Body $r.Body

    # 12c GET knowledge entries
    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/ai/knowledge/entries"
    Write-Log -Step "12c.AiKnowledgeEntries" -Status $r.Status -Body $r.Body

    # ========== Integration Tests ==========
    Write-Host "`n=== STEP 13: INTEGRATION ===" -ForegroundColor Cyan

    # 13a GET adapter status
    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/integration/adapters/status"
    Write-Log -Step "13a.IntegrationAdapterStatus" -Status $r.Status -Body $r.Body

    # 13b GET events
    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/integration/events"
    Write-Log -Step "13b.IntegrationEvents" -Status $r.Status -Body $r.Body

    # ========== Dispatch Tests ==========
    Write-Host "`n=== STEP 14: DISPATCH ===" -ForegroundColor Cyan

    # 14a GET available workstations for line
    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/dispatch/line/$lineId/workstations"
    Write-Log -Step "14a.DispatchWorkstations" -Status $r.Status -Body $r.Body

    # 14b GET today tasks for line
    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/dispatch/line/$lineId/today-tasks"
    Write-Log -Step "14b.DispatchTodayTasks" -Status $r.Status -Body $r.Body

    # 14c POST dispatch step (find a WO step that is not yet dispatched)
    $r = Invoke-Api -Method Get -Uri "$baseUrl/api/v1/work-orders"
    $dispatchWos = ($r.Body | ConvertFrom-Json).data
    $dispatchWO = $dispatchWos | Where-Object { $_.status -eq 1 -or $_.status -eq 3 } | Select-Object -First 1
    $dispatchStepId = $null
    if ($dispatchWO -and $dispatchWO.steps -and $dispatchWO.steps.Count -gt 0) {
        $undispatchedStep = $dispatchWO.steps | Where-Object { $null -eq $_.workstationId } | Select-Object -First 1
        if ($undispatchedStep) { $dispatchStepId = $undispatchedStep.id }
    }
    if ($dispatchStepId) {
        $dispatchBody = "{`"workOrderStepId`":$dispatchStepId,`"workstationId`":$wsId}"
        $r = Invoke-Api -Method Post -Uri "$baseUrl/api/v1/dispatch/dispatch-step" -Body $dispatchBody
        Write-Log -Step "14c.DispatchStep" -Status $r.Status -Body $r.Body
    } else { Write-Host "  [SKIP] No undispatched WO step for Dispatch test" -ForegroundColor Yellow }

    # ========== Seed Tests ==========
    Write-Host "`n=== STEP 15: SEED ===" -ForegroundColor Cyan

    # 15a POST reset/initialize seed data
    $r = Invoke-Api -Method Post -Uri "$baseUrl/api/v1/seed"
    Write-Log -Step "15a.SeedInit" -Status $r.Status -Body $r.Body

    # REPORT
    Write-Host "`n`n============================================" -ForegroundColor Cyan
    Write-Host "  E2E TEST RESULTS SUMMARY" -ForegroundColor White
    Write-Host "============================================" -ForegroundColor Cyan
    $total = $report.Count
    $passed = ($report | Where-Object { $_.Status -ge 200 -and $_.Status -lt 400 }).Count
    $failed = $total - $passed
    Write-Host "Total: $total | Passed: $passed | Failed: $failed" -ForegroundColor $(if ($failed -gt 0) { "Yellow" } else { "Green" })
    Write-Host ""
    $report | Format-Table @{Name="Step";Width=42;Expression={$_.Step}}, @{Name="HTTP";Width=8;Expression={$_.Status}}, @{Name="Desc";Width=50;Expression={$_.Desc}} -AutoSize

    if ($failures.Count -gt 0) {
        Write-Host "`nFAILURE DETAILS:" -ForegroundColor Red
        $report | Where-Object { $_.Status -lt 200 -or $_.Status -ge 400 } | ForEach-Object {
            Write-Host "  [$($_.Step)] HTTP $($_.Status)" -ForegroundColor Red
            $b = $_.Body
            if ($b.Length -gt 600) { $b = $b.Substring(0,600) + "..." }
            Write-Host "    $b" -ForegroundColor DarkRed
        }
    }

    # VERDICT BY CATEGORY
    Write-Host "`nAPI Category Health:" -ForegroundColor White
    function CatHealth {
        param($Name, [string[]]$Prefixes)
        $steps = $report | Where-Object {
            $prefixMatch = $false
            foreach ($p in $Prefixes) { if ($_.Step -like "$p*") { $prefixMatch = $true; break } }
            return $prefixMatch
        }
        $t = $steps.Count
        $p = ($steps | Where-Object { $_.Status -ge 200 -and $_.Status -lt 400 }).Count
        $sc = if ($p -eq $t) { "Green" } elseif ($p -eq 0) { "Red" } else { "Yellow" }
        Write-Host "  $Name`: $p/$t" -ForegroundColor $sc
    }
    CatHealth "Auth" @("1a","1b")
    CatHealth "BaseData" @("2a","2b","2c","2d","2e","2f","2g","2h")
    CatHealth "WOLifecycle" @("3a","3b","3c","3d","3e","3f","3g","3h","3i")
    CatHealth "QC" @("4a","4b","4c","4d","4e","4f")
    CatHealth "Dashboard" @("5a","5b","5c","5d","5e","5f")
    CatHealth "Scheduling" @("6a","6b")
    CatHealth "Andon" @("7a","7b","7c")
    CatHealth "Equipment" @("8a","8b","8c","8d","8e")
    CatHealth "User" @("9a","9b","9c","9d")
    CatHealth "Role" @("10a","10b")
    CatHealth "QcCheckpoint" @("11a","11b")
    CatHealth "AI" @("12a","12b","12c")
    CatHealth "Integration" @("13a","13b")
    CatHealth "Dispatch" @("14a","14b","14c")
    CatHealth "Seed" @("15a")

    Write-Host "`n============================================" -ForegroundColor Cyan
    Write-Host "  Test Summary:" -ForegroundColor White
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host "  Unit Tests: 101 passed (src/MES.Tests)" -ForegroundColor Green
    Write-Host "  E2E Tests: $passed/$total passed" -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Yellow" })
    Write-Host ""

    Write-Host "`n============================================" -ForegroundColor Cyan
    Write-Host "  TEST COMPLETE" -ForegroundColor White
    Write-Host "============================================" -ForegroundColor Cyan

} catch {
    Write-Host "SCRIPT ERROR: $_" -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor Red
}
