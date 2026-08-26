# Booth Sprite — SE Wall

Isometric booth (two upholstered benches + pedestal table with glasses) for the bar map.

## Files

| File | Size | Use |
| --- | --- | --- |
| `booth_se.png` | 181 × 127 px | Source art, 1 px = 1 game pixel. Import this. |
| `booth_se-3x.png` | 543 × 381 px | Nearest-neighbour 3× preview for reference only. |

## Import settings (Unity)

- Texture Type: **Sprite (2D and UI)**
- Sprite Mode: **Single** — this prop is a single static frame, no animation
- Pixels Per Unit: match the rest of the bar props
- Filter Mode: **Point (no filter)**
- Compression: **None**
- Wrap Mode: Clamp
- Generate Mip Maps: off

## Placement

- **Pivot:** bottom centre. The lowest opaque row is the near bench's front face, so
  the sprite sits on the floor tile when the pivot is on that row.
- **Facing:** built for a wall running **SE**. The benches' long axis runs along the
  wall; the table sits between them. Mirror horizontally for the opposing wall.
- **Projection:** 2:1 isometric, same axes and light direction (upper right) as the
  other bar props, so it drops straight into the map without re-lighting.
- **Sorting:** one sprite, one sort key. For an NPC seated in the near bench, sort the
  NPC in front of the booth; the table's pedestal leaves open floor under the top so
  legs can show through.

## Notes

- 11715 opaque pixels, 25 colours, transparent background (trimmed — no padding).
- Cushions have rounded ends, stitched seams and a piped front edge; the table carries
  five glasses. If you need an empty table, that variant can be generated as a second frame.
- Palette matches the shared bar palette: reds `#c94a56`/`#a8323e`/`#7d2029`, wood
  `#d99a52`/`#a4682a`, chrome `#c3c6d3`.
