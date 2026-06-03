using InGame;
using UnityEngine;

namespace Extras
{
    /// <summary>
    /// Plays a softer, pitch-shifted version of the hard-drop clip whenever the player
    /// drives the piece down (PiecesController.OnSoftDrop). Spawns a one-shot AudioSource
    /// per tick so the per-play pitch / volume sticks without affecting other SFX.
    /// </summary>
    public class SoftDropSFX : MonoBehaviour
    {
        [SerializeField] private AudioClip _softDropClip;
        [SerializeField] [Range(0.5f, 2.5f)] private float _pitch = 1.55f;
        [SerializeField] [Range(0f, 1f)] private float _volume = 0.06f;

        private void OnEnable()
        {
            PiecesController.OnSoftDrop += PlayTick;
        }

        private void OnDisable()
        {
            PiecesController.OnSoftDrop -= PlayTick;
        }

        private void PlayTick()
        {
            if (!SFXToggle.IsSfxEnabled) return;
            if (_softDropClip == null) return;

            GameObject go = new GameObject("SoftDropSFX_OneShot");
            go.transform.SetParent(transform, false);

            AudioSource src = go.AddComponent<AudioSource>();
            src.clip = _softDropClip;
            src.volume = _volume;
            src.pitch = _pitch;
            src.playOnAwake = false;
            src.loop = false;
            src.Play();

            float lifetime = _softDropClip.length / Mathf.Max(0.05f, _pitch);
            Destroy(go, lifetime);
        }
    }
}
