using System;
using InGame;
using UnityEngine;
using UnityEngine.UI;

namespace Extras
{
    public class NextPiece : MonoBehaviour
    {
        public GameObject frontCanvas;
        public GameObject[] pieceImages;

        [SerializeField]
        [Tooltip("Uniform scale applied to the spawned preview prefab. The raw prefabs are sized "
               + "in world units that overflow the redesigned 140-px slot, so we shrink them on "
               + "the way in. Tune in the Inspector if a different slot size needs different fit.")]
        [Range(0.1f, 2f)]
        private float _previewScale = 0.6f;

        /// <summary>Fired right after a new next-piece image is instantiated. Effects hook here to animate it.</summary>
        public event Action<GameObject> OnImageInstantiated;

        private Image _nextPieceImage;
        private GameObject _currentShownImage;

        private void Awake()
        {
            _nextPieceImage = GetComponent<Image>();
        }
        private void OnEnable()
        {
            PieceSpawner.OnNextPieceChanged += ShowNextPiece;
        }
        private void OnDisable()
        {
            PieceSpawner.OnNextPieceChanged -= ShowNextPiece;
        }
        private void ShowNextPiece(PieceType nextPieceType)
        {
            //if we already have an image, destroy it
            if(_currentShownImage)
                Destroy(_currentShownImage);
        
            //finds the correct image from the array
            foreach (var pieceImage in pieceImages)
            {
                if (pieceImage.gameObject.name ==  nextPieceType.ToString())
                {
                    _currentShownImage = Instantiate(pieceImage, transform);

                    // Centre the spawned preview inside this slot regardless of the parent's
                    // pivot/anchor. The old version passed _nextPieceImage.transform.position
                    // (a world-space point at the parent's pivot) which left the prefab pinned
                    // to whichever corner the slot's pivot sat on.
                    var rt = _currentShownImage.transform as RectTransform;
                    if (rt != null)
                    {
                        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                        rt.pivot = new Vector2(0.5f, 0.5f);
                        rt.anchoredPosition = Vector2.zero;
                        rt.localScale = Vector3.one * _previewScale;
                    }
                    else
                    {
                        _currentShownImage.transform.localPosition = Vector3.zero;
                        _currentShownImage.transform.localScale = Vector3.one * _previewScale;
                    }

                    OnImageInstantiated?.Invoke(_currentShownImage);
                }
            }
        }
        private void DestroyCurrentShownImage()
        {
            Destroy(_currentShownImage);
        }
    }
}
