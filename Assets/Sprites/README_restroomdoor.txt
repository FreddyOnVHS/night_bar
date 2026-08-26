RESTROOM DOOR — NE WALL — UNITY IMPORT
=====================================
File
  restroom_door_ne_sheet.png    128 x 111 px, cell 64 x 111, 2 frames
    frame 0  CLOSED   panelled leaf, sign plate, brass lever
    frame 1  OPEN     leaf swung inward, dark room beyond
  restroom_door_ne_sheet-3x.png preview only, do not import

Detail level
  Redrawn at 3x the internal resolution of the small prop set so its pixel
  density matches the back bar and the jukebox: moulded architrave with a lit
  inner reveal, three sunken panels with rails and stiles, sign plate and a
  brass lever handle.

Geometry
  Front face shears DOWN-right (1 px per 2 px), so it sits in a wall receding
  up-right — a NE-running wall. The head casing carries a real top face and
  the east end a side face, both receding up-right for a 6 px wall thickness,
  so the frame reads as solid joinery rather than a flat decal.
  Mirror on X for a NW wall.

Import settings
  Texture Type      Sprite (2D and UI)
  Sprite Mode       Multiple
  Pixels Per Unit   42   (3 x the 14 used by the small prop set, so it sits at
                     the same world scale despite the finer pixels — same as
                     the back bar)
  Mesh Type         Full Rect
  Filter Mode       Point (no filter)
  Compression       None
  Generate Mip Maps off
  -> Sprite Editor > Slice > Grid By Cell Size 64 x 111,
     Pivot Bottom Left > Slice > Apply

Use
  Two-state swap on interaction, same as before — no in-between frame. If you
  want a hint of motion, hold frame 1 for 2 samples after a 1-sample cross.
