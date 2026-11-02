# Brick Game Levels spec

115 hand-designed levels; level 16 onward needs Premium. A new mechanic arrives every 5 levels. Level text is English.
Board: 10 columns, 22 rows. Levels live in `Assets/Resources/Levels/levels_XXX_YYY.json`
as `{ "levels": [ ... ] }`. Every field below is required on every level (JsonUtility).

## Level fields

| Field | Type | Meaning |
|---|---|---|
| `id` | int | 1-115 |
| `name` | string | Short title, max 18 chars |
| `hint` | string | Max 60 chars, shown before the level. On a tier's first level, explain the new mechanic |
| `objective` | string | `Lines`, `Score`, `ClearBoard`, `AbilityCells`, `Tetrises`, `Gold`, `Combo` |
| `target` | int | Goal amount. `0` for `ClearBoard` and `Gold` |
| `rows` | string[] | Prefilled board, BOTTOM row first, each exactly 10 chars. `[]` for an empty board |
| `pieces` | string | Allowed pieces from `OISZLJT`, e.g. `"IO"`. `""` means all seven |
| `abilityMode` | string | `none`, `bar`, `bomb`, `laser`, `patch`, `mixed` |
| `abilityEvery` | int | For `bomb`/`laser`/`patch`/`mixed`: every Nth piece is charged, 1 = all. `0` otherwise |
| `noLineClears` | bool | Full rows do NOT clear; only abilities remove tiles |
| `dropLevel` | int | Fixed fall speed 1-15 for the whole level (1 slow, 15 fast) |
| `pieceLimit` | int | Max pieces, `0` = none |
| `timeLimit` | int | Seconds, `0` = none |
| `garbageEvery` | int | Every N pieces a garbage row with one gap rises from the bottom, `0` = none |
| `hideGhost` | bool | No landing ghost |
| `hideNext` | bool | No next-piece preview |

## Board characters

`.` empty, `X` normal tile, `#` fixed tile, `*` gold tile.

- A fixed tile is never destroyed and never moves, except that garbage pushes it up.
  A row counts as full when all 10 cells are occupied; clearing it removes everything
  except fixed tiles. Tiles above fall until blocked.
- A gold tile behaves like a normal tile. The `Gold` objective is won when every `*` is gone.
- `ClearBoard` is won when every starting `X` and `*` is gone. Its `X` tiles show as blue, and
  tiles the player placed never count.

## Abilities

- Bomb: a charged piece shrinks to a single 1x1 tile. It lands, destroys the 3x3 around itself
  (fixed tiles survive), destroys itself, and the three columns it touched settle. With
  `abilityEvery: 1` and `bomb` every piece is one tile, so the piece bag does not matter, the board
  can never grow, and the level needs a `pieceLimit` or `timeLimit` to be losable.
- Laser: the charged tile's row, or column after rotating once. The other tiles of the piece stay,
  except that an `I` always sits in the line it fires along. So `laser` with `abilityEvery: 1` and
  `pieces: "I"` can never grow either and needs a limit too.
- Patch: fills the 6 lowest covered holes. `bar`: clearing 5 lines charges one random ability.
- Dynamite: appears only through the `bar` charge in Extended, as a 1x2 piece that clears 3x4 along its direction. No level mode charges it.
- `AbilityCells` counts cells destroyed by Bomb or Laser, the bomb's own tile included, and cells
  filled by Patch.
- A fixed tile does not fall, so tiles above it hang when the rows below clear. A `#` in the middle
  of a stack leaves that column out of step with the rest. Keep `#` in full-height columns or on
  top of the stack.

## Scoring (for `Score` targets), d = dropLevel

1/2/3/4 lines = 40/100/300/1200 x (d+1). Ability = 5 x cells x (d+1).

## Order

Every 10th level (10, 20 ... 100) is a fun level: bomb only, laser only or Tetris only. No other
level has every piece charged or a `Tetrises` objective. The rest are mixed by difficulty, and each
mechanic keeps its own intro level first: the first Gold level is the Gold intro, with a hint that
starts `New:`. The table lists the mechanics, not the final level numbers.

`dropLevel` rises steadily from 1 at level 1 to 15 at level 100, roughly `1 + (id - 1) x 14 / 99`.
Neighbouring non-fun levels differ by at most 3. Fun levels may sit a step below the curve.
`Score` targets scale with `dropLevel`, since the score does. Hints never quote a speed.

## Tiers

| Levels | New mechanic | Required setup |
|---|---|---|
| 1-5 | Clear lines | `Lines`, gentle speed |
| 6-10 | Reach score | `Score` |
| 11-15 | Clear the board | `ClearBoard`, small prefilled boards |
| 16-20 | Fixed blocks | `#` tiles with `Lines` or `ClearBoard` |
| 21-25 | Ability bar | `abilityMode: bar`, `AbilityCells` or `Lines` |
| 26-30 | Bomb party (fun) | `abilityMode: bomb`, `abilityEvery: 1`, `noLineClears: true`, `ClearBoard` or `AbilityCells` |
| 31-35 | Laser party | `abilityMode: laser`, `abilityEvery` 1-2 |
| 36-40 | Patch | `abilityMode: patch`, holed boards, `Lines` |
| 41-45 | Piece limit | `pieceLimit` |
| 46-50 | Time limit | `timeLimit` |
| 51-55 | High speed | `dropLevel` from the curve above |
| 56-60 | Limited pieces | `pieces` subset |
| 61-65 | No ghost | `hideGhost` |
| 66-70 | Rising garbage | `garbageEvery` |
| 71-75 | Tetris only | `Tetrises`, `pieces` including `I` |
| 76-80 | Gold hunt | `Gold`, `*` buried in the stack |
| 81-85 | Blind next | `hideNext` |
| 86-90 | Combo | `Combo`: N pieces in a row that each clear a line |
| 91-95 | Remix | 2 earlier mechanics per level |
| 96-100 | Finale | 3 or more mechanics, level 100 the hardest |
| 101-115 | Encore | Earlier mechanics at top speed, level 115 the hardest |

Earlier mechanics may return in later levels. Mix objectives inside a tier when the table allows.
Each tier opens easy and ends harder. Levels must stay fair on a phone.

## Fairness rules (the validator enforces these)

Run `python3 Tools/levels/validate_levels.py` until it prints OK. With no arguments it checks all
level files; the rules that compare levels only see the files passed in.

- No row starts full. Prefilled stack is at most 14 rows.
- Every empty cell inside the stack must connect to the top through empty cells, unless the
  level has `patch`, `bomb`, `laser` or `mixed` abilities.
- A `ClearBoard` level without abilities has no overhangs: pieces lock on landing and cannot slide under tiles.
- `pieceLimit`, when set, must allow for the goal: at least 3 pieces per line needed plus 4.
  Levels with `abilityEvery: 1` are exempt, because abilities do the clearing there.
- `Lines` target at most 40, `Tetrises` at most 5, `Combo` at most 8.
- `timeLimit` at least 30.
- `noLineClears` requires a piece-charging `abilityMode`.
- No level is won in fewer than 3 pieces (brute force over straight drops, any piece order).
- A level where every piece destroys itself has a `pieceLimit` or `timeLimit`.
- A level where every piece is a bomb is searched for a win within its `pieceLimit`.
- No two levels share a layout. Mirrors count as the same, and so do layouts that differ only in
  tile kind.
- `dropLevel` moves by at most 3 between non-fun neighbours.
- The first Gold level is the Gold intro, and every 10th level follows the fun rule.
- At most 40 of the 100 levels use the `Lines` objective.
