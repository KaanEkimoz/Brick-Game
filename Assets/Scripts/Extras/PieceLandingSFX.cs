using Piece;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PieceLandingSFX : MonoBehaviour
{
    [SerializeField] private AudioClip _landingClip;
    private AudioSource _source;

    void Awake()
    {
        _source = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        PieceMovement.OnPieceSettled += Play;
    }

    void OnDisable()
    {
        PieceMovement.OnPieceSettled -= Play;
    }

    private void Play()
    {
        if (_source != null && _landingClip != null)
            _source.PlayOneShot(_landingClip);
    }
}
