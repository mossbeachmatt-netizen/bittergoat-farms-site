$HtmlPath = 'C:\Users\mossb\Downloads\dragon_palace_fishing (11).html'
$ImgPath = 'C:\Users\mossb\Downloads\Gun_turret_for_game_202605292210.jpeg'
$html = Get-Content -Path $HtmlPath -Raw
$imgBytes = [System.IO.File]::ReadAllBytes($ImgPath)
$src = 'data:image/jpeg;base64,' + [Convert]::ToBase64String($imgBytes)
$prefix = '<img id="turretImg" src="'
$start = $html.IndexOf($prefix)
if ($start -lt 0) { throw 'turretImg prefix not found' }
$start += $prefix.Length
$end = $html.IndexOf('"', $start)
if ($end -lt 0) { throw 'closing quote not found' }
$newHtml = $html.Substring(0, $start) + $src + $html.Substring($end)
Set-Content -Path $HtmlPath -Value $newHtml -Encoding UTF8
Write-Host 'replaced'
