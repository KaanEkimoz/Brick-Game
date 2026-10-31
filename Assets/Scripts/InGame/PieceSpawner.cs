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
        // Single-spawner game (matches PieceBag's static design). Restart paths use
        // this to re-seed the preview slot, and the save system reads the current
        // piece type and charges the restored piece through it.
        public static PieceSpawner Instance { get; private set; }

        [Space]
        [Header("Prefabs")]
        public GameObject ghostPiecePrefab;
        public GameObject piecePrefab;

        [Tooltip("Ability art sources, ordered by AbilityType (0=Bomb, 1=Laser). The first SpriteRenderer of each prefab skins the charged tile. Abilities without an entry (Patch) use a tint instead.")]
        [SerializeField] private GameObject[] _abilityPiecePrefabs;

        [Space]
        public Vector2Int spawnPosition;

        [Space]
        public Sprite[] tileSprites;

        public static Action OnPieceSpawned;
        public static Action<PieceType> OnNextPieceChanged;
        /// <summary>Fires when an ability is loaded onto the falling piece. The target preview refreshes here.</summary>
        public static Action OnPieceCharged;

        private static readonly Color PatchTileTint = new Color(0.45f, 1f, 0.55f, 1f);
    
        private PieceType _curPieceType;
        private static PieceType _nextPieceType;
        private TileController[] _tiles;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // Seed the preview slot from the 7-bag. SaveManager.RestoreFromData calls
            // SpawnPieceFromSave AFTER this, replacing the preview with the saved value
            // and (in the restore path) replaying the saved bag state — so this initial
            // draw is only used on truly fresh runs.
            _nextPieceType = PieceBag.Next();
        }

        /// <summary>Re-seeds _nextPieceType from the (just-reset) bag on the in-place Restart
        /// path. Called from PiecesController.DestroyCurPiece.</summary>
        public void ResetBufferedAbility()
        {
            _nextPieceType = PieceBag.Next();
            OnNextPieceChanged?.Invoke(_nextPieceType);
        }

        /// <summary>
        /// Moves the attached tiles to form the Tetris piece specified. Also sets the correct color of tile sprite.
        /// </summary>
        /// <param name="newType">Type of tetris piece to be spawned.</param>
        private void UpdateTiles(PieceType newType, PieceController pieceController)
        {
            PieceType curType = newType;
            pieceController.GetComponent<PieceRotation>().curType = curType;
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
        public void SpawnPiece()
        {
            GameObject curPiece = Instantiate(piecePrefab, transform);
            InitializeCurPiece(curPiece);

            bool abilityDue = GameMode.IsExtended
                && AbilityCharger.Instance != null
                && AbilityCharger.Instance.IsReady;

            // A bomb or dynamite arrives on its own, so the previewed piece stays next in line.
            if (!abilityDue || !ArrivesAlone(AbilityCharger.Instance.Pending.Value))
            {
                _curPieceType = _nextPieceType;
                _nextPieceType = PieceBag.Next();
            }

            OnNextPieceChanged?.Invoke(_nextPieceType);
            UpdateTiles(_curPieceType, PiecesController.CurPiece.GetComponent<PieceController>());

            // Full bar: load the ability onto one random tile of this tetromino, so the player
            // aims it through placement and rotation. A bomb shrinks the piece to that one tile.
            if (abilityDue)
            {
                ChargeCurrentPiece(AbilityCharger.Instance.ConsumePending(),
                    Random.Range(0, PieceController.Tiles.Length));
            }

            OnPieceSpawned?.Invoke();
        }

        /// <summary>Loads an ability onto one tile of the falling piece. Used by the spawn path
        /// and by the save system when a charged piece is restored.</summary>
        public void ChargeCurrentPiece(AbilityType ability, int tileIndex)
        {
            GameObject piece = PiecesController.CurPiece;
            if (ability == AbilityType.Dynamite)
            {
                ChargeDynamite(piece);
                return;
            }
            if (ability == AbilityType.Bomb)
            {
                KeepFirstTiles(piece.GetComponent<PieceController>(), 1);
                tileIndex = 0;
            }

            AbilityMarker marker = piece.GetComponent<AbilityMarker>();
            if (marker == null) marker = piece.AddComponent<AbilityMarker>();
            marker.SetAbility(ability);
            marker.SetTileIndex(tileIndex);

            SpriteRenderer tileRenderer = PieceController.Tiles[tileIndex].GetComponent<SpriteRenderer>();
            int artIndex = (int)ability;
            bool hasArt = _abilityPiecePrefabs != null
                && artIndex < _abilityPiecePrefabs.Length
                && _abilityPiecePrefabs[artIndex] != null;
            if (hasArt)
                tileRenderer.sprite = _abilityPiecePrefabs[artIndex].GetComponentInChildren<SpriteRenderer>(true).sprite;
            else
                tileRenderer.color = PatchTileTint;

            if (ability == AbilityType.Laser)
                piece.AddComponent<LaserOrientation>().BindVisual(tileRenderer);

            OnPieceCharged?.Invoke();
        }

        private static bool ArrivesAlone(AbilityType ability) => ability == AbilityType.Bomb || ability == AbilityType.Dynamite;

        /// <summary>Shrinks the falling piece to its first tiles.</summary>
        private static void KeepFirstTiles(PieceController pieceController, int count)
        {
            TileController[] tiles = PieceController.Tiles;
            for (int i = count; i < tiles.Length; i++)
                Destroy(tiles[i].gameObject);

            Array.Resize(ref tiles, count);
            pieceController.tiles = tiles;
            PieceController.Tiles = tiles;
        }

        // Standing 1x2 piece: the first tile is the bottom half, the second the top half.
        private void ChargeDynamite(GameObject piece)
        {
            KeepFirstTiles(piece.GetComponent<PieceController>(), 2);
            TileController[] tiles = PieceController.Tiles;
            tiles[1].UpdatePosition(tiles[0].coordinates + Vector2Int.up);
            piece.GetComponent<PieceRotation>().curType = PieceType.Bomb;

            float tileWidth = tiles[0].GetComponent<SpriteRenderer>().bounds.size.x;
            tiles[0].GetComponent<SpriteRenderer>().sprite = DynamiteArt.Half(false, tileWidth);
            tiles[1].GetComponent<SpriteRenderer>().sprite = DynamiteArt.Half(true, tileWidth);

            AbilityMarker marker = piece.AddComponent<AbilityMarker>();
            marker.SetAbility(AbilityType.Dynamite);
            piece.AddComponent<DynamiteOrientation>();
            OnPieceCharged?.Invoke();
        }

#if UNITY_EDITOR
        /// <summary>Editor-only test hook: replaces the active piece with one carrying the requested ability.</summary>
        public void DebugForceAbility(AbilityType ability)
        {
            if (PiecesController.CurPiece != null && PiecesController.Instance != null)
                PiecesController.Instance.DestroyCurPiece();
            SpawnPiece();
            ChargeCurrentPiece(ability, Random.Range(0, PieceController.Tiles.Length));
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

        /// <summary>Type of the falling piece, read by the save system.</summary>
        public PieceType CurrentPieceType => _curPieceType;

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