PLAYER SPRITE - ISOMETRIC 8-WAY WALK
====================================

CELL 42 x 66 PX   |   8 FACINGS   |   4 FRAMES EACH   |   32 CELLS TOTAL

FILES

  player_walk_sheet.png       168 x 528 px. 4 columns (frames) x 8 rows (facings).
                              Import this one.
  player_walk_sheet-3x.png    504 x 1584 px. Nearest-neighbour 3x preview only.

ROW ORDER (top to bottom)

  1  SE   toward camera, down-right
  2  SW   toward camera, down-left   (mirror of SE)
  3  NE   away from camera, up-right
  4  NW   away from camera, up-left  (mirror of NE)
  5  S    screen down, front view
  6  N    screen up, back view
  7  E    screen right, profile
  8  W    screen left, profile       (mirror of E)

COLUMN ORDER (left to right)

  1  CONTACT L   legs apart, left leg leading
  2  PASS        legs together, body lifts 1 px
  3  CONTACT R   legs apart, right leg leading
  4  PASS        legs together, body lifts 1 px

  Loop 1-2-3-4 at 8 fps for a walk, 5-6 fps for a slow walk. For an idle, hold
  frame 2 or 4 of the facing (legs together) rather than frame 1 or 3.

IMPORT SETTINGS (UNITY)

  Texture Type      Sprite (2D and UI)
  Sprite Mode       Multiple -> Grid by Cell Size, 42 x 66, no padding, no offset
  Pivot             Bottom Centre
  Filter Mode       Point (no filter)
  Compression       None
  Generate Mip Maps off
  Wrap Mode         Clamp
  Pixels Per Unit   match the bar props

  Slice left to right, top to bottom. Every cell is 42 x 66 with the feet on the
  bottom row, so all frames and all facings share one pivot and the player never
  jitters between frames or when the facing changes.

PROJECTION

  2:1 isometric, the same axes as the bar map and props. Light comes from the
  upper right; do not add a second light source in engine.
  Diagonal facings put the camera-near arm and leg on the near side of the body,
  so the upper body and the stride read as pointing the same way.
  Facings that walk AWAY from camera (NE / NW) invert the depth cues: the leading
  foot is the further one, raised slightly and shaded down.

PALETTE

  shirt #5f9bf2   hair #3a2417   skin #f0c096
  jeans #5a6390   boots #3a2b34   outline #17101a

  Shirt and hair are the two intended recolour slots for player customisation;
  everything else is shared with the bartender so the two read as one cast.
