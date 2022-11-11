using UnityEngine;
using UnityEngine.UI;
public class NextPiece : MonoBehaviour
{
    public Image nextPieceImage;
    public GameObject frontCanvas;
    public GameObject[] pieceImages;

    private GameObject currentShownImage;
    private void OnEnable()
    {
        PieceSpawner.NextPieceChanged += ShowNextPiece;
    }
    private void OnDisable()
    {
        PieceSpawner.NextPieceChanged -= ShowNextPiece;
    }
    private void ShowNextPiece()
    {
        if(currentShownImage)
            Destroy(currentShownImage);
        foreach (var pieceImage in pieceImages)
        {
            if (pieceImage.gameObject.name == PieceSpawner.NextPieceType.ToString())
                currentShownImage = Instantiate(pieceImage,nextPieceImage.transform.position,Quaternion.identity,frontCanvas.transform);
        }
    }
}
