# Script para probar el endpoint GetOrderById
# Uso: .\test-order-endpoint.ps1 -OrderId "guid-del-pedido" -Token "jwt-token"

param(
    [Parameter(Mandatory=$true)]
    [string]$OrderId,
    
    [Parameter(Mandatory=$true)]
    [string]$Token
)

$uri = "http://localhost:5000/api/orders/$OrderId"
$headers = @{
    "Authorization" = "Bearer $Token"
    "Content-Type" = "application/json"
    "Accept" = "application/json"
}

Write-Host "Testing endpoint: $uri" -ForegroundColor Cyan
Write-Host "Headers:" -ForegroundColor Yellow
$headers | Format-Table

try {
    $response = Invoke-RestMethod -Uri $uri -Method Get -Headers $headers -ErrorAction Stop
    
    Write-Host "`n✅ SUCCESS - Response received:" -ForegroundColor Green
    Write-Host "Status Code: 200" -ForegroundColor Green
    Write-Host "Response Type: $($response.GetType().Name)" -ForegroundColor Green
    Write-Host "`nResponse Body:" -ForegroundColor Cyan
    $response | ConvertTo-Json -Depth 10
    
    if ($null -eq $response) {
        Write-Host "`n⚠️ WARNING: Response body is NULL!" -ForegroundColor Red
    } else {
        Write-Host "`n✅ Response body is not null" -ForegroundColor Green
        if ($response.id) {
            Write-Host "Order ID: $($response.id)" -ForegroundColor Green
        }
        if ($response.orderNumber) {
            Write-Host "Order Number: $($response.orderNumber)" -ForegroundColor Green
        }
    }
}
catch {
    Write-Host "`n❌ ERROR:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode.value__
        Write-Host "Status Code: $statusCode" -ForegroundColor Red
        
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response Body: $responseBody" -ForegroundColor Red
    }
}

