PLAYER SPRITE (ISOMETRIC, 8-WAY) — UNITY IMPORT
==============================================
Files
  player_iso_sheet.png     56 x 176 px, cell 14 x 22
  player_iso_sheet-4x.png  preview only, do not import

Grid
  Columns (left to right): idle, step A, idle, step B
  Rows (top to bottom):    SE, SW, NE, NW, S, N, E, W
  Pivot: bottom centre. Every frame is centred on x = 6.5, so the sprite
  does not shift when the facing changes.

Facings
  SE SW NE NW  the four tile-edge diagonals (the usual iso movement axes)
  S  N         straight down / up the screen: front and back views
  E  W         straight across the screen: side profiles
  SW, NW and W are exact mirrors of SE, NE and E — you can drop those three
  rows and set scale.x = -1 instead (5 rows instead of 8).

Picking a facing from input
  Take the movement vector in world space, get its angle, and snap to the
  nearest of eight 45 degree sectors:
    idx = round(atan2(dir.y, dir.x) / (PI / 4)) mod 8
    order from +X counter-clockwise: E, NE, N, NW, W, SW, S, SE
  If movement is grid-locked to tiles you only ever need the four diagonals.

Import settings
  Texture Type      Sprite (2D and UI)
  Sprite Mode       Multiple
  Pixels Per Unit   14        matches every other sheet in this folder
  Mesh Type         Full Rect
  Filter Mode       Point (no filter)
  Compression       None
  Generate Mip Maps off
  -> Sprite Editor > Slice > Grid By Cell Size 14 x 22,
     Pivot Bottom Center > Slice > Apply

Clips (Samples 8, loop)
  walk_se row 1   walk_sw row 2   walk_ne row 3   walk_nw row 4
  walk_s  row 5   walk_n  row 6   walk_e  row 7   walk_w  row 8
  Idle pose for a facing = frame 0 of its row.

Sorting
  Set Project Settings > Graphics > Transparency Sort Axis to (0, 1, 0), or
  drive sortingOrder from world Y, so the player occludes props correctly.

The older top-down sheet (player_sheet.png) is left in place; delete it if
the project is iso only.
