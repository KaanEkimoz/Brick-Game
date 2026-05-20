using System;
using Gameplay;
using Piece;
using Tiles;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InGame
{
    public enum PieceType { O, I, S, Z, L, J, T, Bomb, Laser }

    public class PieceSpawner : MonoBehaviour
    {
        [Space]
        [Header("Prefabs")]
        public GameObject ghostPiecePrefab;
        public GameObject piecePrefab;

        [Tooltip("Ability piece prefabs, ordered by AbilityType (0=Bomb, 1=Laser).")]
        [SerializeField] private GameObject[] _abilityPiecePrefabs;

        [Space]
        public Vector2Int spawnPosition;

        [Space]
        public Sprite[] tileSprites;

        public static Action OnPieceSpawned;
        public static Action<PieceType> OnNextPieceChanged;
    
        private PieceType _curPieceType;
        private static PieceType _nextPieceType;
        private TileController[] _tiles;

        private void Start()
        {
            _nextPieceType = (PieceType)Random.Range(0, 7);
        }

        /// <summary>
        /// Moves the attached tiles to form the Tetris piece specified. Also sets the correct color of tile sprite.
        /// </summary>
        /// <param name="newType">Type of tetris piece to be spawned.</param>
        private void UpdateTiles(PieceType newType, PieceController pieceController)
        {
            PieceType curType = newType;
            _tiles = PieceController.Tiles;
            _tiles[0].UpdatePosition(spawnPosition);

            switch (curType)
            {
                case PieceType.I:
                    _tiles[1].UpdatePosition(spawnPosition + Vector2Int.left);
                    _tiles[2].UpdatePosition(spawnPosition + (Vector2Int.right * 2));
                    _tiles[3].UpdatePosition(spawnPosition + Vector2Int.right);
                    SetTileSprites(tileSprites[0]);
                    break;

                case PieceType.J:
                    _tiles[1].UpdatePosition(spawnPosition + Vector2Int.left);
                    _tiles[2].UpdatePosition(spawnPosition + new Vector2Int(-1, 1));
                    _tiles[3].UpdatePosition(spawnPosition + Vector2Int.right);
                    SetTileSprites(tileSprites[1]);
                    break;

                case PieceType.L:
                    _tiles[1].UpdatePosition(spawnPosition + Vector2Int.left);
                    _tiles[2].UpdatePosition(spawnPosition + new Vector2Int(1, 1));
                    _tiles[3].UpdatePosition(spawnPosition + Vector2Int.right);
                    SetTileSprites(tileSprites[2]);
                    break;

                case PieceType.O:
                    _tiles[1].UpdatePosition(spawnPosition + Vector2Int.right);
                    _tiles[2].UpdatePosition(spawnPosition + new Vector2Int(1, 1));
                    _tiles[3].UpdatePosition(spawnPosition + Vector2Int.up);
                    SetTileSprites(tileSprites[3]);
                    break;

                case PieceType.S:
                    _tiles[1].UpdatePosition(spawnPosition + Vector2Int.left);
                    _tiles[2].UpdatePosition(spawnPosition + new Vector2Int(1, 1));
                    _tiles[3].UpdatePosition(spawnPosition + Vector2Int.up);
                    SetTileSprites(tileSprites[4]);
                    break;

                case PieceType.T:
                    _tiles[1].UpdatePosition(spawnPosition + Vector2Int.left);
                    _tiles[2].UpdatePosition(spawnPosition + Vector2Int.up);
                    _tiles[3].UpdatePosition(spawnPosition + Vector2Int.right);
                    SetTileSprites(tileSprites[5]);
                    break;

                case PieceType.Z:
                    _tiles[1].UpdatePosition(spawnPosition + Vector2Int.up);
                    _tiles[2].UpdatePosition(spawnPosition + new Vector2Int(-1, 1));
                    _tiles[3].UpdatePosition(spawnPosition + Vector2Int.right);
                    SetTileSprites(tileSprites[6]);
                    break;
            }
            int index = 0;
            foreach(TileController tile in _tiles)
            {
                tile.InitializeTile(pieceController, index);
                index++;
            }
        }
        /// <summary>
        /// Sets the sprites of all tiles on this piece
        /// </summary>
        /// <param name="newSpr">New sprite to set for this tile</param>
        private void SetTileSprites(Sprite newSpr)
        {
            for(int i = 0; i < _tiles.Length; i++)
            {
                if(_tiles[i] == null)
                {
                    continue;
                }
                _tiles[i].gameObject.GetComponent<SpriteRenderer>().sprite = newSpr;
            }
        }
        // The ability that will spawn on the NEXT call to SpawnPiece. Set the moment the bar
        // fills (which also resets the bar visually); cleared when the ability piece actually
        // spawns. PieceSpawner owns this state so a double SpawnPiece cannot fire two abilities.
        private AbilityType? _bufferedAbility;

        public void SpawnPiece()
        {
            // 1) If an ability is buffered from a previous spawn, fire it now.
            if (_bufferedAbility.HasValue && _abilityPiecePrefabs != null)
            {
                AbilityType ability = _bufferedAbility.Value;
                _bufferedAbility = null;
                int idx = (int)ability;
                if (idx >= 0 && idx < _abilityPiecePrefabs.Length && _abilityPiecePrefabs[idx] != null)
                {
                    GameObject abilityPiece = Instantiate(_abilityPiecePrefabs[idx], transform);
                    InitializeCurPiece(abilityPiece);

                    AbilityMarker marker = abilityPiece.GetComponent<AbilityMarker>();
                    if (marker != null) marker.SetAbility(ability);

                    PieceController pc = abilityPiece.GetComponent<PieceController>();
                    if (PieceController.Tiles != null && PieceController.Tiles.Length > 0)
                    {
                        PieceController.Tiles[0].UpdatePosition(spawnPosition);
                        PieceController.Tiles[0].InitializeTile(pc, 0);
                    }

                    // _nextPieceType was overridden to the ability type for the preview during the
                    // arming spawn. Re-roll it so the spawn AFTER this one uses a normal tetromino;
                    // otherwise UpdateTiles would be called with Bomb/Laser (no switch case) and
                    // tiles 1..3 would stay at their default (0,0) coords, eventually triggering
                    // game-over from the broken layout.
                    _nextPieceType = (PieceType) Random.Range(0, 7);
                    OnNextPieceChanged?.Invoke(_nextPieceType);

                    OnPieceSpawned?.Invoke();
                    return;
                }
            }

            // 2) Normal tetromino spawn.
            GameObject curPiece = Instantiate(piecePrefab, transform);
            InitializeCurPiece(curPiece);

            _curPieceType = _nextPieceType;
            _nextPieceType = (PieceType) Random.Range(0, 7);

            // 3) If the bar is full and we don't already have a buffered ability, take the
            // pending ability now (which also empties the bar) and queue it for the next spawn.
            // The preview slot is overridden so the player sees what's coming.
            if (GameMode.IsExtended
                && AbilityCharger.Instance != null
                && AbilityCharger.Instance.IsReady
                && !_bufferedAbility.HasValue)
            {
                _bufferedAbility = AbilityCharger.Instance.ConsumePending();
                _nextPieceType = AbilityToPieceType(_bufferedAbility.Value);
            }

            OnNextPieceChanged?.Invoke(_nextPieceType);
            UpdateTiles(_curPieceType, PiecesController.CurPiece.GetComponent<PieceController>());
            OnPieceSpawned?.Invoke();
        }

        private static PieceType AbilityToPieceType(AbilityType ability)
        {
            switch (ability)
            {
                case AbilityType.Bomb: return PieceType.Bomb;
                case AbilityType.Laser: return PieceType.Laser;
                default: return PieceType.O;
            }
        }

#if UNITY_EDITOR
        /// <summary>Editor-only test hook: kills the active piece and immediately spawns the requested ability piece.</summary>
        public void DebugForceAbility(AbilityType ability)
        {
            if (PiecesController.CurPiece != null && PiecesController.Instance != null)
                PiecesController.Instance.DestroyCurPiece();
            _bufferedAbility = ability;
            SpawnPiece();
        }
#endif
        private void InitializeCurPiece(GameObject curPiece)
        {
            PiecesController.CurPiece = curPiece;
        }

        public void SpawnGhostPiece()
        {
            Instantiate(ghostPiecePrefab, transform);
        }

        // === Persistence hooks ===

        /// <summary>Upcoming piece type. Used by the save system to persist the preview state.</summary>
        public static PieceType NextPieceType => _nextPieceType;

        /// <summary>Tile sprite palette, exposed for the save system to resolve sprite indices.</summary>
        public Sprite[] TileSprites => tileSprites;

        /// <summary>
        /// Spawns a piece with explicit current/next types — used when restoring from a save file.
        /// Mirrors <see cref="SpawnPiece"/> but bypasses the random next-piece roll.
        /// </summary>
        public void SpawnPieceFromSave(PieceType currentType, PieceType nextType)
        {
            GameObject curPiece = Instantiate(piecePrefab, transform);
            InitializeCurPiece(curPiece);

            _curPieceType = currentType;
            _nextPieceType = nextType;

            OnNextPieceChanged?.Invoke(_nextPieceType);
            UpdateTiles(_curPieceType, PiecesController.CurPiece.GetComponent<PieceController>());
            OnPieceSpawned?.Invoke();
        }
    }
}