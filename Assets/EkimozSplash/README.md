# Ekimoz Games splash

Plays the Ekimoz Games intro once per app launch, before the first scene shows: ten blocks fall
into an "E", a light runs over them, EKIMOZ rises in letter by letter and GAMES settles under it.
About 3.3 seconds; a tap skips to the fade-out. Everything is drawn in code, the font and the
jingle sit in `Resources/EkimozSplash/`.

## Use in another game

1. Copy this `EkimozSplash` folder into `Assets/`. The project needs TextMeshPro (built into uGUI).
2. That is all: `EkimozSplash` starts itself (`RuntimeInitializeOnLoadMethod`, BeforeSceneLoad).
3. Turn off Unity's own splash if the plan allows it (Player Settings > Splash Image).

## Optional hooks (from game code)

- `EkimozSplash.SoundAllowed = () => mySfxOn;` set at `SubsystemRegistration` to mute the jingle.
- `EkimozSplash.IsShowing` / `EkimozSplash.OnFinished` to hold banners or popups until it is gone.
  Brick Game does both in `Assets/Scripts/Scene/SplashGlue.cs`.

## Changing it

- Timing, colors and layout: constants at the top of `Runtime/EkimozSplash.cs`.
- Jingle: `python3 Tools/splash/make_splash_sound.py Assets/EkimozSplash/Resources/EkimozSplash/ekimoz_splash.wav`
  (keep its block timings in step with the script).
- Font: Venus Rising (Typodermic), static TMP atlas; the source `.ttf` is in `Fonts/`.
