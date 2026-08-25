BARTENDER SPRITE — UNITY IMPORT
==============================
File
  bartender_sheet.png     56 x 88 px, cell 14 x 22
  bartender_sheet-4x.png  preview only, do not import

Grid
  Columns: idle, step A, idle, step B  (pour row: pour A, pour B, pour A, pour B)
  Rows (top to bottom):
    1  WALK NE     patrol toward the NE end of the bar
    2  WALK SW     patrol toward the SW end (mirror of row 1)
    3  IDLE        facing the customer, no stride — all 4 frames identical
    4  POUR        2-frame loop (poses A/B alternate), bottle over a glass
  Pivot: bottom centre.

Behaviour (random back-and-forth patrol)
  1. Pick a random point along the bar counter.
  2. Walk toward it (row 1 if point is ahead/NE, row 2 if behind/SW),
     looping frames 0-3 at ~8 samples/sec.
  3. On arrival, switch to POUR for a random 1-2 seconds.
  4. Pick a new random point (can be either direction) and repeat.
  A MonoBehaviour state machine (Idle/Walk/Pour) driven by a Random.Range
  target and a simple Vector2.MoveTowards covers this; no NavMesh needed
  since movement is 1D along the counter.

Import settings
  Texture Type      Sprite (2D and UI)
  Sprite Mode       Multiple
  Pixels Per Unit   14        matches the rest of the bar sprite set
  Mesh Type         Full Rect
  Filter Mode       Point (no filter)
  Compression       None
  Generate Mip Maps off
  -> Sprite Editor > Slice > Grid By Cell Size 14 x 22,
     Pivot Bottom Center > Slice > Apply

Clips (Samples 8, loop)
  walk_ne row 1   walk_sw row 2   idle row 3 (1 frame is enough)   pour row 4
