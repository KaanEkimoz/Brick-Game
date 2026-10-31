#!/usr/bin/env python3
"""Validates level JSON files against Tools/levels/LEVEL_SPEC.md. Usage: validate_levels.py file..."""
import json
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
    if limit:
        lines = {"Lines": target, "Tetrises": target * 4, "Combo": target}.get(objective, 0)
        if objective == "ClearBoard":
            lines = len(rows)
        if limit < lines * 3 + 4: errors.append(f"pieceLimit below {lines * 3 + 4}")
    return errors


def main():
    failed = False
    for path in sys.argv[1:]:
        with open(path, encoding="utf-8") as handle:
            levels = json.load(handle)["levels"]
        for level in levels:
            for error in check(level):
                failed = True
                print(f"{path}: level {level.get('id')}: {error}")
        print(f"{path}: {len(levels)} levels checked")
    print("FAILED" if failed else "OK")
    sys.exit(1 if failed else 0)


if __name__ == "__main__":
    main()
