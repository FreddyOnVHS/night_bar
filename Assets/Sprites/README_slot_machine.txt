SLOT MACHINE — NE WALL — UNITY IMPORT
====================================
File
  slot_machine_ne_sheet.png    352 x 165 px, cell 88 x 165, 4 frames
  slot_machine_ne_sheet-3x.png preview only, do not import

Frames — LIGHTS ONLY
  All four frames are the same machine. Reels, buttons, coin tray and the
  handle never move; only the lighting changes:
    frame 0  chase step 1
    frame 1  chase step 2
    frame 2  chase step 3
    frame 3  chase step 4
  The chase runs across the marquee bulb rows (top and bottom), the jackpot
  strip inside the marquee, and the credit LEDs above the button bank.

Detail level
  Drawn at 3x the internal resolution of the small prop set, matching the
  back bar, jukebox and restroom door: moulded gold marquee, three light
  reel drums with shaded top/bottom curves and real BAR / SEVEN / CHERRIES
  symbols, a win line, pay-table LEDs, four-button bank, recessed coin tray.

Geometry
  Front face shears DOWN-right (1 px per 2 px), so it stands against a wall
  receding up-right — a NE-running wall. The cabinet top and its east end
  each carry a real face receding up-right over a 6 px depth.
  Mirror on X for a NW wall.

Import settings
  Texture Type      Sprite (2D and UI)
  Sprite Mode       Multiple
  Pixels Per Unit   42   (3 x the 14 of the small prop set, so it sits at the
                     same world scale as the back bar and door)
  Mesh Type         Full Rect
  Filter Mode       Point (no filter)
  Compression       None
  Generate Mip Maps off
  -> Sprite Editor > Slice > Grid By Cell Size 88 x 165,
     Pivot Bottom Left > Slice > Apply

Clips
  slot_attract  frames 0, 1, 2, 3   Samples 8, loop — steady chase
  slot_win      frames 0, 2         Samples 12, loop 3-4 times — fast flash
  For a machine standing idle and unpowered, just hold frame 0 and disable
  the clip.
