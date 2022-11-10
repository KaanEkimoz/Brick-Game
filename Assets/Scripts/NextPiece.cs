using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextPiece : MonoBehaviour
{
    public Image nextPieceImage;
    public GameObject frontCanvas;
    public GameObject[] pieceVisuals;

    private GameObject currentObject;

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
        if(currentObject)
            Destroy(currentObject);
        foreach (var piece in pieceVisuals)
        {
            if (piece.gameObject.name == PieceSpawner.NextPieceType.ToString())
                currentObject = Instantiate(piece,nextPieceImage.transform.position,Quaternion.identity,frontCanvas.transform);
        }
    }
}
