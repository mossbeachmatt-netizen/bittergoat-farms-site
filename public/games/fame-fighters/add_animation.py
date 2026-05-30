#!/usr/bin/env python3
"""
Fame Fighters — Add Animation Script
=====================================
1. Put your frame PNGs in the same folder as this script
2. Edit the CONFIG section below
3. Run: python3 add_animation.py
4. It will update assets.js automatically
5. Then start a new Claude chat with game.html to wire it into the game

Requirements:
    pip install Pillow scipy numpy
"""

import os, sys, base64, re
import numpy as np
from PIL import Image
from collections import deque
from scipy import ndimage

# ═══════════════════════════════════════════════
#  CONFIG — edit this for each new animation
# ═══════════════════════════════════════════════

FRAMES = [
    'don-kick-1.png',
    'don-kick-2.png',
    'don-kick-3.png',
    'don-kick-4.png',
    'don-kick-5.png',
    'don-kick-6.png',
]

# Variable name that will appear in assets.js
# Examples: P3_PUNCH_B64, P1_HURT_B64, P2_JUMP_B64
ASSET_NAME = 'DON_KICK_B64'

# Output sprite sheet filename
OUTPUT_PNG = 'don_kick.png'

# Path to your assets.js file
ASSETS_JS_PATH = 'assets.js'

# ═══════════════════════════════════════════════
#  PROCESSING — no need to edit below this line
# ═══════════════════════════════════════════════

def remove_background(img_path):
    """Remove checkerboard or black background from a frame."""
    img = Image.open(img_path).convert('RGBA')
    arr = np.array(img, dtype=np.int16)
    h, w = arr.shape[:2]

    # Detect background type from corners
    corners = [arr[0,0,:3], arr[0,-1,:3], arr[-1,0,:3], arr[-1,-1,:3]]
    avg = np.mean(corners, axis=0)
    is_black_bg = all(v < 15 for v in avg)

    if is_black_bg:
        # Black background removal
        def is_bg(r, g, b):
            return int(r) < 5 and int(g) < 5 and int(b) < 5
    else:
        # Checkerboard removal (cold gray)
        def is_bg(r, g, b):
            r,g,b = int(r),int(g),int(b)
            if abs(r-g) > 8 or abs(g-b) > 8 or abs(r-b) > 8:
                return False
            return r > 120

    # Flood fill from edges
    visited = np.zeros((h,w), bool)
    mask    = np.zeros((h,w), bool)
    q = deque()
    for x in range(w):
        for y in [0, h-1]:
            if not visited[y,x]:
                visited[y,x] = True
                r,g,b = arr[y,x,:3]
                if is_bg(r,g,b): mask[y,x]=True; q.append((y,x))
    for y in range(h):
        for x in [0, w-1]:
            if not visited[y,x]:
                visited[y,x] = True
                r,g,b = arr[y,x,:3]
                if is_bg(r,g,b): mask[y,x]=True; q.append((y,x))
    while q:
        y,x = q.popleft()
        for dy,dx in [(-1,0),(1,0),(0,-1),(0,1)]:
            ny,nx = y+dy, x+dx
            if 0<=ny<h and 0<=nx<w and not visited[ny,nx]:
                visited[ny,nx] = True
                r,g,b = arr[ny,nx,:3]
                if is_bg(r,g,b): mask[ny,nx]=True; q.append((ny,nx))

    result = np.array(img)
    result[mask, 3] = 0

    # Remove enclosed background patches (trapped checker squares)
    gray = (
        (result[:,:,3] > 0) &
        (np.abs(result[:,:,0].astype(int) - result[:,:,1].astype(int)) < 15) &
        (np.abs(result[:,:,1].astype(int) - result[:,:,2].astype(int)) < 15) &
        (result[:,:,0] > 130)
    )
    labeled, nf = ndimage.label(gray)
    border = set(
        labeled[0,:].tolist() + labeled[-1,:].tolist() +
        labeled[:,0].tolist() + labeled[:,-1].tolist()
    )
    border.discard(0)
    for lbl in range(1, nf+1):
        if lbl not in border:
            result[labeled==lbl, 3] = 0

    return Image.fromarray(result)


def get_bbox(img, min_density=4):
    arr = np.array(img)
    vis = (arr[:,:,3] > 10)
    rc  = vis.sum(axis=1)
    cc  = vis.sum(axis=0)
    rows = np.where(rc >= min_density)[0]
    cols = np.where(cc >= min_density)[0]
    if len(rows)==0 or len(cols)==0:
        return (0, img.height, 0, img.width)
    return (rows[0], rows[-1], cols[0], cols[-1])


def build_sheet(frame_paths):
    frames  = []
    bboxes  = []
    missing = []

    for p in frame_paths:
        if not os.path.exists(p):
            print(f"  WARNING: {p} not found — skipping")
            missing.append(p)
            continue
        print(f"  Processing {p}...")
        img = remove_background(p)
        bb  = get_bbox(img)
        frames.append(img)
        bboxes.append(bb)

    if not frames:
        print("ERROR: No frames found!")
        sys.exit(1)

    # Global bounding box
    gt = min(b[0] for b in bboxes)
    gb = max(b[1] for b in bboxes)
    gl = min(b[2] for b in bboxes)
    gr = max(b[3] for b in bboxes)
    PAD = 12
    FW  = gr - gl + PAD*2
    FH  = gb - gt + PAD*2
    N   = len(frames)

    sheet = Image.new('RGBA', (FW*N, FH), (0,0,0,0))
    for i,(img,bb) in enumerate(zip(frames,bboxes)):
        cropped = img.crop((bb[2], bb[0], bb[3]+1, bb[1]+1))
        cw, ch  = cropped.size
        frame   = Image.new('RGBA', (FW,FH), (0,0,0,0))
        px = PAD + (gr-gl-cw)//2
        py = FH - ch - PAD
        frame.paste(cropped, (px, py), cropped)
        sheet.paste(frame, (i*FW, 0), frame)

    DW = round(360 * (FW/FH))
    return sheet, FW, FH, N, DW, 360


def update_assets_js(asset_name, b64_data, assets_path):
    line = f'const {asset_name} = "data:image/png;base64,{b64_data}";'

    if not os.path.exists(assets_path):
        print(f"  Creating new {assets_path}")
        with open(assets_path, 'w') as f:
            f.write('// Fame Fighters — Asset Bundle\n')
            f.write(line + '\n')
        return

    with open(assets_path) as f:
        content = f.read()

    if asset_name in content:
        # Update existing entry
        content = re.sub(
            rf'const {asset_name} = "data:image/png;base64,[^"]+";',
            line,
            content
        )
        print(f"  Updated existing {asset_name} in {assets_path}")
    else:
        # Append new entry
        content += '\n' + line + '\n'
        print(f"  Appended {asset_name} to {assets_path}")

    with open(assets_path, 'w') as f:
        f.write(content)


# ═══════════════════════════════════════════════
#  MAIN
# ═══════════════════════════════════════════════
if __name__ == '__main__':
    print(f"\n🎮 Fame Fighters — Processing {len(FRAMES)} frames → {ASSET_NAME}\n")

    # Build sprite sheet
    sheet, FW, FH, N, DW, DH = build_sheet(FRAMES)

    # Save PNG
    sheet.save(OUTPUT_PNG)
    print(f"\n✓ Sprite sheet saved: {OUTPUT_PNG}")
    print(f"  Frame size: {FW}x{FH}  |  Frames: {N}  |  Draw size: {DW}x{DH}")

    # Encode to base64
    with open(OUTPUT_PNG, 'rb') as f:
        b64 = base64.b64encode(f.read()).decode()
    print(f"  Base64 size: {len(b64)//1024}KB")

    # Update assets.js
    update_assets_js(ASSET_NAME, b64, ASSETS_JS_PATH)
    print(f"\n✓ {ASSETS_JS_PATH} updated")

    # Print wiring instructions for Claude
    print(f"""
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ DONE! Now open a new Claude chat and say:

"Wire {ASSET_NAME} into the game as [CHARACTER]'s 
[ANIMATION] animation — {N} frames, {FW}x{FH} per frame,
draw size {DW}x{DH}. Triggered by [KEY]."

Upload game.html to that chat.
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
""")
