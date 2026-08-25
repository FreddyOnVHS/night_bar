JUKEBOX — NE WALL — UNITY IMPORT
==============================
File
  jukebox_ne_sheet.png    1260 x 574 px, cell 315 x 574, 4 frames
    frame 0  unlit / attract off
    frame 1  warm amber glow
    frame 2  red-hot glow
    frame 3  rainbow cycle

Small version (matches the drawn prop set)
  jukebox_ne_small_sheet.png    112 x 51 px, cell 28 x 51, same 4 frames
  jukebox_ne_small_sheet-6x.png preview only, do not import
  Scaled to sit alongside the hand-drawn props (dart cabinet 26 x 46, slot
  machine 24 x 40, claw 20 x 32), so import it at Pixels Per Unit 14 like the
  rest of that set. Alpha is snapped to fully on/off so the silhouette stays
  crisp; use Point filtering. The full-size sheet above is the one to use if
  you would rather run the whole game at the higher art resolution.

Orientation
  Mirrored on X from the supplied art so the cabinet's depth recedes up-RIGHT
  and its back sits flush against a wall running NE. The front panel (arch,
  selection buttons, grille) faces down-left toward the room.
  For a NW wall, flip scale.x = -1 to get the original orientation back.

Alignment
  All four frames are padded to a common cell and bottom-centre aligned, so
  the cabinet's floor contact point is identical in every frame and the
  sprite does not shift while the lights cycle.

Import settings
  Texture Type      Sprite (2D and UI)
  Sprite Mode       Multiple
  Pixels Per Unit   set so the cabinet reads at the right height beside your
                     other props — this art is higher-res than the drawn
                     pixel props, so it needs a larger PPU to scale down
  Mesh Type         Full Rect
  Filter Mode       Point (no filter) — this is true pixel art, keep it crisp
  Compression       None
  Generate Mip Maps off
  -> Sprite Editor > Slice > Grid By Cell Size 315 x 574,
     Pivot Bottom Center > Slice > Apply

Clips
  juke_idle    frame 0                     single frame, machine off
  juke_attract frames 1, 2                 Samples 4, loop — pulsing glow
  juke_playing frames 1, 2, 3, 2           Samples 6, loop — colour cycle
  A slower Samples value on juke_playing reads as a lazy rainbow sweep.

Note
  The neon tubes carry a soft glow halo in the source art. Against a dark bar
  interior it reads well as-is; if you add a Light2D or additive glow sprite,
  keep it subtle so it does not double up with the baked halo.
