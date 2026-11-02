#!/usr/bin/env python3
"""Validates level JSON files against Tools/levels/LEVEL_SPEC.md.

Usage: validate_levels.py [file...]   (no arguments: every Assets/Resources/Levels/levels_*.json)
Per-level rules run on any file; the set rules (duplicates, speed curve, Gold intro, fun levels)
compare whatever levels were passed, so run it on all four files before shipping.
"""
import glob
import json
import os
import sys

FIELDS = {
    "id": int, "name": str, "hint": str, "objective": str, "target": int, "rows": list,
    "pieces": str, "abilityMode": str, "abilityEvery": int, "noLineClears": bool,
    "dropLevel": int, "pieceLimit": int, "timeLimit": int, "garbageEvery": int,
    "hideGhost": bool, "hideNext": bool,
}
OBJECTIVES = {"Lines", "Score", "ClearBoard", "AbilityCells", "Tetrises", "Gold", "Combo"}
MODES = {"none", "bar", "bomb", "laser", "patch", "mixed"}
CHARGING = {"bomb", "laser", "patch", "mixed"}

BOARD_WIDTH, BOARD_HEIGHT = 10, 22
MIN_PIECES_TO_WIN = 3      # no level may be won by fewer well-placed pieces than this
MAX_DROP_JUMP = 3          # largest dropLevel step between neighbouring non-fun levels
MAX_LINES_LEVELS = 40      # keeps the objective mix varied
BOMB_SEARCH_WIDTH = 400    # beam width of the bomb-level winnability proof
PATCH_CELLS = 6
TOP_ROW = 20               # a piece resting at or above this row tops out

EMPTY, NORMAL, FIXED, GOLD, GOAL = 0, 1, 2, 3, 4
CELL = {".": EMPTY, "X": NORMAL, "#": FIXED, "*": GOLD}
SHAPES = {
    "O": ((0, 0), (1, 0), (0, 1), (1, 1)),
    "I": ((-1, 0), (0, 0), (1, 0), (2, 0)),
    "S": ((-1, 0), (0, 0), (0, 1), (1, 1)),
    "Z": ((-1, 1), (0, 1), (0, 0), (1, 0)),
    "L": ((-1, 0), (0, 0), (1, 0), (1, 1)),
    "J": ((-1, 1), (-1, 0), (0, 0), (1, 0)),
    "T": ((-1, 0), (0, 0), (1, 0), (0, 1)),
}


def rotations(letter):
    """All four rotations of a piece; tile order is kept so a charged tile can be tracked."""
    tiles, result = SHAPES[letter], []
    for _ in range(4):
        result.append(tiles)
        tiles = tuple((y, -x) for x, y in tiles)
    return result


ROTATIONS = {letter: rotations(letter) for letter in SHAPES}


# --- Board model. Mirrors BoardController and AbilityExecutor. ---

class Board:
    __slots__ = ("cells", "lines", "tetrises", "ability_cells", "score", "streak", "best_combo")

    def __init__(self, rows):
        self.cells = [[CELL[char] for char in row] for row in rows]
        self.cells += [[EMPTY] * BOARD_WIDTH for _ in range(BOARD_HEIGHT - len(rows))]
        self.lines = self.tetrises = self.ability_cells = self.score = self.streak = self.best_combo = 0

    def copy(self):
        other = Board.__new__(Board)
        other.cells = [row[:] for row in self.cells]
        other.lines, other.tetrises, other.ability_cells = self.lines, self.tetrises, self.ability_cells
        other.score, other.streak, other.best_combo = self.score, self.streak, self.best_combo
        return other

    def key(self):
        return bytes(cell for row in self.cells for cell in row)

    def count(self, *kinds):
        return sum(cell in kinds for row in self.cells for cell in row)

    def fits(self, tiles, x, y):
        for dx, dy in tiles:
            cx, cy = x + dx, y + dy
            if not 0 <= cx < BOARD_WIDTH or cy < 0:
                return False
            if cy < BOARD_HEIGHT and self.cells[cy][cx]:
                return False
        return True

    def landings(self, tiles):
        """Every straight drop of a shape, as lists of landed cells."""
        for x in range(-2, BOARD_WIDTH + 2):
            y = BOARD_HEIGHT - 1
            if not self.fits(tiles, x, y):
                continue
            while self.fits(tiles, x, y - 1):
                y -= 1
            cells = [(x + dx, y + dy) for dx, dy in tiles]
            if all(cy < TOP_ROW for _, cy in cells):
                yield cells

    def destroy(self, x, y):
        if self.cells[y][x] in (EMPTY, FIXED):
            return 0
        self.cells[y][x] = EMPTY
        return 1

    def compact_column(self, x):
        write = 0
        for y in range(BOARD_HEIGHT):
            cell = self.cells[y][x]
            if not cell:
                continue
            if cell == FIXED:
                write = y + 1
                continue
            if y != write:
                self.cells[write][x], self.cells[y][x] = cell, EMPTY
            write += 1

    def drop_rows(self, start):
        for y in range(start, BOARD_HEIGHT):
            row, below = self.cells[y], self.cells[y - 1]
            for x in range(BOARD_WIDTH):
                if row[x] and row[x] != FIXED and not below[x]:
                    below[x], row[x] = row[x], EMPTY

    def fire(self, ability, x, y, column_axis, level):
        changed = 0
        if ability == "bomb":
            for cy in range(max(y - 1, 0), min(y + 2, BOARD_HEIGHT)):
                for cx in range(max(x - 1, 0), min(x + 2, BOARD_WIDTH)):
                    changed += self.destroy(cx, cy)
            if changed:
                for cx in range(max(x - 1, 0), min(x + 2, BOARD_WIDTH)):
                    self.compact_column(cx)
        elif ability == "laser" and column_axis:
            changed = sum(self.destroy(x, cy) for cy in range(BOARD_HEIGHT))
        elif ability == "laser":
            changed = sum(self.destroy(cx, y) for cx in range(BOARD_WIDTH))
            if changed:
                self.drop_rows(y + 1)
        elif ability == "patch":
            tops = [max((cy + 1 for cy in range(BOARD_HEIGHT) if self.cells[cy][cx]), default=0)
                    for cx in range(BOARD_WIDTH)]
            for cy in range(BOARD_HEIGHT):
                for cx in range(BOARD_WIDTH):
                    if changed < PATCH_CELLS and not self.cells[cy][cx] and cy < tops[cx]:
                        self.cells[cy][cx] = NORMAL
                        changed += 1
        self.ability_cells += changed
        self.score += changed * 5 * (level["dropLevel"] + 1)

    def clear_lines(self, level):
        if level["noLineClears"]:
            self.streak = 0
            return
        full = [y for y in range(BOARD_HEIGHT) if all(self.cells[y])]
        for y in full:
            self.cells[y] = [FIXED if cell == FIXED else EMPTY for cell in self.cells[y]]
        for index, y in enumerate(full):
            self.drop_rows(y + 1 - index)
        count = len(full)
        self.lines += count
        self.tetrises += count >= 4
        if count:
            self.score += {1: 40, 2: 100, 3: 300}.get(count, 1200) * (level["dropLevel"] + 1)
        self.streak = self.streak + 1 if count else 0
        self.best_combo = max(self.best_combo, self.streak)

    def place(self, cells, ability, tile, column_axis, level):
        result = self.copy()
        for x, y in cells:
            result.cells[y][x] = NORMAL
        if ability:
            anchor_x, anchor_y = cells[tile]
            result.fire(ability, anchor_x, anchor_y, column_axis, level)
        result.clear_lines(level)
        return result

    def won(self, level):
        objective, target = level["objective"], level["target"]
        if objective == "Lines": return self.lines >= target
        if objective == "Score": return self.score >= target
        if objective == "AbilityCells": return self.ability_cells >= target
        if objective == "Tetrises": return self.tetrises >= target
        if objective == "Combo": return self.best_combo >= target
        if objective == "Gold": return self.count(GOLD) == 0
        return self.count(GOAL, GOLD) == 0


def starting_board(level):
    """Marks a ClearBoard level's starting tiles: tiles the player places never count."""
    board = Board(level["rows"])
    if level["objective"] == "ClearBoard":
        board.cells = [[GOAL if cell == NORMAL else cell for cell in row] for row in board.cells]
    return board


def abilities_for(level, piece_number):
    """Abilities the numbered piece (1-based) may carry."""
    every = level["abilityEvery"]
    if every <= 0 or piece_number % every:
        return [None]
    return ["bomb", "laser", "patch"] if level["abilityMode"] == "mixed" else [level["abilityMode"]]


def moves(board, level, piece_number):
    """Yields every board one piece can produce, by straight drop, for any allowed piece."""
    for ability in abilities_for(level, piece_number):
        if ability == "bomb":
            for cells in board.landings(((0, 0),)):
                yield board.place(cells, ability, 0, False, level)
            continue
        for letter in level["pieces"] or "OISZLJT":
            for rotation, tiles in enumerate(ROTATIONS[letter]):
                for cells in board.landings(tiles):
                    for tile in range(4) if ability else (0,):
                        yield board.place(cells, ability, tile, rotation % 2 == 1, level)


def can_win_within_two(level):
    """Bag order is ignored, so this also rejects levels that only a lucky deal wins this fast."""
    if cannot_finish_in_two(level):
        return 0
    start = starting_board(level)
    second_ply = {}
    for board in moves(start, level, 1):
        if board.won(level):
            return 1
        second_ply[board.key() + bytes((board.lines, board.tetrises, board.streak, board.best_combo))] = board
    for board in second_ply.values():
        if any(next_board.won(level) for next_board in moves(board, level, 2)):
            return 2
    return 0


def cannot_finish_in_two(level):
    """Cheap bounds that skip the search on levels that plainly take longer."""
    objective, target = level["objective"], level["target"]
    d = level["dropLevel"] + 1
    if objective == "Lines": return target > 8
    if objective == "Tetrises": return target > 2
    if objective == "Combo": return target > 2
    if objective == "AbilityCells": return target > 80
    if objective == "Score": return target > 2 * (1200 * d + 200 * d)
    return False


def solve_bomb_level(level):
    """Fewest bombs found to win a level where every piece is a bomb (a beam search, so an upper
    bound). Bag order and tile choice do not matter here, so a result is a proof of winnability."""
    start = starting_board(level)
    frontier = [start]
    if start.won(level):
        return 0
    limit = level["pieceLimit"] or 60
    for piece in range(1, limit + 1):
        seen = {}
        for board in frontier:
            for cells in board.landings(((0, 0),)):
                next_board = board.place(cells, "bomb", 0, False, level)
                if next_board.won(level):
                    return piece
                seen[next_board.key() + bytes((min(next_board.ability_cells, 255),))] = next_board
        frontier = sorted(seen.values(), key=lambda b: bomb_rank(b, level))[:BOMB_SEARCH_WIDTH]
    return None


def bomb_rank(board, level):
    if level["objective"] == "AbilityCells":
        return -board.ability_cells
    tiles = board.count(GOAL, GOLD)
    height = max((y for y in range(BOARD_HEIGHT) if any(c in (GOAL, GOLD) for c in board.cells[y])), default=0)
    return tiles * 3 + height


def always_self_destructs(level):
    """True when no placed piece can stay on the board, so the stack can never top out."""
    if level["abilityEvery"] != 1 or level["garbageEvery"]:
        return False
    if level["abilityMode"] == "bomb":
        return True
    return level["abilityMode"] == "laser" and level["pieces"] == "I"


def reachable_errors(rows):
    height = len(rows)
    seen = {(x, height) for x in range(10)}
    stack = list(seen)
    while stack:
        x, y = stack.pop()
        for nx, ny in ((x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)):
            if not (0 <= nx < 10 and 0 <= ny <= height) or (nx, ny) in seen:
                continue
            if ny < height and rows[ny][nx] != ".":
                continue
            seen.add((nx, ny))
            stack.append((nx, ny))
    return [(x, y) for y in range(height) for x in range(10) if rows[y][x] == "." and (x, y) not in seen]


def check(level):
    errors = []
    for field, kind in FIELDS.items():
        if field not in level:
            errors.append(f"missing {field}")
        elif type(level[field]) is not kind:
            errors.append(f"{field} must be {kind.__name__}")
    if errors:
        return errors

    rows = level["rows"]
    if len(level["name"]) > 18: errors.append("name over 18 chars")
    if len(level["hint"]) > 60: errors.append("hint over 60 chars")
    if level["objective"] not in OBJECTIVES: errors.append("bad objective")
    if level["abilityMode"] not in MODES: errors.append("bad abilityMode")
    if not 1 <= level["dropLevel"] <= 15: errors.append("dropLevel out of range")
    if len(rows) > 14: errors.append("stack over 14 rows")
    if set(level["pieces"]) - set("OISZLJT"): errors.append("bad pieces")
    for i, row in enumerate(rows):
        if len(row) != 10 or set(row) - set(".X#*"):
            errors.append(f"row {i} malformed")
        elif "." not in row:
            errors.append(f"row {i} starts full")
    if errors:
        return errors

    charging = level["abilityMode"] in CHARGING
    if charging != (level["abilityEvery"] > 0): errors.append("abilityEvery does not match abilityMode")
    if not charging and reachable_errors(rows): errors.append(f"sealed holes {reachable_errors(rows)[:4]}")
    if has_overhang(level): errors.append("ClearBoard board has an overhang")
    if level["noLineClears"] and not charging: errors.append("noLineClears needs a charging abilityMode")

    objective, target = level["objective"], level["target"]
    cells = "".join(rows)
    if objective in ("ClearBoard", "Gold") and target != 0: errors.append("target must be 0")
    if objective not in ("ClearBoard", "Gold") and target <= 0: errors.append("target must be positive")
    if objective == "ClearBoard" and not set(cells) & set("X*"): errors.append("ClearBoard needs tiles")
    if objective == "Gold" and "*" not in cells: errors.append("Gold needs * tiles")
    if objective == "Lines" and target > 40: errors.append("Lines target over 40")
    if objective == "Tetrises" and target > 5: errors.append("Tetrises target over 5")
    if objective == "Tetrises" and level["pieces"] and "I" not in level["pieces"]: errors.append("Tetrises needs I")
    if objective == "Combo" and target > 8: errors.append("Combo target over 8")
    if level["timeLimit"] and level["timeLimit"] < 30: errors.append("timeLimit under 30")
    if level["noLineClears"] and objective in ("Lines", "Tetrises", "Combo"): errors.append("objective needs line clears")

    limit = level["pieceLimit"]
    if limit and level["abilityEvery"] != 1:
        lines = {"Lines": target, "Tetrises": target * 4, "Combo": target}.get(objective, 0)
        if objective == "ClearBoard":
            lines = len(rows)
        if limit < lines * 3 + 4: errors.append(f"pieceLimit below {lines * 3 + 4}")

    if always_self_destructs(level):
        if not limit and not level["timeLimit"]:
            errors.append("unlosable: pieces destroy themselves and there is no pieceLimit or timeLimit")
    if level["abilityMode"] == "bomb" and level["abilityEvery"] == 1 and not level["garbageEvery"]:
        needed = solve_bomb_level(level)
        if needed is None:
            errors.append(f"bombs not proven to win within {limit or 60} pieces")
        elif limit and needed < MIN_PIECES_TO_WIN:
            errors.append(f"won by {needed} bombs")
        elif level["timeLimit"] and level["timeLimit"] < needed * 2:
            errors.append(f"timeLimit too short for {needed} bombs")

    drops = can_win_within_two(level)
    if drops:
        errors.append(f"won in {drops} straight drop(s), needs at least {MIN_PIECES_TO_WIN} pieces")
    return errors


def has_overhang(level):
    """A ClearBoard level without abilities can only be filled from above: pieces lock on landing."""
    if level["objective"] != "ClearBoard" or level["abilityMode"] != "none":
        return False
    rows = level["rows"]
    return any(rows[y][x] == "." and any(row[x] != "." for row in rows[y + 1:])
               for y in range(len(rows)) for x in range(10))


def is_fun(level):
    return level["id"] % 10 == 0


def fun_shape_ok(level):
    every_charged = level["abilityEvery"] == 1
    if level["objective"] == "Tetrises":
        return not every_charged
    return every_charged and level["abilityMode"] in ("bomb", "laser")


def check_set(levels):
    """Rules that compare levels with each other. Returns (level id, message) pairs."""
    errors = []
    ordered = sorted(levels, key=lambda level: level["id"])

    layouts = {}
    for level in ordered:
        if not level["rows"]:
            continue
        shape = tuple("".join("." if c == "." else "X" for c in row) for row in level["rows"])
        mirror = tuple(row[::-1] for row in shape)
        twin = layouts.get(shape) or layouts.get(mirror)
        if twin is not None:
            errors.append((level["id"], f"same layout as level {twin} (mirrors and tile kinds ignored)"))
        layouts.setdefault(shape, level["id"])

    plain = [level for level in ordered if not is_fun(level)]
    for before, after in zip(plain, plain[1:]):
        jump = abs(after["dropLevel"] - before["dropLevel"])
        if after["id"] - before["id"] <= 2 and jump > MAX_DROP_JUMP:
            errors.append((after["id"], f"dropLevel jumps {jump} from level {before['id']}"))

    gold = [level for level in ordered if level["objective"] == "Gold" or "*" in "".join(level["rows"])]
    if gold and not (gold[0]["hint"].startswith("New:") and "gold" in gold[0]["hint"].lower()):
        errors.append((gold[0]["id"], "first Gold level must be the intro (hint 'New: ... gold ...')"))

    for level in ordered:
        if is_fun(level) and not fun_shape_ok(level):
            errors.append((level["id"], "every 10th level must be bomb only, laser only or Tetris only"))
        if not is_fun(level) and (level["abilityEvery"] == 1 or level["objective"] == "Tetrises"):
            errors.append((level["id"], "only fun levels charge every piece or ask for Tetrises"))

    lines_levels = sum(level["objective"] == "Lines" for level in ordered)
    if len(ordered) == 100 and lines_levels > MAX_LINES_LEVELS:
        errors.append((0, f"{lines_levels} Lines levels, at most {MAX_LINES_LEVELS}"))
    return errors


def default_files():
    root = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..")
    return sorted(glob.glob(os.path.join(root, "Assets", "Resources", "Levels", "levels_*.json")))


def main():
    failed = False
    everything = []
    for path in sys.argv[1:] or default_files():
        with open(path, encoding="utf-8") as handle:
            levels = json.load(handle)["levels"]
        for level in levels:
            for error in check(level):
                failed = True
                print(f"{path}: level {level.get('id')}: {error}")
        print(f"{path}: {len(levels)} levels checked")
        everything += levels
    if all(type(level.get("id")) is int and "rows" in level for level in everything):
        for level_id, error in check_set(everything):
            failed = True
            print(f"level {level_id}: {error}")
    print("FAILED" if failed else "OK")
    sys.exit(1 if failed else 0)


if __name__ == "__main__":
    main()
