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
                    _currentShownImage = Instantiate(pieceImage,_nextPieceImage.transform.position,Quaternion.identity,transform);
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
