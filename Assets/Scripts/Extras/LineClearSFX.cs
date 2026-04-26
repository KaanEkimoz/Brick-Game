using Board;
using UnityEngine;

namespace Extras
{
    /// <summary>
    /// Plays the explosion-particle clip when one or more rows are cleared, trimmed
    /// to a configurable start offset and play duration so the head/tail of the raw
    /// clip can be cut to match the LineClearVFX timing. Spawns a one-shot AudioSource
    /// GameObject per play so overlapping clears don't cut each other off.
    /// </summary>
    public class LineClearSFX : MonoBehaviour
    {
        [SerializeField] private AudioClip _clip;
        [SerializeField] private float _trimStart = 0.1f;
        [SerializeField] private float _playDuration = 0.35f;
        [SerializeField] [Range(0f, 1f)] private float _volume = 1f;
        [SerializeField] private AudioSource _muteReference;

        private void OnEnable()
        {
            BoardController.OnLinesCleared += HandleLinesCleared;
        }

        private void OnDisable()
        {
            BoardController.OnLinesCleared -= HandleLinesCleared;
        }

        private void HandleLinesCleared(int count)
        {
            if (count <= 0 || _clip == null) return;
            if (_muteReference != null && _muteReference.mute) return;

            GameObject go = new GameObject("LineClearSFX_OneShot");
            go.transform.SetParent(transform, false);

            AudioSource src = go.AddComponent<AudioSource>();
            src.clip = _clip;
            src.volume = _volume;
            src.playOnAwake = false;
            src.loop = false;

            float startClamped = Mathf.Clamp(_trimStart, 0f, _clip.length - 0.01f);
            src.time = startClamped;
            src.Play();

            float duration = Mathf.Min(_playDuration, _clip.length - startClamped);
            Destroy(go, duration);
        }
    }
}
