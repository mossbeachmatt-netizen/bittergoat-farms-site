$htmlPath = 'C:\Users\mossb\Downloads\dragon_palace_fishing (11).html'
$imgPath = 'C:\Users\mossb\Downloads\Gun_turret_for_game_202605292210.jpeg'
$html = Get-Content -Path $htmlPath -Raw
$imgBytes = [System.IO.File]::ReadAllBytes($imgPath)
$src = 'data:image/jpeg;base64,' + [Convert]::ToBase64String($imgBytes)
$pattern = '(<img id="turretImg" src=")[^"]+("\s*/?>)'
$regex = [regex]$pattern
$newHtml, $count = $regex::Replace($html, { param($m) $m.Groups[1].Value + $src + $m.Groups[2].Value }, 1)
if ($count -ne 1) { throw "Expected to replace 1 turretImg src, but replaced $count" }
Set-Content -Path $htmlPath -Value $newHtml -Encoding UTF8
Write-Host 'replaced' $count
