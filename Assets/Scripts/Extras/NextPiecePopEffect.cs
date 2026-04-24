using System.Collections;
using UnityEngine;

namespace Extras
{
    /// <summary>
    /// When a new next-piece image is spawned, scale-pops it from slightly larger down to 1x
    /// via an ease-out curve. Stand-alone sibling of NextPiece; no particles.
    /// </summary>
    [RequireComponent(typeof(NextPiece))]
    public class NextPiecePopEffect : MonoBehaviour
    {
        [SerializeField] private float _duration = 0.18f;
        [SerializeField] private float _startScale = 1.3f;

        private NextPiece _nextPiece;
        private Coroutine _running;

        private void Awake()
        {
            _nextPiece = GetComponent<NextPiece>();
        }

        private void OnEnable()
        {
            if (_nextPiece != null) _nextPiece.OnImageInstantiated += Handle;
        }

        private void OnDisable()
        {
            if (_nextPiece != null) _nextPiece.OnImageInstantiated -= Handle;
            if (_running != null) { StopCoroutine(_running); _running = null; }
        }

        private void Handle(GameObject img)
        {
            if (img == null) return;
            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(Pop(img.transform));
        }

        private IEnumerator Pop(Transform t)
        {
            if (t == null) yield break;
            Vector3 baseScale = t.localScale;
            float time = 0f;
            while (time < _duration)
            {
                if (t == null) yield break;
                time += Time.deltaTime;
                float n = Mathf.Clamp01(time / _duration);
                float ease = 1f - Mathf.Pow(1f - n, 3f); // ease-out cubic
                float s = Mathf.Lerp(_startScale, 1f, ease);
                t.localScale = baseScale * s;
                yield return null;
            }
            if (t != null) t.localScale = baseScale;
            _running = null;
        }
    }
}
