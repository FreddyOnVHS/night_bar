TOILET SPRITE — UNITY IMPORT
==========================
File
  toilet_sheet.png    1188 x 886 px, cell 594 x 886, 2 frames
    frame 0  lid closed
    frame 1  lid open, bowl visible

One sheet, two equal cells. The source renders arrived as separate images at
slightly different sizes, so each frame is padded to a common cell and
bottom-centre aligned — the toilet's base sits at the same spot in both
frames, so the sprite does not jump when the lid is toggled.

Import settings
  Texture Type      Sprite (2D and UI)
  Sprite Mode       Multiple
  Pixels Per Unit   set so the toilet reads at the right height next to your
                     other props — this art is higher-res than the rest of
                     the bar set, so a larger PPU is needed to scale it down
  Mesh Type         Full Rect
  Filter Mode       Bilinear (smooth-shaded render, not 1:1 pixel art —
                     Point filtering will look chunky on it)
  Compression       None or High Quality
  Generate Mip Maps off
  -> Sprite Editor > Slice > Grid By Cell Size 594 x 886,
     Pivot Bottom Center > Slice > Apply

Use
  Swap frame 0 / frame 1 on interaction. There is no in-between frame, so
  drive it as a two-state sprite rather than an animation clip.
