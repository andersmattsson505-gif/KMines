using UnityEngine;
using System.Collections.Generic;

namespace KMines
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-1000)]
    public class Board : MonoBehaviour
    {
        // används av KMinesBoardMenu.cs i Editor
        public bool autoBuildInEditor = true;

        [Header("Size")]
        public int width = 9;
        public int height = 15;
        public float tileSize = 1.0f;

        [Header("Mines")]
        public float mineDensity = 0.16f;
        public bool firstClickIsSafe = true;

        [Header("Missiles")]
        [SerializeField] int startMissiles = 3;
        bool missilesEnabled = true;
        int missiles = 0;
        bool missileArmed = false;

        [Header("Theme")]
        [SerializeField] string currentThemeId = "grass";

        public Cell[,] grid;
        public bool[,] mines;
        public bool[,] flags;
        public int[,] near;

        public GameTimer gameTimer;
        public float bonusPerSafeReveal = 0f;
        public int score = 0;

        [HideInInspector] public Color panelColorForHUD = new Color(0.07f, 0.09f, 0.11f, 1f);
        [HideInInspector] public Color accentColorForHUD = new Color(0.10f, 0.60f, 0.90f, 1f);

        bool firstClickDone = false;

        void Awake()
        {
            if (string.IsNullOrEmpty(currentThemeId))
                currentThemeId = "grass";
        }

        void Start()
        {
            Build();
        }

        public void SetTheme(string themeId)
        {
            if (string.IsNullOrEmpty(themeId))
                themeId = "grass";
            currentThemeId = themeId;
        }

        public void SetMissilesEnabled(bool enabled)
        {
            missilesEnabled = enabled;
        }

        public int MissileCount() => missiles;

        public bool IsMissileArmed() => missileArmed && missiles > 0 && missilesEnabled;

        public void ArmMissile() => ArmMissile(true);

        public void ArmMissile(bool armed)
        {
            missileArmed = armed && missilesEnabled && missiles > 0;
        }

        public void AddScore(int amount)
        {
            score += amount;
        }

        public int CountCorrectFlags()
        {
            if (flags == null || mines == null)
                return 0;

            int correct = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (flags[x, y] && mines[x, y])
                        correct++;
                }
            }
            return correct;
        }

        public void Build()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(transform.GetChild(i).gameObject);
                else
                    Destroy(transform.GetChild(i).gameObject);
#else
                Destroy(transform.GetChild(i).gameObject);
#endif
            }

            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);
            tileSize = Mathf.Max(0.01f, tileSize);
            mineDensity = Mathf.Clamp01(mineDensity);

            grid = new Cell[width, height];
            mines = new bool[width, height];
            flags = new bool[width, height];
            near = new int[width, height];
            score = 0;
            firstClickDone = false;

            int wantedMines = Mathf.RoundToInt(width * height * mineDensity);
            wantedMines = Mathf.Clamp(wantedMines, 0, width * height - 1);

            System.Random rng = new System.Random();
            int placed = 0;
            while (placed < wantedMines)
            {
                int rx = rng.Next(0, width);
                int ry = rng.Next(0, height);
                if (mines[rx, ry]) continue;
                mines[rx, ry] = true;
                placed++;
            }

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    near[x, y] = CountNear8(x, y);

            BoardTheme theme = ThemeLibrary.GetTheme(currentThemeId);
            Texture2D texClosed = !string.IsNullOrEmpty(theme.closedTexPath)
                ? Resources.Load<Texture2D>(theme.closedTexPath)
                : null;
            Texture2D texOpen = !string.IsNullOrEmpty(theme.openTexPath)
                ? Resources.Load<Texture2D>(theme.openTexPath)
                : null;

            panelColorForHUD = theme.backplateColor;
            accentColorForHUD = Color.Lerp(theme.backplateColor, new Color(0.1f, 0.6f, 0.9f, 1f), 0.35f);

            float startX = -(width - 1) * tileSize * 0.5f;
            float startZ = -(height - 1) * tileSize * 0.5f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    GameObject go = new GameObject($"Cell_{x}_{y}");
                    go.transform.SetParent(this.transform, false);
                    go.transform.localPosition = new Vector3(startX + x * tileSize, 0f, startZ + y * tileSize);
                    go.transform.localRotation = Quaternion.identity;

                    var cell = go.AddComponent<Cell>();
                    bool hasMine = mines[x, y];
                    int nearCount = near[x, y];

                    cell.Init(this, x, y, hasMine, nearCount, tileSize, texClosed, texOpen);

                    grid[x, y] = cell;
                }
            }

            missiles = missilesEnabled ? startMissiles : 0;
            missileArmed = false;
        }

        public void ClickAt(Vector3 worldPos)
        {
            Cell c = WorldToCell(worldPos);
            if (c == null) return;
            ClickCell(c.x, c.y);
        }

        public void ToggleFlagAt(Vector3 worldPos)
        {
            Cell c = WorldToCell(worldPos);
            if (c == null) return;

            int x = c.x;
            int y = c.y;

            flags[x, y] = !flags[x, y];
        }

        public void UseMissileAt(Vector3 worldPos)
        {
            if (!IsMissileArmed()) return;

            Cell c = WorldToCell(worldPos);
            if (c == null) return;

            var wlm = FindObjectOfType<WinLoseManager>();
            if (wlm != null)
                wlm.BeginMissileGrace(0.35f);

            missiles = Mathf.Max(0, missiles - 1);

            int cx = c.x;
            int cy = c.y;

            for (int y = cy - 1; y <= cy + 1; y++)
            {
                for (int x = cx - 1; x <= cx + 1; x++)
                {
                    if (x < 0 || y < 0 || x >= width || y >= height) continue;

                    mines[x, y] = false;
                    near[x, y] = CountNear8(x, y);

                    var cell = grid[x, y];
                    if (cell != null && !cell.opened)
                    {
                        cell.Open(false);
                        score += (near[x, y] > 0) ? 1500 : 1000;

                        if (bonusPerSafeReveal > 0f && gameTimer != null && gameTimer.IsActive())
                            gameTimer.AddTime(bonusPerSafeReveal);
                    }

                    flags[x, y] = false;
                }
            }

            for (int y = cy - 2; y <= cy + 2; y++)
                for (int x = cx - 2; x <= cx + 2; x++)
                    if (x >= 0 && y >= 0 && x < width && y < height)
                        near[x, y] = CountNear8(x, y);

            missileArmed = false;
        }

        void ClickCell(int x, int y)
        {
            if (x < 0 || y < 0 || x >= width || y >= height) return;

            var cell = grid[x, y];
            if (cell == null) return;
            if (cell.opened) return;
            if (flags[x, y]) return;

            if (firstClickIsSafe && !firstClickDone)
            {
                firstClickDone = true;
                if (mines[x, y])
                {
                    if (TryRelocateMine(x, y))
                    {
                        for (int yy = 0; yy < height; yy++)
                            for (int xx = 0; xx < width; xx++)
                                near[xx, yy] = CountNear8(xx, yy);
                        FloodReveal(x, y);
                        return;
                    }
                }
            }

            if (mines[x, y])
            {
                cell.hasMine = true;
                cell.Open(true);
            }
            else
            {
                FloodReveal(x, y);
            }
        }

        void FloodReveal(int sx, int sy)
        {
            Queue<Vector2Int> q = new Queue<Vector2Int>();
            q.Enqueue(new Vector2Int(sx, sy));

            while (q.Count > 0)
            {
                var v = q.Dequeue();
                int x = v.x;
                int y = v.y;

                if (x < 0 || y < 0 || x >= width || y >= height) continue;
                var cell = grid[x, y];
                if (cell == null) continue;
                if (cell.opened) continue;
                if (flags[x, y]) continue;

                cell.Open(false);

                int gained = (near[x, y] > 0) ? 1500 : 1000;
                score += gained;

                if (near[x, y] == 0)
                {
                    q.Enqueue(new Vector2Int(x - 1, y));
                    q.Enqueue(new Vector2Int(x + 1, y));
                    q.Enqueue(new Vector2Int(x, y - 1));
                    q.Enqueue(new Vector2Int(x, y + 1));
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

        public Cell WorldToCell(Vector3 worldPos)
        {
            float startX = -(width - 1) * tileSize * 0.5f;
            float startZ = -(height - 1) * tileSize * 0.5f;

            float localX = (worldPos.x - (transform.position.x + startX)) / tileSize;
            float localY = (worldPos.z - (transform.position.z + startZ)) / tileSize;

            int x = Mathf.RoundToInt(localX);
            int y = Mathf.RoundToInt(localY);

            if (x < 0 || y < 0 || x >= width || y >= height)
                return null;

            return grid[x, y];
        }
    }
}
