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
        [SerializeField] private ScreenShakeEffect _screenShake;
        [Tooltip("Extra world-space margin added to both sides of the confetti emitter so the rain extends past the screen edges.")]
        [SerializeField] private float _edgeOvershoot = 1f;
        [Tooltip("World-space offset above the top of the camera viewport where confetti starts falling from.")]
        [SerializeField] private float _topMargin = 0.5f;

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

        private void Start()
        {
            FitToCameraViewport();
        }

        // Positions the confetti emitter at the top centre of the camera viewport and stretches
        // the shape so the rainfall spans the full screen width regardless of aspect ratio.
        private void FitToCameraViewport()
        {
            var cam = Camera.main;
            if (cam == null || _particles == null) return;

            float z = Mathf.Abs(cam.transform.position.z);
            Vector3 topLeft = cam.ViewportToWorldPoint(new Vector3(0f, 1f, z));
            Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1f, 1f, z));
            float width = topRight.x - topLeft.x + _edgeOvershoot * 2f;
            float centerX = (topLeft.x + topRight.x) * 0.5f;
            float topY = topRight.y + _topMargin;

            // Emitter sits in world space at the top centre of the camera
            transform.position = new Vector3(centerX, topY, 0f);

            // Stretch the emission shape to match the viewport width
            var shape = _particles.shape;
            Vector3 scale = shape.scale;
            scale.x = width;
            shape.scale = scale;
        }

        private void Play()
        {
            if (_particles != null)
            {
                _particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                _particles.Play();
            }
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
