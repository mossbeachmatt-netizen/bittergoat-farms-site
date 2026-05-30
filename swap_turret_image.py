from pathlib import Path
import re, base64
html_path = Path(r'C:/Users/mossb/Downloads/dragon_palace_fishing (11).html')
img_path = Path(r'C:/Users/mossb/Downloads/Gun_turret_for_game_202605292210.jpeg')
html = html_path.read_text(encoding='utf-8')
img_data = base64.b64encode(img_path.read_bytes()).decode('ascii')
new_src = 'data:image/jpeg;base64,' + img_data
pattern = re.compile(r'(<img id="turretImg" src=")[^"]+("\s*/?>)')
new_html, count = pattern.subn(lambda m: m.group(1) + new_src + m.group(2), html, count=1)
if count != 1:
    raise RuntimeError(f'Expected to replace 1 turretImg src, but replaced {count}')
html_path.write_text(new_html, encoding='utf-8')
print('replaced', count)
