using Board;
using UnityEngine;

namespace Extras
{
    /// <summary>
    /// Plays a particle burst at the centre of every cleared row. Multi-line clears trigger
    /// one burst per row in the same frame; the ParticleSystem must be configured with World
    /// simulation space so each burst stays at its emit position.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class LineClearEffect : MonoBehaviour
    {
        [SerializeField] private BoardController _boardController;
        [SerializeField] private int _particlesPerRow = 35;

        private ParticleSystem _particles;

        private void Awake()
        {
            _particles = GetComponent<ParticleSystem>();
        }

        private void OnEnable()
        {
            BoardController.OnLineCleared += Play;
        }

        private void OnDisable()
        {
            BoardController.OnLineCleared -= Play;
        }

        private void Play(int rowY)
        {
            if (_particles == null || _boardController == null) return;
            float cx = (_boardController.gridSizeX - 1) * 0.5f;
            transform.position = new Vector3(cx, rowY, 0f);
            _particles.Emit(_particlesPerRow);
        }
    }
}
