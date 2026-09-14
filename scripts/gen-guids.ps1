$g = @()
for ($i = 0; $i -lt 5; $i++) { $g += [guid]::NewGuid().ToString().ToUpper() }
Write-Host FOLDER=$($g[0])
Write-Host DOMAIN=$($g[1])
Write-Host APPLICATION=$($g[2])
Write-Host INFRASTRUCTURE=$($g[3])
Write-Host API=$($g[4])
