$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot
$utf8SinBom = New-Object System.Text.UTF8Encoding $false

Write-Host "=== Preparando entorno TF15 para presentacion ===" -ForegroundColor Cyan

# Detectar automaticamente la instancia de SQL Server disponible en esta maquina
$instancias = @("localhost", ".\SQLEXPRESS", "(localdb)\mssqllocaldb")
$instanciaActiva = $null

foreach ($srv in $instancias) {
    if ($srv -like "*localdb*") {
        sqllocaldb start mssqllocaldb 2>$null | Out-Null
    }
    try {
        $conn = New-Object System.Data.SqlClient.SqlConnection("Server=$srv;Database=master;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=3;")
        $conn.Open()
        $conn.Close()
        $instanciaActiva = $srv
        break
    } catch { }
}

if (-not $instanciaActiva) {
    Write-Warning "No se detecto SQL Server activo. Verifica que el servicio SQL Server este iniciado."
    exit 1
}

Write-Host "SQL Server detectado en: $instanciaActiva" -ForegroundColor Green

# Ajustar appsettings.Development.json con la instancia detectada
$devJsonPath = Join-Path $PSScriptRoot "src\SoporteTecnico.Api\appsettings.Development.json"
$jsonText = [System.IO.File]::ReadAllText($devJsonPath)
$jsonText = $jsonText -replace 'Server=[^;]+;', "Server=$instanciaActiva;"
[System.IO.File]::WriteAllText($devJsonPath, $jsonText, $utf8SinBom)

# Confiar en certificado HTTPS de desarrollo de .NET
dotnet dev-certs https --trust 2>$null | Out-Null

# Compilar solucion
Write-Host "Compilando solucion..." -ForegroundColor Cyan
dotnet build --nologo -v minimal

# Iniciar API y Frontend en ventanas separadas
Write-Host "Iniciando API (https://localhost:7022) y Frontend (https://localhost:63369)..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot'; dotnet run --project .\src\SoporteTecnico.Api\SoporteTecnico.Api.csproj --launch-profile https"
Start-Sleep -Seconds 4
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot'; dotnet run --project .\src\SoporteTecnico.Web\SoporteTecnico.Web.csproj --launch-profile https"

Start-Sleep -Seconds 5
Start-Process "https://localhost:63369"