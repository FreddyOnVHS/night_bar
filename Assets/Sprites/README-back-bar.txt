BACK BAR — NW WALL — UNITY IMPORT
================================
File
  back_bar_nw_sheet.png    cell 322 x 270, 2 frames (frame 0 = glow strip lit,
                           frame 1 = dim)
  back_bar_nw_sheet-2x.png preview only, do not import

Detail level
  Drawn at 3x the internal resolution of the other bar props so its pixel
  density matches the high-res toilet art: shaped bottles with necks, caps
  and paper labels, moulded gold cornice with dentil teeth, bevelled
  six-bay mirror with sheen streaks, wood grain, panelled lower cabinet.

Geometry
  Sheared onto the isometric plane rising 1 px per 2 px to the right, so it
  sits flush against a wall receding up-LEFT (a NW-running wall), with a
  12 px depth cap on the near-left end. Mirror on X for a NE wall.
  Solid volume, not a flat panel: the cornice, both bottle shelves and the
  counter over the cabinet each carry their own horizontal top face receding
  up-left, lit on the near edge and shaded at the far edge.
  The SW end is the box's west face in wood browns, receding up-left on the
  same 2:1 axis as the top planes for the full depth, with outlined near and
  bottom edges — so the cornice's top plane is backed by solid geometry and
  the top-left corner closes cleanly.
  Bottles are shaded as cylinders — lit left edge, specular highlight, darker
  right side, elliptical cap top — and each shelf has a thick front plank with
  an underside shadow.

Import settings
  Texture Type      Sprite (2D and UI)
  Sprite Mode       Multiple
  Pixels Per Unit   42   (3 x the 14 used by the rest of the set, so it sits
                     at the same world scale despite the finer pixels)
  Mesh Type         Full Rect
  Filter Mode       Point (no filter)
  Compression       None
  Generate Mip Maps off
  -> Sprite Editor > Slice > Grid By Cell Size 322 x 270,
     Pivot Bottom Right > Slice > Apply

Clips
  Two-frame flicker for the under-shelf glow, Samples 4, or hold frame 0
  while the bar is open and frame 1 after close.
