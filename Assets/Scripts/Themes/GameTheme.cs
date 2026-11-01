using TMPro;
using UnityEngine;

namespace Themes
{
    /// <summary>Everything a theme replaces. An empty field keeps what the scene already uses.</summary>
    [CreateAssetMenu(menuName = "Brick Game/Theme", fileName = "Theme")]
    public class GameTheme : ScriptableObject
    {
        [SerializeField] private Sprite[] _tileSprites;
        [SerializeField] private Sprite _background;
        [SerializeField] private Sprite _gridSprite;
        [SerializeField] private TMP_FontAsset _font;
        [SerializeField] private Sprite _logo;
        [SerializeField] private Texture2D[] _modeImages;
        [SerializeField] private Sprite _panelSprite;
        [SerializeField] private Sprite _frameSprite;
        [SerializeField] private Sprite _popupSprite;
        [SerializeField] private Sprite _ghostSprite;
        [SerializeField] private Sprite _nextBoxSprite;
        [SerializeField] private Sprite _buttonHolderSprite;
        [SerializeField] private Sprite[] _particleFrames;
        [SerializeField] private Color _frameTint = Color.white;
        [SerializeField] private Color _buttonTint = Color.white;
        [SerializeField] private Color _buttonTextColor = Color.white;
        [SerializeField] private Color _secondaryButtonTint = Color.white;
        [SerializeField] private Color _panelTextColor = Color.white;
        [SerializeField] private Color _ghostTint = Color.white;
        [SerializeField] private Color _soundPanelTint = Color.white;
        [SerializeField] private Color _chargeFillColor = Color.white;
        [SerializeField] private Color _chargeTrackColor = Color.black;
        [SerializeField] private Color _chargeLowColor = Color.white;
        [SerializeField] private Color _chargePanelColor = Color.black;
        [SerializeField] private Color _chargeBorderColor = Color.white;
        [SerializeField] private Color _panelTint = Color.white;
        [SerializeField] private Color _gridTint = Color.white;
        [SerializeField] private Color _effectColor = Color.white;
        [SerializeField] private AudioClip _lineClearClip;
        [SerializeField] private AudioClip _landingClip;
        [SerializeField] private AudioClip _softDropClip;
        [SerializeField] private AudioClip _bombClip;
        [SerializeField] private AudioClip _laserClip;
        [SerializeField] private AudioClip _buttonClip;
        [SerializeField] private float _musicCutoff;

        public Sprite[] TileSprites => _tileSprites;
        public Sprite Background => _background;
        public Sprite GridSprite => _gridSprite;
        public TMP_FontAsset Font => _font;
        public Sprite Logo => _logo;
        public Texture2D[] ModeImages => _modeImages;
        public Sprite PanelSprite => _panelSprite;
        public Sprite FrameSprite => _frameSprite;
        public Sprite PopupSprite => _popupSprite;
        public Sprite GhostSprite => _ghostSprite;
        public Sprite NextBoxSprite => _nextBoxSprite;
        public Sprite ButtonHolderSprite => _buttonHolderSprite;
        public Sprite[] ParticleFrames => _particleFrames;
        public Color FrameTint => _frameTint;
        public Color ButtonTint => _buttonTint;
        public Color ButtonTextColor => _buttonTextColor;
        public Color SecondaryButtonTint => _secondaryButtonTint;
        public Color PanelTextColor => _panelTextColor;
        public Color GhostTint => _ghostTint;
        public Color SoundPanelTint => _soundPanelTint;
        public Color ChargeFillColor => _chargeFillColor;
        public Color ChargeTrackColor => _chargeTrackColor;
        public Color ChargeLowColor => _chargeLowColor;
        public Color ChargePanelColor => _chargePanelColor;
        public Color ChargeBorderColor => _chargeBorderColor;
        public Color PanelTint => _panelTint;
        public Color GridTint => _gridTint;
        public Color EffectColor => _effectColor;
        public AudioClip LineClearClip => _lineClearClip;
        public AudioClip LandingClip => _landingClip;
        public AudioClip SoftDropClip => _softDropClip;
        public AudioClip BombClip => _bombClip;
        public AudioClip LaserClip => _laserClip;
        public AudioClip ButtonClip => _buttonClip;
        public float MusicCutoff => _musicCutoff;
    }
}
