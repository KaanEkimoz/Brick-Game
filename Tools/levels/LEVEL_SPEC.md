# Brick Game Levels spec

100 hand-designed levels. A new mechanic arrives every 5 levels. Level text is English.
Board: 10 columns, 22 rows. Levels live in `Assets/Resources/Levels/levels_XXX_YYY.json`
as `{ "levels": [ ... ] }`. Every field below is required on every level (JsonUtility).

## Level fields

| Field | Type | Meaning |
|---|---|---|
| `id` | int | 1-100 |
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
- `ClearBoard` is won when only `#` tiles remain on the board, so tiles the player placed
  must also be gone.

## Abilities

- Bomb: destroys a 3x3 around the charged tile. Laser: the charged tile's row, or column after
  rotating once. Patch: fills the 6 lowest covered holes. `bar`: clearing 5 lines charges one
  random ability.
- `AbilityCells` counts cells destroyed by Bomb or Laser and cells filled by Patch.

## Scoring (for `Score` targets), d = dropLevel

1/2/3/4 lines = 40/100/300/1200 x (d+1). Ability = 5 x cells x (d+1).

## Order

Every 10th level (10, 20 ... 100) is a fun level: bomb only, laser only or Tetris only. No other
level has every piece charged or a `Tetrises` objective. The rest are mixed by difficulty, and each
mechanic keeps its own intro level first. The table lists the mechanics, not the final level numbers.

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
| 51-55 | Fixed high speed | `dropLevel` 10-15 |
| 56-60 | Limited pieces | `pieces` subset |
| 61-65 | No ghost | `hideGhost` |
| 66-70 | Rising garbage | `garbageEvery` |
| 71-75 | Tetris only | `Tetrises`, `pieces` including `I` |
| 76-80 | Gold hunt | `Gold`, `*` buried in the stack |
| 81-85 | Blind next | `hideNext` |
| 86-90 | Combo | `Combo`: N pieces in a row that each clear a line |
| 91-95 | Remix | 2 earlier mechanics per level |
| 96-100 | Finale | 3 or more mechanics, level 100 the hardest |

Earlier mechanics may return in later levels. Mix objectives inside a tier when the table allows.
Each tier opens easy and ends harder. Levels must stay fair on a phone.

## Fairness rules (the validator enforces these)

Run `python3 Tools/levels/validate_levels.py <file>` until it prints OK.

- No row starts full. Prefilled stack is at most 14 rows.
- Every empty cell inside the stack must connect to the top through empty cells, unless the
  level has `patch`, `bomb`, `laser` or `mixed` abilities.
- `pieceLimit`, when set, must allow for the goal: at least 3 pieces per line needed plus 4.
- `Lines` target at most 40, `Tetrises` at most 5, `Combo` at most 8.
- `timeLimit` at least 30.
- `noLineClears` requires a piece-charging `abilityMode`.
