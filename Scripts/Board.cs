using System.Collections.Generic;
using UnityEngine;

namespace KMines
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-1000)]
    public class Board : MonoBehaviour
    {
        [Header("Layout")]
        public int width = 9;
        public int height = 15;
        public float tileSize = 1.0f;
        public float verticalGridOffset = 0.5f;

        [Header("Mines")]
        public float mineDensity = 0.16f;

        [Header("Visuals")]
        public Texture2D closedTex;
        public Texture2D openTex;
        public Transform cellRoot;

        [Header("Score")]
        public int scorePerSafe = 1000;
        public int scorePerNumber = 1500;

        [Header("Missiles")]
        [SerializeField] int startMissiles = 3;

        [Header("Themes (valfritt)")]
        public List<ThemeDef> themes = new List<ThemeDef>();
        string currentTheme = "grass";

        public Cell[,] grid;
        public bool[,] mines;
        public bool[,] flagged;
        public int[,] near;

        int missiles;
        bool missileArmed;
        bool missilesEnabled = true;

        [HideInInspector] public GameTimer gameTimer;
        [HideInInspector] public float bonusPerSafeReveal = 0f;

        [HideInInspector] public Color panelColorForHUD = new Color(0.07f, 0.09f, 0.11f, 1f);
        [HideInInspector] public Color accentColorForHUD = new Color(0.1f, 0.6f, 0.9f, 1f);

        public int score = 0;
        bool firstClickDone = false;

        void Awake()
        {
            if (!cellRoot) cellRoot = this.transform;
        }

        void Start()
        {
            Build();
        }

        public void Build()
        {
            if (closedTex == null) closedTex = Resources.Load<Texture2D>("Art/tile_closed");
            if (openTex == null) openTex = Resources.Load<Texture2D>("Art/tile_open");

            grid = new Cell[width, height];
            mines = new bool[width, height];
            flagged = new bool[width, height];
            near = new int[width, height];

            if (cellRoot != null)
            {
                for (int i = cellRoot.childCount - 1; i >= 0; i--)
                {
                    var ch = cellRoot.GetChild(i);
#if UNITY_EDITOR
                    if (!Application.isPlaying) DestroyImmediate(ch.gameObject);
                    else Destroy(ch.gameObject);
#else
                    Destroy(ch.gameObject);
#endif
                }
            }

            int mineCount = Mathf.RoundToInt(width * height * Mathf.Clamp01(mineDensity));
            PlaceMinesRandom(mineCount);
            RecalcNear();

            float offX = -(width - 1) * 0.5f * tileSize;
            float offZ = -(height - 1) * 0.5f * tileSize;
            float worldOffsetZ = verticalGridOffset * tileSize;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector3 pos = new Vector3(
                        offX + x * tileSize,
                        0f,
                        offZ + y * tileSize - worldOffsetZ
                    );

                    var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    go.name = $"Cell_{x}_{y}";
                    go.transform.SetParent(cellRoot, false);
                    go.transform.localPosition = pos;
                    go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

                    // VISUELL skala – bara lite mindre på RIKTIG mobil
                    float visualScale = tileSize;
                    if (Application.isMobilePlatform)
                        visualScale = tileSize * 0.95f;

                    go.transform.localScale = new Vector3(visualScale, visualScale, 1f);

                    var col = go.GetComponent<Collider>();
                    if (col) Destroy(col);

                    var cell = go.AddComponent<Cell>();
                    bool hasMine = mines[x, y];
                    int nearCount = near[x, y];
                    cell.Init(this, x, y, hasMine, nearCount, tileSize, closedTex, openTex);
                    grid[x, y] = cell;
                }
            }

            if (missilesEnabled) missiles = startMissiles;
            else missiles = 0;
            missileArmed = false;

            score = 0;
            firstClickDone = false;
        }

        void PlaceMinesRandom(int count)
        {
            int placed = 0;
            int max = width * height;
            System.Random rng = new System.Random();
            while (placed < count && placed < max)
            {
                int x = rng.Next(0, width);
                int y = rng.Next(0, height);
                if (mines[x, y]) continue;
                mines[x, y] = true;
                placed++;
            }
        }

        void RecalcNear()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    near[x, y] = CountNear8(x, y);
                }
            }
        }

        int CountNear8(int cx, int cy)
        {
            int cnt = 0;
            for (int y = cy - 1; y <= cy + 1; y++)
            {
                for (int x = cx - 1; x <= cx + 1; x++)
                {
                    if (x == cx && y == cy) continue;
                    if (x < 0 || y < 0 || x >= width || y >= height) continue;
                    if (mines[x, y]) cnt++;
                }
            }
            return cnt;
        }

        public void ClickAt(Vector3 worldPos)
        {
            var cell = WorldToCell(worldPos);
            ClickCell(cell);
        }

        public void ToggleFlagAt(Vector3 worldPos)
        {
            var cell = WorldToCell(worldPos);
            if (cell == null) return;
            int x = cell.x;
            int y = cell.y;
            bool now = !flagged[x, y];
            flagged[x, y] = now;
            cell.SetFlag(now);
        }

        public Cell WorldToCell(Vector3 worldPos)
        {
            Vector3 local = worldPos - transform.position;
            float correctedZ = local.z + (verticalGridOffset * tileSize);

            float localX = (local.x / tileSize) + (width - 1) * 0.5f;
            float localY = (correctedZ / tileSize) + (height - 1) * 0.5f;

            int x = Mathf.Clamp(Mathf.RoundToInt(localX), 0, width - 1);
            int y = Mathf.Clamp(Mathf.RoundToInt(localY), 0, height - 1);
            return grid[x, y];
        }

        public void ClickCell(Cell cell)
        {
            if (cell == null) return;
            int x = cell.x;
            int y = cell.y;
            if (cell.opened) return;

            if (!firstClickDone)
            {
                firstClickDone = true;
                if (mines[x, y])
                {
                    if (TryRelocateMine(x, y))
                    {
                        RecalcNear();
                        RevealFrom4(x, y);
                        return;
                    }
                }
            }

            if (mines[x, y])
            {
                cell.Open(true);
                return;
            }

            RevealFrom4(x, y);
        }

        bool TryRelocateMine(int fromX, int fromY)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x == fromX && y == fromY) continue;
                    if (mines[x, y]) continue;
                    mines[x, y] = true;
                    mines[fromX, fromY] = false;
                    return true;
                }
            }
            return false;
        }

        void RevealFrom4(int sx, int sy)
        {
            var q = new Queue<Vector2Int>();
            q.Enqueue(new Vector2Int(sx, sy));

            while (q.Count > 0)
            {
                var v = q.Dequeue();
                int x = v.x;
                int y = v.y;
                if (x < 0 || y < 0 || x >= width || y >= height) continue;

                var c = grid[x, y];
                if (c == null) continue;
                if (c.opened) continue;
                if (flagged[x, y]) continue;
                if (mines[x, y]) continue;

                c.near = near[x, y];
                c.Open(false);

                score += (near[x, y] > 0) ? scorePerNumber : scorePerSafe;

                if (bonusPerSafeReveal > 0f && gameTimer != null && gameTimer.IsActive())
                    gameTimer.AddTime(bonusPerSafeReveal);

                if (near[x, y] == 0)
                {
                    q.Enqueue(new Vector2Int(x - 1, y));
                    q.Enqueue(new Vector2Int(x + 1, y));
                    q.Enqueue(new Vector2Int(x, y - 1));
                    q.Enqueue(new Vector2Int(x, y + 1));
                }
            }
        }

        public int MissileCount() => missiles;
        public bool IsMissileArmed() => missileArmed && missiles > 0;

        public void SetMissilesEnabled(bool enabled) => missilesEnabled = enabled;

        public void ArmMissile()
        {
            if (!missilesEnabled) return;
            if (missiles <= 0) return;
            missileArmed = true;
        }

        public void UseMissileAt(Vector3 worldPoint)
        {
            var cell = WorldToCell(worldPoint);
            UseMissileAtCell(cell);
        }

        public void UseMissileAtCell(Cell center)
        {
            if (center == null) return;
            if (!IsMissileArmed()) return;

            var wlm = FindObjectOfType<WinLoseManager>();
            if (wlm != null) wlm.BeginMissileGrace(0.35f);

            missiles = Mathf.Max(0, missiles - 1);
            int cx = center.x;
            int cy = center.y;

            for (int y = cy - 1; y <= cy + 1; y++)
            {
                for (int x = cx - 1; x <= cx + 1; x++)
                {
                    if (x < 0 || y < 0 || x >= width || y >= height) continue;
                    bool wasMine = mines[x, y];
                    mines[x, y] = false;
                    var c = grid[x, y];
                    if (c != null)
                    {
                        c.hasMine = false;
                        if (wasMine)
                            MissileHitFX.Spawn(c.transform.position);
                    }
                }
            }

            for (int y = cy - 2; y <= cy + 2; y++)
            {
                for (int x = cx - 2; x <= cx + 2; x++)
                {
                    if (x < 0 || y < 0 || x >= width || y >= height) continue;
                    near[x, y] = CountNear8(x, y);
                }
            }

            for (int y = cy - 1; y <= cy + 1; y++)
            {
                for (int x = cx - 1; x <= cx + 1; x++)
                {
                    if (x < 0 || y < 0 || x >= width || y >= height) continue;
                    flagged[x, y] = false;
                    var c = grid[x, y];
                    if (c == null) continue;
                    c.near = near[x, y];
                    c.Open(false);

                    int gained = (near[x, y] > 0) ? 1500 : 1000;
                    score += gained;
                    ScorePopupFX.Spawn(c.transform.position, gained);
                }
            }

            missileArmed = false;
        }

        public void SetTheme(string themeId)
        {
            if (string.IsNullOrEmpty(themeId)) themeId = "grass";
            currentTheme = themeId;

            var tl = ThemeLibrary.GetTheme(themeId);
            panelColorForHUD = tl.backplateColor;
            accentColorForHUD = new Color(1f, 0.6f, 0.2f, 1f);
        }

        public int CountCorrectFlags()
        {
            int correct = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (flagged[x, y] && mines[x, y])
                        correct++;
                }
            }
            return correct;
        }

        public void AddScore(int amount)
        {
            score += amount;
        }

        [System.Serializable]
        public class ThemeDef
        {
            public string id;
            public Sprite boardSprite;
            public Color panelColor = new Color(0.07f, 0.09f, 0.11f, 1f);
            public Color accentColor = new Color(0.1f, 0.6f, 0.9f, 1f);
        }
    }
}
