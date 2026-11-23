$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repoRoot
try {
    docker compose up -d --wait db
    if ($LASTEXITCODE -ne 0) { throw 'PostgreSQL nie jest gotowy.' }
    dotnet run --project RentalCompany/RentalCompany --launch-profile http
    if ($LASTEXITCODE -ne 0) { throw 'Aplikacja zakonczyla prace z bledem.' }
}
finally {
    Pop-Location
}
