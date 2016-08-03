Import-Module .\tools\scripts\Build-netmf.psm1
if( Test-Path -PathType Container (Join-Path $SPOROOT "bin\automation") )
{
    Write-Host "Existing tools installation found"
    return
}

Invoke-WebRequest -UseBasicParsing -Uri "http://netmf.github.io/downloads/build-tools.zip" | Expand-Stream -Destination $SPOROOT

