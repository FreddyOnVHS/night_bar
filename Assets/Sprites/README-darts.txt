DART MACHINE SPRITES — UNITY IMPORT
==================================
Files

  dart_cabinet_ne_sheet.png   44 x 48 px   cell 22 x 48, 2 frames
                              isometric wall unit for a NE-running wall
                              frame 0 = marquee + score lit
                              frame 1 = dark
  *-4x.png                    preview only, do not import

Import settings (same for both sheets)
  Texture Type      Sprite (2D and UI)
  Sprite Mode       Multiple
  Pixels Per Unit   14        (match the player sheet so scale is consistent)
  Mesh Type         Full Rect
  Filter Mode       Point (no filter)
  Compression       None
  Generate Mip Maps off
  -> Apply, then Sprite Editor > Slice > Grid By Cell Size
       cabinet:  26 x 46, Pivot Bottom Center
       board:    21 x 21, Pivot Center
     Slice > Apply

NE wall unit
  The face is sheared onto the iso plane (drops 1 px for every 2 px right),
  so it sits flush against a wall that recedes up and to the right, with a
  4 px end cap showing the cabinet depth. Pivot: bottom LEFT of the cell —
  the lowest point of the sprite is its near-left corner where it meets the
  floor. For a wall running the other way (up-left), mirror the sprite on X.
  Slice: Grid By Cell Size 22 x 48.

Notes
  Cabinet is 46 px tall against a 22 px player, so it stands a little over
  twice player height — matches the freestanding machine proportion.
  Wall board is the same face without the case, for hanging on a wall.
  Blink loop: 2-frame clip, Samples 4, hold each frame 2 samples
  (or drive frame 1 only while the machine is idle / attracting).
  Marquee and lower door panels are intentionally blank — drop your own
  game logo in there.
