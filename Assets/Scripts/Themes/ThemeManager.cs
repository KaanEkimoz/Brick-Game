using Extras;
using InGame;
using TMPro;
using Tiles;
using UnityEngine;
using UnityEngine.Events;
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
        private const float MinBackgroundHeight = 1000f; // Unity's built-in UI sprite shares the name
        private const string PopupSpriteName = "!TetrisGameSprites_2";
        private const string ChargeBarObjectName = "AbilityChargeBar";
        private const float PopupBorderDensity = 1f;
        private const string PlayOneShotMethod = "PlayOneShot";
        private const string HudObjectName = "TopHUD";
        private const string SoundPanelObjectName = "SoundPanel";
        private const string FrameSpritePrefix = "!TetrisGameSprites_";
        private const string ButtonSpriteName = "!TetrisGameSprites_3";
        private const string PanelSpriteName = "PanelBackground";
        private const string GridSpriteName = "Grid";
        private const string LogoObjectName = "I_Logo";
        private static readonly string[] ExplosionTextures = { "explosion_one_texture", "spritesheet" };
        private const string PieceLetters = "IJLOSTZ";
        private const string FrameObjectName = "I_GridFrame";
        private const string NextBoxObjectName = "I_NextPiece";
        private const string PauseObjectName = "B_Pause";
        private const string RemoveAdsObjectName = "B_RemoveAds_Game";
        private const float PanelBorderDensity = 1.6f;
        private const float FrameBorderDensity = 1.5f;
        private const float BoardFramePadding = 13f;
        private static readonly Vector2 HolderPadding = new Vector2(16f, 18f);
        private static readonly Vector2 PanelInset = new Vector2(-18f, -8f);

        private static GameTheme _active;
        private static bool _ghostHooked;

        /// <summary>The theme's asset name, or empty for the standard look.</summary>
        public static string Selected
        {
            get => PlayerPrefs.GetInt(Ekimoz.IAP.PurchaseManager.PremiumPrefKey) == 1 ? PlayerPrefs.GetString(ThemeKey) : string.Empty;
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
            _active = null;
            if (string.IsNullOrEmpty(Selected)) return;

            GameTheme theme = Resources.Load<GameTheme>(ResourceFolder + Selected);
            if (theme == null) return;

            _active = theme;
            ApplyVisuals(theme);
            ApplyText(theme);
            HookGhost();
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

                if (image.sprite.texture.name == BackgroundTextureName
                    && image.sprite.rect.height > MinBackgroundHeight && theme.Background != null)
                {
                    image.sprite = theme.Background;
                    image.color = Color.white;
                }
                else if (image.name == FrameObjectName && theme.FrameSprite != null)
                    image.color = Color.clear;
                else if (image.name == NextBoxObjectName && theme.NextBoxSprite != null)
                {
                    image.sprite = theme.NextBoxSprite;
                    image.color = Color.white;
                }
                else if (image.sprite.name == PanelSpriteName && theme.PanelSprite != null)
                {
                    ApplyPanel(image, theme.PanelSprite, PanelBorderDensity);
                    image.rectTransform.sizeDelta += PanelInset;
                }
                else if (image.name == LogoObjectName && theme.Logo != null)
                    image.sprite = theme.Logo;
                else if (image.name == SoundPanelObjectName)
                    image.color = theme.SoundPanelTint;
                else if (image.sprite.name == PopupSpriteName && image.name != ChargeBarObjectName && theme.PopupSprite != null)
                    ApplyPanel(image, theme.PopupSprite, PopupBorderDensity);
                else if (image.sprite.name == ButtonSpriteName)
                    image.color = theme.ButtonTint;
                else if (image.sprite.name == PanelSpriteName)
                    image.color = theme.PanelTint;
                else if (image.sprite.name.StartsWith(FrameSpritePrefix) && image.sprite.name != ButtonSpriteName)
                    image.color = theme.FrameTint;
            }

        }

        /// <summary>The pause menu styles its buttons when it first opens.</summary>
        public static Color PrimaryButtonTint(Color standard) => _active != null ? _active.ButtonTint : standard;
        public static Color PrimaryButtonText(Color standard) => _active != null ? _active.ButtonTextColor : standard;
        public static Color SecondaryButtonTint(Color standard) => _active != null ? _active.SecondaryButtonTint : standard;

        /// <summary>The mode screen's art, matched by the standard image's name inside the file name.</summary>
        public static Texture2D ModeImage(string imageName, Texture2D standard)
        {
            if (_active == null || _active.ModeImages == null) return standard;

            foreach (Texture2D image in _active.ModeImages)
                if (image != null && image.name.EndsWith(imageName)) return image;
            return standard;
        }

        /// <summary>The charge bar draws itself from code, so it asks for each color.</summary>
        public static Color ChargeColor(System.Func<GameTheme, Color> pick, Color standard)
        {
            if (string.IsNullOrEmpty(Selected)) return standard;

            GameTheme theme = _active != null ? _active : Resources.Load<GameTheme>(ResourceFolder + Selected);
            return theme != null ? pick(theme) : standard;
        }

        /// <summary>Swaps a click chain's scene-wired sound. ConfirmDialog keeps chains apart
        /// from their buttons, so it hands them over itself.</summary>
        public static void ThemeClickSound(UnityEvent clickChain)
        {
            if (string.IsNullOrEmpty(Selected)) return;

            GameTheme theme = Resources.Load<GameTheme>(ResourceFolder + Selected);
            if (theme == null || theme.ButtonClip == null) return;

            for (int i = 0; i < clickChain.GetPersistentEventCount(); i++)
            {
                if (clickChain.GetPersistentMethodName(i) != PlayOneShotMethod) continue;
                if (!(clickChain.GetPersistentTarget(i) is AudioSource source)) continue;
                if (clickChain.GetPersistentListenerState(i) == UnityEventCallState.Off) continue;

                clickChain.SetPersistentListenerState(i, UnityEventCallState.Off);
                clickChain.AddListener(() => source.PlayOneShot(theme.ButtonClip));
            }
        }

        /// <summary>The theme's click sound, or null for the standard one.</summary>
        public static AudioClip ButtonClip
        {
            get
            {
                if (string.IsNullOrEmpty(Selected)) return null;
                GameTheme theme = Resources.Load<GameTheme>(ResourceFolder + Selected);
                return theme != null ? theme.ButtonClip : null;
            }
        }

        /// <summary>The theme's font. LevelUi reads it in Awake, before the scene finishes loading.</summary>
        public static TMP_FontAsset ThemeFont(TMP_FontAsset standard)
        {
            if (string.IsNullOrEmpty(Selected)) return standard;

            GameTheme theme = Resources.Load<GameTheme>(ResourceFolder + Selected);
            return theme != null && theme.Font != null ? theme.Font : standard;
        }

        // The plain font stays for long sentences in dialogs.
        private static void ApplyFont(TMP_FontAsset font)
        {
            if (font == null) return;

            foreach (TMP_Text label in Resources.FindObjectsOfTypeAll<TMP_Text>())
                if (label.gameObject.scene.IsValid() && label.font != Levels.PlainFont.Asset)
                    label.font = font;
        }

        private static void ApplyText(GameTheme theme)
        {
            ApplyFont(theme.Font);

            GameObject hud = GameObject.Find(HudObjectName);
            if (hud != null)
                foreach (TMP_Text label in hud.GetComponentsInChildren<TMP_Text>(true))
                    label.color = theme.PanelTextColor;

            foreach (Button button in Resources.FindObjectsOfTypeAll<Button>())
            {
                if (!button.gameObject.scene.IsValid() || !(button.targetGraphic is Image image)) continue;
                if (image.sprite == null || image.sprite.name != ButtonSpriteName) continue;

                TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
                if (label != null) label.color = theme.ButtonTextColor;
            }
        }

        private static void HookGhost()
        {
            if (_ghostHooked) return;
            _ghostHooked = true;
            PieceSpawner.OnPieceSpawned += ThemeGhost;
        }

        private static void ThemeGhost()
        {
            if (_active == null || _active.GhostSprite == null) return;

            foreach (GhostTileController tile in FindAll<GhostTileController>())
            {
                SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
                renderer.sprite = _active.GhostSprite;
                renderer.color = _active.GhostTint;
            }
        }

        private static void ApplyPanel(Image image, Sprite sprite, float borderDensity)
        {
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = borderDensity;
            image.color = Color.white;
        }

        // The standard frame art has wide empty margins, so this one is fitted to the grid itself.
        private static void AddBoardFrame(Sprite sprite, Bounds grid)
        {
            GameObject standard = GameObject.Find(FrameObjectName);
            if (sprite == null || standard == null) return;

            RectTransform parent = (RectTransform)standard.transform;
            Canvas canvas = parent.GetComponentInParent<Canvas>();
            Camera canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Canvas.ForceUpdateCanvases();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, Camera.main.WorldToScreenPoint(grid.min), canvasCamera, out Vector2 lower);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, Camera.main.WorldToScreenPoint(grid.max), canvasCamera, out Vector2 upper);

            GameObject frame = new GameObject("I_ThemeFrame", typeof(RectTransform), typeof(Image));
            frame.transform.SetParent(parent, false);
            frame.transform.SetAsFirstSibling();
            Image image = frame.GetComponent<Image>();
            image.raycastTarget = false;
            ApplyPanel(image, sprite, FrameBorderDensity);

            RectTransform rect = (RectTransform)frame.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = upper - lower + Vector2.one * BoardFramePadding * 2f;
            rect.localPosition = (lower + upper) * 0.5f;
        }

        // Placed once the canvas has its final layout, behind both buttons.
        private static void AddButtonHolder(Sprite sprite)
        {
            GameObject pause = GameObject.Find(PauseObjectName);
            GameObject removeAds = GameObject.Find(RemoveAdsObjectName);
            if (sprite == null || pause == null || removeAds == null) return;

            Canvas.ForceUpdateCanvases();
            Vector3[] corners = new Vector3[4];
            Vector3[] others = new Vector3[4];
            ((RectTransform)pause.transform).GetWorldCorners(corners);
            ((RectTransform)removeAds.transform).GetWorldCorners(others);

            Transform parent = pause.transform.parent;
            GameObject holder = new GameObject("I_ButtonHolder", typeof(RectTransform), typeof(Image));
            holder.transform.SetParent(parent, false);
            holder.transform.SetSiblingIndex(pause.transform.GetSiblingIndex());
            holder.GetComponent<Image>().sprite = sprite;
            holder.GetComponent<Image>().raycastTarget = false;

            Vector3 lower = parent.InverseTransformPoint(Vector3.Min(corners[0], others[0]));
            Vector3 upper = parent.InverseTransformPoint(Vector3.Max(corners[2], others[2]));
            RectTransform rect = (RectTransform)holder.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = (Vector2)(upper - lower) + HolderPadding * 2f;
            rect.localPosition = (lower + upper) * 0.5f;
        }

        /// <summary>The board builds its grid in Start, after the theme is applied.</summary>
        public static void TintGrid(Transform board)
        {
            if (string.IsNullOrEmpty(Selected)) return;

            GameTheme theme = Resources.Load<GameTheme>(ResourceFolder + Selected);
            if (theme == null) return;

            Bounds grid = new Bounds();
            bool hasGrid = false;
            foreach (SpriteRenderer renderer in board.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (renderer.sprite == null || renderer.sprite.name != GridSpriteName) continue;
                if (theme.GridSprite != null) renderer.sprite = theme.GridSprite;
                renderer.color = theme.GridTint;

                if (hasGrid) grid.Encapsulate(renderer.bounds);
                else grid = renderer.bounds;
                hasGrid = true;
            }

            if (hasGrid) AddBoardFrame(theme.FrameSprite, grid);
            AddButtonHolder(theme.ButtonHolderSprite);
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
                if (sheet.GetSprite(0) == null) continue;
                if (System.Array.IndexOf(ExplosionTextures, sheet.GetSprite(0).texture.name) < 0) continue;

                while (sheet.spriteCount > 0) sheet.RemoveSprite(0);
                foreach (Sprite frame in frames) sheet.AddSprite(frame);

                // The standard fade colors belong to the explosion art.
                ParticleSystem.ColorOverLifetimeModule colors = particles.colorOverLifetime;
                colors.enabled = false;
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

            foreach (Button button in Resources.FindObjectsOfTypeAll<Button>())
                if (button.gameObject.scene.IsValid())
                    ThemeClickSound(button.onClick);

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
