$baseUrl = "http://localhost:5180"
try {
    $r = Invoke-WebRequest -Uri "$baseUrl/api/v1/factories" -UseBasicParsing -TimeoutSec 5
    Write-Host "Backend is responding. Status:" $r.StatusCode
} catch {
    Write-Host "Backend NOT responding. Error:" $_.Exception.Message
}
