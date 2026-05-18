using Piece;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PieceLandingSFX : MonoBehaviour
{
    [SerializeField] private AudioClip _landingClip;

    [SerializeField]
    [Tooltip("Volume multiplier for the landing one-shot — the thud when a piece actually settles " +
             "on the board. OnPieceSettled fires only inside SetPiece(), so this clip plays once " +
             "per landing (not per soft-drop tick). Keep at full strength.")]
    [Range(0f, 1f)] private float _volume = 1f;

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
            _source.PlayOneShot(_landingClip, _volume);
    }
}
