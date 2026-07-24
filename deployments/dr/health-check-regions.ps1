[CmdletBinding()]
param (
    [string]$PrimaryUrl = "http://localhost:5000",
    [string]$SecondaryUrl = "http://localhost:5007"
)

Write-Host "🔍 [Multi-Region Cluster Health Inspector]" -ForegroundColor Cyan
Write-Host "--------------------------------------------------------"

# 1. Primary Region Health
Write-Host "Checking Primary Region Endpoint ($PrimaryUrl)..." -NoNewline
try {
    $res = Invoke-RestMethod -Uri "$PrimaryUrl/health/live" -TimeoutSec 3 -ErrorAction Stop
    Write-Host " [HEALTHY 🟢]" -ForegroundColor Green
} catch {
    Write-Host " [UNHEALTHY 🔴]" -ForegroundColor Red
}

# 2. Secondary Region Health
Write-Host "Checking Secondary Region Endpoint ($SecondaryUrl)..." -NoNewline
try {
    $res = Invoke-RestMethod -Uri "$SecondaryUrl/health/live" -TimeoutSec 3 -ErrorAction Stop
    Write-Host " [HEALTHY 🟢]" -ForegroundColor Green
} catch {
    Write-Host " [UNHEALTHY 🔴]" -ForegroundColor Red
}

Write-Host "--------------------------------------------------------"
