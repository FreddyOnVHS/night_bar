BACK BAR — UNITY IMPORT
======================
File
  back_bar_nw_sheet.png    644 x 270 px, cell 322 x 270, 2 frames
    frame 0  under-shelf glow lit
    frame 1  glow off
  back_bar_nw_sheet-2x.png preview only, do not import

What it is
  A full back bar: panelled lower cabinet with a wood counter top, a mirrored
  gantry split into six bays by gold pilasters, two gold bottle rails carrying
  rows of bottles, and a moulded cornice across the top.

Rebuilt geometry
  Reconstructed in iso world space rather than as a sheared flat face — that
  older approach is what produced the corner notch and the hatched top planes.
    u = along the bar   v = depth into the wall   z = height
    screen x = (v - u) * 2S      screen y = -(u + v) * S - z * S
  2 px across per 1 px up: the same 2:1 slope as the booth, slot machine and
  door. Every element is a closed box showing its top face plus the two faces
  turned toward the camera, and boxes are painted far to near, so the counter
  and cabinet correctly overlap the shelves behind them and the SW end reads
  as solid wood instead of a hollow black cap.

Detail level
  3x the internal resolution of the small prop set, matching the jukebox,
  restroom door, slot machine and booth.

Import settings
  Texture Type      Sprite (2D and UI)
  Sprite Mode       Multiple
  Pixels Per Unit   42   (3 x the 14 of the small prop set, so it sits at the
                     same world scale as the other 3x props)
  Mesh Type         Full Rect
  Filter Mode       Point (no filter)
  Compression       None
  Generate Mip Maps off
  -> Sprite Editor > Slice > Grid By Cell Size 322 x 270,
     Pivot Bottom Centre > Slice > Apply

Clips
  Two-frame flicker on the under-shelf glow, Samples 4 — or hold frame 0 while
  the bar is open and frame 1 after close. Mirror on X for the opposite wall.
