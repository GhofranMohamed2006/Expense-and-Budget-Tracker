# Pennywise Authentication Module — Integration Test Suite
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "Pennywise Authentication Automated Test Suite" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan

$serverProcess = $null
$port = 5005
$baseUrl = "http://localhost:$port"

try {
    # 0. Clean test DB
    if (Test-Path "$PSScriptRoot\Backend\pennywise_test.db") {
        Remove-Item "$PSScriptRoot\Backend\pennywise_test.db" -Force -ErrorAction SilentlyContinue
    }

    # 1. Start backend server
    Write-Host "`n[1/12] Starting Pennywise backend server on port $port..." -ForegroundColor Yellow
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = "dotnet"
    $psi.Arguments = "run"
    $psi.WorkingDirectory = "$PSScriptRoot\Backend"
    $psi.UseShellExecute = $false
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.EnvironmentVariables["PORT"] = "$port"
    $psi.EnvironmentVariables["PENNYWISE_DB_CONN"] = "Data Source=pennywise_test.db"

    $serverProcess = [System.Diagnostics.Process]::Start($psi)
    Start-Sleep -Seconds 3

    # 2. Test Health Endpoint
    Write-Host "`n[2/12] Testing GET /api/auth/health..." -ForegroundColor Yellow
    $healthResp = Invoke-RestMethod -Uri "$baseUrl/api/auth/health" -Method Get
    if ($healthResp.status -eq "healthy") {
        Write-Host "PASS: Health check returned healthy status." -ForegroundColor Green
    } else {
        throw "FAIL: Health check failed: $($healthResp | ConvertTo-Json)"
    }

    # 3. Test Missing Fields in Registration (Expect 400)
    Write-Host "`n[3/12] Testing POST /api/auth/register with empty payload (Expect 400)..." -ForegroundColor Yellow
    try {
        Invoke-RestMethod -Uri "$baseUrl/api/auth/register" -Method Post -ContentType "application/json" -Body "{}"
        throw "FAIL: Expected 400 but request succeeded."
    } catch {
        if ($_.Exception.Response.StatusCode.value__ -eq 400) {
            Write-Host "PASS: Received expected 400 Bad Request for missing fields." -ForegroundColor Green
        } else {
            throw "FAIL: Unexpected status code: $($_.Exception.Response.StatusCode.value__)"
        }
    }

    # 4. Test Invalid Email Format in Registration (Expect 400)
    Write-Host "`n[4/12] Testing POST /api/auth/register with invalid email (Expect 400)..." -ForegroundColor Yellow
    try {
        $invalidEmailBody = @{
            name = "Test User"
            email = "invalid-email-address"
            password = "Password123!"
            confirmPassword = "Password123!"
        } | ConvertTo-Json
        Invoke-RestMethod -Uri "$baseUrl/api/auth/register" -Method Post -ContentType "application/json" -Body $invalidEmailBody
        throw "FAIL: Expected 400 for invalid email but request succeeded."
    } catch {
        if ($_.Exception.Response.StatusCode.value__ -eq 400) {
            Write-Host "PASS: Received expected 400 Bad Request for invalid email format." -ForegroundColor Green
        } else {
            throw "FAIL: Unexpected status code: $($_.Exception.Response.StatusCode.value__)"
        }
    }

    # 5. Test Password Confirmation Mismatch (Expect 400)
    Write-Host "`n[5/12] Testing POST /api/auth/register with mismatched passwords (Expect 400)..." -ForegroundColor Yellow
    try {
        $mismatchBody = @{
            name = "Ahmed Mohamed"
            email = "ahmed@example.com"
            password = "ExamplePassword123!"
            confirmPassword = "DifferentPassword456!"
        } | ConvertTo-Json
        Invoke-RestMethod -Uri "$baseUrl/api/auth/register" -Method Post -ContentType "application/json" -Body $mismatchBody
        throw "FAIL: Expected 400 for password mismatch but request succeeded."
    } catch {
        if ($_.Exception.Response.StatusCode.value__ -eq 400) {
            Write-Host "PASS: Received expected 400 Bad Request for password mismatch." -ForegroundColor Green
        } else {
            throw "FAIL: Unexpected status code: $($_.Exception.Response.StatusCode.value__)"
        }
    }

    # 6. Test Successful Registration (Expect 201)
    Write-Host "`n[6/12] Testing POST /api/auth/register with valid credentials (Expect 201)..." -ForegroundColor Yellow
    $validRegBody = @{
        name = "Ahmed Mohamed"
        email = "ahmed@example.com"
        password = "ExamplePassword123!"
        confirmPassword = "ExamplePassword123!"
    } | ConvertTo-Json
    $regResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/register" -Method Post -ContentType "application/json" -Body $validRegBody
    if ($regResponse.success -eq $true -and $regResponse.user.email -eq "ahmed@example.com") {
        Write-Host "PASS: Successfully registered user Ahmed Mohamed (ID: $($regResponse.user.id))." -ForegroundColor Green
        if ($regResponse.user.passwordHash -or $regResponse.user.salt) {
            throw "FAIL: Security violation! Password hash or salt exposed in response!"
        }
        Write-Host "PASS: Credentials properly withheld from response payload." -ForegroundColor Green
    } else {
        throw "FAIL: Registration failed: $($regResponse | ConvertTo-Json)"
    }

    # 7. Test Duplicate Registration Conflict (Expect 409)
    Write-Host "`n[7/12] Testing POST /api/auth/register with duplicate email (Expect 409)..." -ForegroundColor Yellow
    try {
        Invoke-RestMethod -Uri "$baseUrl/api/auth/register" -Method Post -ContentType "application/json" -Body $validRegBody
        throw "FAIL: Expected 409 for duplicate email but request succeeded."
    } catch {
        if ($_.Exception.Response.StatusCode.value__ -eq 409) {
            Write-Host "PASS: Received expected 409 Conflict for duplicate email." -ForegroundColor Green
        } else {
            throw "FAIL: Unexpected status code: $($_.Exception.Response.StatusCode.value__)"
        }
    }

    # 8. Test Login with Missing Fields (Expect 400)
    Write-Host "`n[8/12] Testing POST /api/auth/login with missing fields (Expect 400)..." -ForegroundColor Yellow
    try {
        Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -ContentType "application/json" -Body "{}"
        throw "FAIL: Expected 400 for empty login."
    } catch {
        if ($_.Exception.Response.StatusCode.value__ -eq 400) {
            Write-Host "PASS: Received expected 400 Bad Request for missing login fields." -ForegroundColor Green
        } else {
            throw "FAIL: Unexpected status code: $($_.Exception.Response.StatusCode.value__)"
        }
    }

    # 9. Test Login with Incorrect Credentials (Expect 401)
    Write-Host "`n[9/12] Testing POST /api/auth/login with wrong password (Expect 401)..." -ForegroundColor Yellow
    try {
        $wrongPwdBody = @{
            email = "ahmed@example.com"
            password = "WrongPassword999!"
        } | ConvertTo-Json
        Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -ContentType "application/json" -Body $wrongPwdBody
        throw "FAIL: Expected 401 for wrong password."
    } catch {
        if ($_.Exception.Response.StatusCode.value__ -eq 401) {
            Write-Host "PASS: Received expected 401 Unauthorized for incorrect password." -ForegroundColor Green
        } else {
            throw "FAIL: Unexpected status code: $($_.Exception.Response.StatusCode.value__)"
        }
    }

    # 10. Test Login with Non-Existent User (Expect 401)
    Write-Host "`n[10/12] Testing POST /api/auth/login with nonexistent email (Expect 401)..." -ForegroundColor Yellow
    try {
        $nonExistentBody = @{
            email = "nobody@example.com"
            password = "ExamplePassword123!"
        } | ConvertTo-Json
        Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -ContentType "application/json" -Body $nonExistentBody
        throw "FAIL: Expected 401 for non-existent user."
    } catch {
        if ($_.Exception.Response.StatusCode.value__ -eq 401) {
            Write-Host "PASS: Received expected 401 Unauthorized for non-existent email." -ForegroundColor Green
        } else {
            throw "FAIL: Unexpected status code: $($_.Exception.Response.StatusCode.value__)"
        }
    }

    # 11. Test Successful Login (Expect 200)
    Write-Host "`n[11/12] Testing POST /api/auth/login with valid credentials (Expect 200)..." -ForegroundColor Yellow
    $loginBody = @{
        email = "ahmed@example.com"
        password = "ExamplePassword123!"
    } | ConvertTo-Json
    $loginResp = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -ContentType "application/json" -Body $loginBody
    if ($loginResp.success -eq $true -and $loginResp.user.name -eq "Ahmed Mohamed") {
        Write-Host "PASS: Login succeeded for Ahmed Mohamed. Redirect URL: $($loginResp.redirectUrl)" -ForegroundColor Green
        if ($loginResp.user.passwordHash -or $loginResp.user.salt) {
            throw "FAIL: Security violation! Password hash exposed in login response!"
        }
        Write-Host "PASS: Password hashes strictly withheld from login response." -ForegroundColor Green
    } else {
        throw "FAIL: Login failed: $($loginResp | ConvertTo-Json)"
    }

    # 12. Test Static File Serving & Asset Integrity
    Write-Host "`n[12/12] Testing static frontend delivery..." -ForegroundColor Yellow
    $loginHtml = Invoke-WebRequest -Uri "$baseUrl/login.html" -Method Get -UseBasicParsing
    if ($loginHtml.StatusCode -eq 200 -and $loginHtml.Content -like "*Welcome back!*") {
        Write-Host "PASS: login.html served correctly." -ForegroundColor Green
    } else {
        throw "FAIL: Failed to serve login.html"
    }

    $walletSvg = Invoke-WebRequest -Uri "$baseUrl/assets/images/wallet-illustration.svg" -Method Get -UseBasicParsing
    if ($walletSvg.StatusCode -eq 200 -and $walletSvg.Headers["Content-Type"] -like "*image/svg+xml*") {
        Write-Host "PASS: wallet-illustration.svg served with image/svg+xml content-type." -ForegroundColor Green
    } else {
        throw "FAIL: Failed to serve wallet-illustration.svg"
    }

    Write-Host "`n=================================================" -ForegroundColor Cyan
    Write-Host "ALL 12 INTEGRATION TESTS PASSED SUCCESSFULLY!" -ForegroundColor Green
    Write-Host "=================================================" -ForegroundColor Cyan

} finally {
    if ($serverProcess -and !$serverProcess.HasExited) {
        Write-Host "`nCleaning up background test server..." -ForegroundColor DarkGray
        $serverProcess.Kill()
        $serverProcess.WaitForExit()
    }
    # Clean up test DB
    if (Test-Path "$PSScriptRoot\Backend\pennywise_test.db") {
        Remove-Item "$PSScriptRoot\Backend\pennywise_test.db" -Force -ErrorAction SilentlyContinue
    }
}
