# Builds Assets/EkimozSplash/Resources/EkimozSplash/ekimoz_splash.wav, timed to EkimozSplash.cs:
# a soft rising air at the start, one pitched tick per landing block, a warm bell chord when the
# mark is complete. Pure Python, no dependencies.
# Usage: python3 Tools/splash/make_splash_sound.py <output.wav>
import math, random, struct, sys, wave

RATE = 44100
LENGTH = 3.2
# Keep in step with EkimozSplash.cs: TileStart + i * TileStagger + TileFall.
TILE_START, TILE_STAGGER, TILE_FALL, TILES = 0.15, 0.07, 0.38, 10
CHIME_AT = 1.22

n = int(RATE * LENGTH)
out = [0.0] * n
random.seed(7)

def add(i, v):
    if 0 <= i < n:
        out[i] += v

# Rising air: low-passed noise whose cutoff and level climb, then fade before the first landing.
lp = 0.0
for i in range(int(RATE * 0.62)):
    t = i / RATE
    cutoff = 0.02 + 0.10 * (t / 0.62)
    lp += cutoff * (random.uniform(-1, 1) - lp)
    env = math.sin(math.pi * min(1.0, t / 0.62)) ** 2
    add(i, lp * env * 0.12)

# Landing ticks: short plucks climbing a pentatonic scale.
scale = [523.25, 587.33, 659.25, 783.99, 880.00, 1046.5, 1174.7, 1318.5, 1568.0, 1760.0]
for k in range(TILES):
    start = int(RATE * (TILE_START + k * TILE_STAGGER + TILE_FALL))  # the block lands here
    f = scale[k]
    for j in range(int(RATE * 0.18)):
        t = j / RATE
        env = math.exp(-t * 38.0) * min(1.0, t / 0.002)
        v = math.sin(2 * math.pi * f * t) + 0.35 * math.sin(2 * math.pi * f * 2.01 * t) * math.exp(-t * 60)
        add(start + j, v * env * 0.16)

# Bell chord: a soft Cmaj9 an octave up, each note doubled a few cents apart for a slow shimmer,
# with inharmonic bell partials, a gentle attack and a slow decay.
chord = [523.25, 659.25, 783.99, 987.77, 1174.66]
partials = [(1.0, 1.0), (2.0, 0.3), (2.76, 0.14), (4.1, 0.05)]
detune = [0.997, 1.003]
start = int(RATE * CHIME_AT)
for j in range(n - start):
    t = j / RATE
    env = math.exp(-t * 1.5) * min(1.0, t / 0.012)
    v = 0.0
    for idx, f in enumerate(chord):
        for d in detune:
            for mult, amp in partials:
                v += amp * math.sin(2 * math.pi * f * d * mult * t + idx) * math.exp(-t * (0.7 + mult * 0.6))
    add(start + j, v * env * 0.035)

# A short glint on top of the chord.
for j in range(int(RATE * 0.6)):
    t = j / RATE
    env = math.exp(-t * 6.0) * min(1.0, t / 0.004)
    add(start + j, math.sin(2 * math.pi * 2093.0 * t) * env * 0.05)

# A small room: four feedback combs and two allpasses (Schroeder), mixed in lightly.
def comb(signal, delay, feedback, damp):
    buf = [0.0] * delay
    res = [0.0] * len(signal)
    idx, low = 0, 0.0
    for i, x in enumerate(signal):
        y = buf[idx]
        low = y * (1 - damp) + low * damp
        buf[idx] = x + low * feedback
        idx = (idx + 1) % delay
        res[i] = y
    return res

def allpass(signal, delay, gain):
    buf = [0.0] * delay
    res = [0.0] * len(signal)
    idx = 0
    for i, x in enumerate(signal):
        y = buf[idx]
        buf[idx] = x + y * gain
        res[i] = y - x * gain
        idx = (idx + 1) % delay
    return res

wet = [0.0] * n
for delay in (1557, 1617, 1491, 1422):
    for i, v in enumerate(comb(out, delay, 0.8, 0.3)):
        wet[i] += v * 0.25
for delay in (225, 556):
    wet = allpass(wet, delay, 0.5)
out = [d + w * 0.3 for d, w in zip(out, wet)]

peak = max(abs(v) for v in out) or 1.0
gain = 0.6 / peak
fade = int(RATE * 0.25)
for i in range(fade):
    out[n - 1 - i] *= i / fade

with wave.open(sys.argv[1], "wb") as w:
    w.setnchannels(1)
    w.setsampwidth(2)
    w.setframerate(RATE)
    w.writeframes(b"".join(struct.pack("<h", int(max(-1, min(1, v * gain)) * 32767)) for v in out))
print("wrote", sys.argv[1], round(LENGTH, 2), "s")
