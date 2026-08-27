BARTENDER SPRITE - ISOMETRIC
============================

CELL 42 x 66 PX   |   3 STATES   |   4 FRAMES EACH

Built on the player figure: same cell, same pivot, same iso axes, same light, so
he can share the player's animator setup and sort against the same props.

CHARACTER

  Bald with a shiny scalp and stubble at the sides, short goatee around the mouth
  and chin, heavy brow, ruddy nose, gut over the belt, grimy shirt with a
  sweat-dark collar and armpits, stained apron, rolled sleeves.

FILES

  bartender_walk_sheet.png    168 x 132 px. 4 columns x 2 rows.
                              Row 1 = NE, row 2 = SW.
  bartender_idle_sheet.png    168 x 132 px. 4 columns x 2 rows.
                              Row 1 = SE, row 2 = SW.
  bartender_pour_sheet.png    168 x 66 px. 4 columns x 1 row. SE only.
  *-3x.png                    Nearest-neighbour 3x previews, reference only.

STATES

  WALK   NE and SW only - he patrols the length of the bar and nothing else.
         Columns: CONTACT L, PASS, CONTACT R, PASS. Loop at 8 fps.

  IDLE   SE and SW. A slow two-frame breath (the body lifts 1 px on frames 2-3).
         Loop at 3 fps. Play this while he is stopped between walks.

  POUR   SE only, facing the customer over the bar.
         Columns: LIFT, TILT, POUR, POUR.
         Run 1-2 once at 6 fps, then hold frames 3-4 alternating for as long as
         the pour lasts, then play 2-1 in reverse to bring the bottle back down.
         The bottle and the amber stream are drawn into the frames; the glass on
         the bar is not - place that as a separate prop under his hand.
         Body, legs and resting arm are identical to the SE idle pose, so IDLE SE
         and POUR SE cut together with no popping.

SUGGESTED STATE MACHINE

  IDLE (SE) -> WALK (NE) -> IDLE (SE or SW) -> WALK (SW) -> POUR (SE) -> IDLE

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
  bottom row, so all frames and all facings share one pivot and the bartender never
  jitters between frames or when the facing changes.

PROJECTION

  2:1 isometric, the same axes as the bar map and props. Light comes from the
  upper right; do not add a second light source in engine.
  Diagonal facings put the camera-near arm and leg on the near side of the body,
  so the upper body and the stride read as pointing the same way.
  Facings that walk AWAY from camera (NE / NW) invert the depth cues: the leading
  foot is the further one, raised slightly and shaded down.

PALETTE

  shirt #b0a58e   apron #6d6355   goatee #4a423a   skin #dfa877
  stain #6a5a3c   jeans #4a4453   boots #33282c
  bottle #2f6b45   label #c9bb92   pour #e0a848   outline #17101a

  Shirt, apron and goatee are the intended recolour slots if you want a second
  bartender without redrawing.
