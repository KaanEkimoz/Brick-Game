using Board;
using UnityEngine;

namespace Extras
{
    /// <summary>
    /// Triggers on 4-line Tetris clears: strong screen shake + colourful confetti particles.
    /// The ParticleSystem on this component carries the confetti config (random colours from
    /// a MinMaxGradient, gravity on, scattered velocities).
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class TetrisCelebrationEffect : MonoBehaviour
    {
        [SerializeField] private int _confettiCount = 1;
        [SerializeField] private ScreenShakeEffect _screenShake;

        private ParticleSystem _particles;

        private void Awake()
        {
            _particles = GetComponent<ParticleSystem>();
        }

        private void OnEnable()
        {
            BoardController.OnTetrisCleared += Play;
        }

        private void OnDisable()
        {
            BoardController.OnTetrisCleared -= Play;
        }

        private void Play()
        {
            if (_particles != null)
                _particles.Emit(_confettiCount);
            if (_screenShake != null)
                _screenShake.ShakeStrong();
        }

        [ContextMenu("Test Burst")]
        private void TestBurst()
        {
            if (_particles == null) _particles = GetComponent<ParticleSystem>();
            Play();
        }
    }
}
