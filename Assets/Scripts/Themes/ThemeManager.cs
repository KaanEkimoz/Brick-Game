using Extras;
using InGame;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Themes
{
    /// <summary>Applies the selected theme each time the scene loads, after every Awake and
    /// before every Start, so the game reads the themed values from its first frame.</summary>
    public static class ThemeManager
    {
        private const string ThemeKey = "game_theme";
        private const string ResourceFolder = "Themes/";
        private const string BackgroundTextureName = "Background";
        private const string FrameSpritePrefix = "!TetrisGameSprites_";
        private const string ButtonSpriteName = "!TetrisGameSprites_3";
        private const string PanelSpriteName = "PanelBackground";
        private const string GridSpriteName = "Grid";
        private const string LogoObjectName = "I_Logo";
        private const string ExplosionTexturePrefix = "explosion_one";
        private const string PieceLetters = "IJLOSTZ";

        /// <summary>The theme's asset name, or empty for the standard look.</summary>
        public static string Selected
        {
            get => PlayerPrefs.GetString(ThemeKey);
            set
            {
                PlayerPrefs.SetString(ThemeKey, value);
                PlayerPrefs.Save();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private static void HandleSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            if (string.IsNullOrEmpty(Selected)) return;

            GameTheme theme = Resources.Load<GameTheme>(ResourceFolder + Selected);
            if (theme == null) return;

            ApplyVisuals(theme);
            ApplyEffects(theme.EffectColor);
            ApplyParticles(theme.ParticleFrames);
            foreach (NextPiece preview in FindAll<NextPiece>())
                preview.OnImageInstantiated += image => ThemePreview(image, theme);
            ApplySounds(theme);
        }

        private static void ApplyVisuals(GameTheme theme)
        {
            PieceSpawner spawner = Object.FindAnyObjectByType<PieceSpawner>(FindObjectsInactive.Include);
            if (spawner != null && theme.TileSprites.Length == spawner.tileSprites.Length)
                spawner.tileSprites = theme.TileSprites;

            foreach (Image image in Resources.FindObjectsOfTypeAll<Image>())
            {
                if (!image.gameObject.scene.IsValid() || image.sprite == null) continue;

                if (image.sprite.texture.name == BackgroundTextureName && theme.Background != null)
                    image.sprite = theme.Background;
                else if (image.name == LogoObjectName && theme.Logo != null)
                    image.sprite = theme.Logo;
                else if (image.sprite.name == ButtonSpriteName)
                    image.color = theme.ButtonTint;
                else if (image.sprite.name == PanelSpriteName)
                    image.color = theme.PanelTint;
                else if (image.sprite.name.StartsWith(FrameSpritePrefix) && image.sprite.name != ButtonSpriteName)
                    image.color = theme.FrameTint;
            }

        }

        /// <summary>The board builds its grid in Start, after the theme is applied.</summary>
        public static void TintGrid(Transform board)
        {
            if (string.IsNullOrEmpty(Selected)) return;

            GameTheme theme = Resources.Load<GameTheme>(ResourceFolder + Selected);
            if (theme == null) return;

            foreach (SpriteRenderer renderer in board.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (renderer.sprite == null || renderer.sprite.name != GridSpriteName) continue;
                if (theme.GridSprite != null) renderer.sprite = theme.GridSprite;
                renderer.color = theme.GridTint;
            }
        }

        private static void ApplyEffects(Color color)
        {
            foreach (LineClearEffect effect in FindAll<LineClearEffect>()) effect.SetColor(color);
            foreach (LineExplosionEffect effect in FindAll<LineExplosionEffect>()) effect.SetColor(color);
            foreach (HardDropSparkEffect effect in FindAll<HardDropSparkEffect>()) effect.SetColor(color);
            foreach (HardDropStreakEffect effect in FindAll<HardDropStreakEffect>()) effect.SetColor(color);
            foreach (BombExplosionEffect effect in FindAll<BombExplosionEffect>()) effect.SetColor(color);
        }

        private static void ApplyParticles(Sprite[] frames)
        {
            if (frames == null || frames.Length == 0) return;

            foreach (ParticleSystem particles in FindAll<ParticleSystem>())
            {
                ParticleSystem.TextureSheetAnimationModule sheet = particles.textureSheetAnimation;
                if (!sheet.enabled || sheet.mode != ParticleSystemAnimationMode.Sprites || sheet.spriteCount == 0) continue;
                if (sheet.GetSprite(0) == null || !sheet.GetSprite(0).texture.name.StartsWith(ExplosionTexturePrefix)) continue;

                while (sheet.spriteCount > 0) sheet.RemoveSprite(0);
                foreach (Sprite frame in frames) sheet.AddSprite(frame);
            }
        }

        // The preview prefabs show the standard tiles, named after their piece.
        private static void ThemePreview(GameObject preview, GameTheme theme)
        {
            foreach (Image image in preview.GetComponentsInChildren<Image>(true))
            {
                if (image.sprite == null || image.sprite.name.Length != 1) continue;
                int index = PieceLetters.IndexOf(image.sprite.name[0]);
                if (index >= 0 && index < theme.TileSprites.Length) image.sprite = theme.TileSprites[index];
            }
        }

        private static void ApplySounds(GameTheme theme)
        {
            foreach (LineClearSFX sound in FindAll<LineClearSFX>()) sound.SetClip(theme.LineClearClip);
            foreach (PieceLandingSFX sound in FindAll<PieceLandingSFX>()) sound.SetClip(theme.LandingClip);
            foreach (SoftDropSFX sound in FindAll<SoftDropSFX>()) sound.SetClip(theme.SoftDropClip);
            foreach (AbilityBombSFX sound in FindAll<AbilityBombSFX>()) sound.SetClip(theme.BombClip);
            foreach (AbilityLaserSFX sound in FindAll<AbilityLaserSFX>()) sound.SetClip(theme.LaserClip);

            if (theme.MusicCutoff <= 0f) return;
            foreach (BGMusicToggle music in FindAll<BGMusicToggle>())
            {
                AudioLowPassFilter filter = music.gameObject.AddComponent<AudioLowPassFilter>();
                filter.cutoffFrequency = theme.MusicCutoff;
            }
        }

        private static T[] FindAll<T>() where T : Object
        {
            return Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }
    }
}
