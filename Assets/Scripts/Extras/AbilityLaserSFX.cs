using Gameplay;
using UnityEngine;

namespace Extras
{
    /// <summary>
    /// Plays a laser sound each time the horizontal row or vertical column ability
    /// detonates. Drops in on any AudioSource — mute / volume are driven by the
    /// existing SFX toggle on the same source.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AbilityLaserSFX : MonoBehaviour
    {
        [SerializeField] private AudioClip _laserClip;
        [SerializeField] [Range(0f, 1f)] private float _volume = 1f;

        private AudioSource _source;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            AbilityExecutor.OnRowExploded += PlayLaser;
            AbilityExecutor.OnColumnExploded += PlayLaser;
        }

        private void OnDisable()
        {
            AbilityExecutor.OnRowExploded -= PlayLaser;
            AbilityExecutor.OnColumnExploded -= PlayLaser;
        }

        private void PlayLaser(Vector2Int _)
        {
            if (!SFXToggle.IsSfxEnabled) return;
            if (_source != null && _laserClip != null)
                _source.PlayOneShot(_laserClip, _volume);
        }
    }
}
