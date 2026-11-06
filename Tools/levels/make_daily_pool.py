# Builds Assets/Resources/Daily/daily_challenges.json, the daily challenge pool.
# Usage: python3 Tools/levels/make_daily_pool.py Assets/Resources/Daily/daily_challenges.json
# Then: python3 Tools/levels/validate_levels.py --daily Assets/Resources/Daily/daily_challenges.json
import json, sys
OUT = sys.argv[1]
def L(name, hint, objective, target, rows=(), pieces="", mode="none", every=0, noclear=False,
      drop=4, limit=0, time=0, garbage=0, ghost=False, nxt=False):
    return dict(name=name, hint=hint, objective=objective, target=target, rows=list(rows),
                pieces=pieces, abilityMode=mode, abilityEvery=every, noLineClears=noclear,
                dropLevel=drop, pieceLimit=limit, timeLimit=time, garbageEvery=garbage,
                hideGhost=ghost, hideNext=nxt)
pool = [
 L("Warm Lines", "Clear 10 lines within 40 pieces.", "Lines", 10, drop=4, limit=40),
 L("Quick Points", "Reach 3000 points in 40 pieces.", "Score", 3000, drop=4, limit=40),
 L("Blue Sweep", "Clear every blue tile.", "ClearBoard", 0,
   rows=["XXXX.XXXXX", "XXX..XXXX.", "X......XX."], drop=3),
 L("Chain Four", "Clear lines with 4 pieces in a row.", "Combo", 4,
   rows=["XXXXXXXX..", "XXXXXXXX..", "XXXXXXXX..", "XXXXXXX...", "XXXXXXX..."], drop=3),
 L("Gold Dig", "Dig out all the gold.", "Gold", 0,
   rows=["XXX*XXXX.X", "XXXXXXXX.X", "X*XXXXXX.X", "XXXXXXX*.X", "*X.XX.XX.."], drop=4),
 L("Beat the Clock", "Clear 8 lines in 60 seconds.", "Lines", 8, drop=5, time=60),
 L("Bomb Day", "Every piece is a bomb. Blast the board clean.", "ClearBoard", 0,
   rows=["XXXXX.XXXX", "XXXX..XXXX", "XXX....XXX"], mode="bomb", every=1, noclear=True, drop=3, limit=20),
 L("Laser Day", "Every piece fires a laser. Hit 40 cells.", "AbilityCells", 40,
   rows=["XXXXXXXX.X", "XXXX.XXXXX", "X.XXXXXXXX", "XXXXXX.XXX"], pieces="I", mode="laser", every=1, drop=3, limit=25),
 L("Quad Day", "Clear two Quads.", "Tetrises", 2,
   rows=["XXXXXXXXX.", "XXXXXXXXX.", "XXXXXXXXX.", "XXXXXXXX.."], pieces="IOLJ", drop=4),
 L("Blind Drop", "No preview today. Clear 10 lines.", "Lines", 10, drop=5, nxt=True),
 L("No Ghost", "No landing ghost. Clear 8 lines.", "Lines", 8, drop=4, ghost=True),
 L("Rising Tide", "Garbage rises. Clear 12 lines.", "Lines", 12, drop=4, garbage=7),
 L("Three Shapes", "Only I, O and T today. Clear 10 lines.", "Lines", 10, pieces="IOT", drop=4),
 L("Patch Work", "Patches fill holes. Clear 8 lines.", "Lines", 8,
   rows=["X.XXX.XXXX", "XX.XXXX.XX", "XXXX.XX.XX", "X..XXXXX.X"], mode="patch", every=5, drop=4),
 L("Charge Up", "Fill the bar and hit 20 cells with abilities.", "AbilityCells", 20, mode="bar", drop=4),
 L("Iron Blocks", "Clear the blue tiles around fixed blocks.", "ClearBoard", 0,
   rows=["#XXXX.XXX#", "#XXX..XXX#", "#X......X#"], drop=4),
 L("Fast Lane", "High speed. Clear 8 lines.", "Lines", 8, drop=10),
 L("Score Rush", "Reach 5000 points in 90 seconds.", "Score", 5000, drop=4, time=90),
 L("Long Chain", "Clear lines with 5 pieces in a row.", "Combo", 5,
   rows=["XXXXXXXX..", "XXXXXXXX..", "XXXXXXXX..", "XXXXXXXX..", "XXXXXXX...", "XXXXXXX..."], drop=3),
 L("Gold Rush", "Garbage rises. Grab the gold first.", "Gold", 0,
   rows=["XX*XXXX.XX", "XXXXXXX.*X", "X.XX*XX.XX", "X.*X.XX.X."], drop=4, garbage=8),
 L("Mixed Bag", "Every third piece has an ability. Hit 30 cells.", "AbilityCells", 30,
   rows=["XXXX.XXXXX", "XX.XXXXX.X", "XXXXXX.XXX"], mode="mixed", every=3, drop=4),
 L("Deep Clean", "Clear every blue tile.", "ClearBoard", 0,
   rows=["XXXXXX.XXX", "XXXXX..XXX", "XXXX...XXX", "XX.....XX.", "X.......X."], drop=4),
 L("Steady Hands", "No ghost. Clear 15 lines in 50 pieces.", "Lines", 15, drop=4, limit=50, ghost=True),
 L("Blind Quad", "No preview. Clear one Quad.", "Tetrises", 1,
   rows=["XXXXXX.XXX", "XXXXXX.XXX", "XXXXXX.XXX", "XX.X...X.X"], pieces="", drop=4, nxt=True),
 L("Line Lasers", "Lasers on every other piece. Clear 10 lines.", "Lines", 10, mode="laser", every=2, drop=4),
 L("Bomb Gold", "Bombs every fourth piece. Free the gold.", "Gold", 0,
   rows=["XXXX*XXX.X", "X*XXXX.XXX", "XXX.XXXX*X", "XX.XXX.XXX"], mode="bomb", every=4, drop=4),
 L("Sprint", "Clear 6 lines in 45 seconds.", "Lines", 6, drop=6, time=45),
 L("Remix", "Garbage and no ghost. Reach 4000 points.", "Score", 4000, drop=4, ghost=True, garbage=8),
]
for i, level in enumerate(pool, 1):
    level = {"id": i, **level}
    pool[i - 1] = level
json.dump({"levels": pool}, open(OUT, "w", encoding="utf-8"), indent=1, ensure_ascii=False)
print(len(pool))
