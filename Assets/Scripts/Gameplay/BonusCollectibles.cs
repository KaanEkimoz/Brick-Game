using System.Collections;
using Board;
using Extras;
using InGame;
using Piece;
using Tiles;
using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Extended mode: a fruit appears on the low part of the stack for a few seconds. Landing a
    /// piece on its cell collects it, and it flies to the score, which pays the bonus on arrival.
    /// Classic and Levels stay out, because their scores are checked against lines and targets.
    /// </summary>
    public class BonusCollectibles : MonoBehaviour
    {
        private const float MinSpawnDelay = 16f;
        private const float MaxSpawnDelay = 28f;
        private const float MinLifetime = 5f;
        private const float MaxLifetime = 8f;
        private const float WarningSeconds = 1.5f;
        private const float BlinkSpeed = 10f;
        private const float BobHeight = 0.08f;
        private const float BobSpeed = 4f;
        private const float PopSeconds = 0.2f;
        private const float FlightSeconds = 0.65f;
        private const float FlightEndScale = 0.45f;
        private const float DisplaySize = 0.9f;
        private const int HighestRow = 7;
        private const int SortingOrder = 9; // one above the target preview

        public static System.Action OnFruitCollected;

        private static readonly string[] SpritePaths = { "Collectibles/cherry", "Collectibles/strawberry", "Collectibles/orange" };
        private static readonly int[] BasePoints = { 50, 75, 100 };

        private ScoreController _score;
        private SpriteRenderer _fruit;
        private SpriteRenderer _flying;
        private Vector2Int _cell;
        private int _points;
        private float _age;
        private float _lifetime;
        private float _spawnTimer;

        private void Awake()
        {
            _score = FindAnyObjectByType<ScoreController>(FindObjectsInactive.Include);
            _spawnTimer = Random.Range(MinSpawnDelay, MaxSpawnDelay);
        }

        private void OnEnable()
        {
            PieceMovement.OnPieceSettled += HandlePieceSettled;
            PiecesController.OnGameOver += Clear;
            BoardController.OnBoardCleared += Clear;
        }

        private void OnDisable()
        {
            PieceMovement.OnPieceSettled -= HandlePieceSettled;
            PiecesController.OnGameOver -= Clear;
            BoardController.OnBoardCleared -= Clear;
        }

        private void Update()
        {
            if (!GameMode.IsExtended || GameMode.IsLevels) return;
            if (PiecesController.Instance == null || !PiecesController.Instance.IsRunActive) return;

            if (_fruit != null) UpdateFruit();
            else if ((_spawnTimer -= Time.deltaTime) <= 0f) TrySpawn();
        }

        private void UpdateFruit()
        {
            _age += Time.deltaTime;
            bool covered = !BoardController.Instance.IsPosEmpty(_cell);
            if (_age >= _lifetime || covered)
            {
                RemoveFruit();
                return;
            }

            _fruit.transform.position = BoardController.CellToWorld(_cell) + Vector3.up * (Mathf.Sin(_age * BobSpeed) * BobHeight);
            _fruit.transform.localScale = Vector3.one * (DisplaySize * Mathf.Clamp01(_age / PopSeconds));
            bool warning = _lifetime - _age < WarningSeconds;
            _fruit.enabled = !warning || Mathf.Sin(_age * BlinkSpeed) > -0.3f;
        }

        // A random column whose surface is low enough. No room just tries again next frame.
        private void TrySpawn()
        {
            BoardController board = BoardController.Instance;
            int column = Random.Range(0, board.gridSizeX);
            int row = HighestRow;
            while (row > 0 && board.IsPosEmpty(new Vector2Int(column, row - 1))) row--;
            if (!board.IsPosEmpty(new Vector2Int(column, row))) return;

            int kind = Random.Range(0, SpritePaths.Length);
            _cell = new Vector2Int(column, row);
            _points = BasePoints[kind] * (LevelController.CurrentLevel + 1);
            _age = 0f;
            _lifetime = Random.Range(MinLifetime, MaxLifetime);
            _fruit = CreateFruit(kind);
        }

        private static SpriteRenderer CreateFruit(int kind)
        {
            Texture2D texture = Resources.Load<Texture2D>(SpritePaths[kind]);
            GameObject go = new GameObject("BonusFruit");
            SpriteRenderer fruit = go.AddComponent<SpriteRenderer>();
            fruit.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.width);

            SpriteRenderer tile = PieceController.Tiles[0].GetComponent<SpriteRenderer>();
            fruit.sharedMaterial = tile.sharedMaterial;
            fruit.sortingLayerID = tile.sortingLayerID;
            fruit.sortingOrder = SortingOrder;
            return fruit;
        }

        private void HandlePieceSettled()
        {
            if (_fruit == null) return;

            foreach (TileController tile in PieceController.Tiles)
            {
                if (tile.coordinates != _cell) continue;
                if (_flying != null) Destroy(_flying.gameObject);
                _flying = _fruit;
                _fruit = null;
                ResetSpawnTimer();
                // Paid on landing: the flight stops if the next piece ends the run.
                _score.AddBonusScore(_points);
                OnFruitCollected?.Invoke();
                StartCoroutine(FlyToScore(_flying));
                return;
            }
        }

        private IEnumerator FlyToScore(SpriteRenderer fruit)
        {
            fruit.enabled = true;
            Vector3 from = fruit.transform.position;
            Vector3 to = ScoreWorldPosition();
            float baseScale = fruit.transform.localScale.x;

            for (float t = 0f; t < FlightSeconds; t += Time.deltaTime)
            {
                float n = t / FlightSeconds;
                fruit.transform.position = Vector3.Lerp(from, to, n * n);
                fruit.transform.localScale = Vector3.one * (baseScale * Mathf.Lerp(1f, FlightEndScale, n));
                yield return null;
            }

            Destroy(fruit.gameObject);
        }

        private Vector3 ScoreWorldPosition()
        {
            Canvas canvas = _score.scoreText.canvas;
            Camera canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(canvasCamera, _score.scoreText.rectTransform.position);
            Camera camera = Camera.main;
            Vector3 world = camera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -camera.transform.position.z));
            world.z = 0f;
            return world;
        }

        private void RemoveFruit()
        {
            Destroy(_fruit.gameObject);
            _fruit = null;
            ResetSpawnTimer();
        }

        private void ResetSpawnTimer()
        {
            _spawnTimer = Random.Range(MinSpawnDelay, MaxSpawnDelay);
        }

        private void Clear()
        {
            StopAllCoroutines();
            if (_fruit != null) Destroy(_fruit.gameObject);
            if (_flying != null) Destroy(_flying.gameObject);
            _fruit = null;
            ResetSpawnTimer();
        }
    }
}
