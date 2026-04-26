using Gameplay;
using UnityEngine;

namespace Extras
{
    /// <summary>
    /// Plays an explosion sound when the Bomb ability detonates (AbilityExecutor.OnBombExploded).
    /// Shares an AudioSource with the other SFX components on this GameObject so mute /
    /// volume stay consistent.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AbilityBombSFX : MonoBehaviour
    {
        [SerializeField] private AudioClip _bombClip;
        [SerializeField] [Range(0f, 1f)] private float _volume = 1f;

        private AudioSource _source;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            AbilityExecutor.OnBombExploded += PlayBomb;
        }

        private void OnDisable()
        {
            AbilityExecutor.OnBombExploded -= PlayBomb;
        }

        private void PlayBomb(Vector2Int _)
        {
            if (_source != null && _bombClip != null)
                _source.PlayOneShot(_bombClip, _volume);
        }
    }
}
