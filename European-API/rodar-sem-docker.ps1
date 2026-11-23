# Sobe o MongoDB local e a API do European, sem Docker.
#
# Uso (na pasta European-API):
#   .\rodar-sem-docker.ps1                 # sobe MongoDB + API com os dados que já existem
#   .\rodar-sem-docker.ps1 -Seed           # antes, apaga o banco e grava os dados de demonstração (DADOS.txt)
#   .\rodar-sem-docker.ps1 -Mongod "C:\caminho\mongod.exe" -Dados "C:\caminho\dados"
#
# Se o MongoDB já estiver rodando na porta 27017 (ex.: instalado como serviço), ele é reaproveitado.
# Para parar: feche a janela do MongoDB e aperte Ctrl+C aqui.
param(
    [switch]$Seed,
    [string]$Mongod,
    [string]$Dados = (Join-Path $PSScriptRoot 'dados-mongo')
)

function Test-Porta([int]$porta) {
    try { $c = New-Object Net.Sockets.TcpClient; $c.Connect('127.0.0.1', $porta); $c.Close(); return $true } catch { return $false }
}

if (Test-Porta 27017) {
    Write-Host 'MongoDB já está rodando na porta 27017.'
} else {
    if (-not $Mongod) {
        $Mongod = (Get-Command mongod -ErrorAction SilentlyContinue).Source
        if (-not $Mongod) {
            $Mongod = Get-ChildItem 'C:\Program Files\MongoDB\Server\*\bin\mongod.exe' -ErrorAction SilentlyContinue |
                Sort-Object FullName -Descending | Select-Object -First 1 -ExpandProperty FullName
        }
    }
    if (-not $Mongod -or -not (Test-Path $Mongod)) {
        Write-Host 'mongod.exe não encontrado. Instale o MongoDB, informe -Mongod "caminho\mongod.exe" ou use: docker compose up -d'
        exit 1
    }

    New-Item -ItemType Directory -Force $Dados | Out-Null
    Start-Process $Mongod -ArgumentList '--dbpath', "`"$Dados`"", '--port', '27017', '--bind_ip', '127.0.0.1'
    for ($i = 0; $i -lt 30 -and -not (Test-Porta 27017); $i++) { Start-Sleep -Seconds 1 }
}

if ($Seed) {
    # Apaga o banco "european" e grava os dados do DADOS.txt (todas as senhas: Senha123)
    $env:EUROPEAN_MONGO = 'mongodb://127.0.0.1:27017/european'
    dotnet run --project (Join-Path $PSScriptRoot '..\Seed')
}

Set-Location $PSScriptRoot
if (-not (Test-Path node_modules)) { npm install }

$env:MONGO_URL = 'mongodb://127.0.0.1:27017'
$env:DB_NAME = 'european'
node src/server.js
