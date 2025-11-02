using System.Collections.Generic;
using UnityEngine;

namespace KMines
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-1000)]
    public class Board : MonoBehaviour
    {
        // -------------------------------------------------
        // Layout / data (det som Boot & LevelLoader förväntar sig)
        // -------------------------------------------------
        [Header("Layout")]
        public int width = 9;
        public int height = 15;

        // Boot + LevelLoader använder detta namnet
        [Tooltip("Storlek på 1 ruta i world units")]
        public float tileSize = 1.0f;

        [Header("Mines")]
        [Tooltip("0.16 = 16% av rutorna blir minor")]
        public float mineDensity = 0.16f;

        [Header("Visuals")]
        [Tooltip("Om tomma laddas från Resources/Art/…")]
        public Texture2D closedTex;
        public Texture2D openTex;

        [Tooltip("Root att lägga alla celler under. Om null används detta GameObject.")]
        public Transform cellRoot;

        [Header("Score")]
        public int scorePerSafe = 1000;
        public int scorePerNumber = 1500;

        [Header("Missiles")]
        [SerializeField] int startMissiles = 3;

        [Header("Themes (valfritt)")]
        public List<ThemeDef> themes = new List<ThemeDef>();
        string currentTheme = "grass";

        // runtime-speldata
        public Cell[,] grid;
        public bool[,] mines;
        public bool[,] flagged;
        public int[,] near;

        // missiles / energy
        int missiles;
        bool missileArmed;
        bool missilesEnabled = true;

        // timer-koppling
        [HideInInspector] public GameTimer gameTimer;
        [HideInInspector] public float bonusPerSafeReveal = 0f;

        // HUD-färger
        [HideInInspector] public Color panelColorForHUD = new Color(0.07f, 0.09f, 0.11f, 1f);
        [HideInInspector] public Color accentColorForHUD = new Color(0.1f, 0.6f, 0.9f, 1f);

        public int score = 0;

        bool firstClickDone = false;

        void Awake()
        {
            if (!cellRoot)
                cellRoot = this.transform;
        }

        void Start()
        {
            Build();
        }

        // -------------------------------------------------
        // BYGG BRÄDE
        // -------------------------------------------------
        public void Build()
        {
            // 1) laddning av standard-texturer
            if (closedTex == null)
                closedTex = Resources.Load<Texture2D>("Art/tile_closed");
            if (openTex == null)
                openTex = Resources.Load<Texture2D>("Art/tile_open");

            // 2) skapa arrayer
            grid = new Cell[width, height];
            mines = new bool[width, height];
            flagged = new bool[width, height];
            near = new int[width, height];

            // 3) rensa gamla celler (om vi t.ex. laddar om nivå)
            if (cellRoot != null)
            {
                for (int i = cellRoot.childCount - 1; i >= 0; i--)
                {
                    var ch = cellRoot.GetChild(i);
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        DestroyImmediate(ch.gameObject);
                    else
                        Destroy(ch.gameObject);
#else
                    Destroy(ch.gameObject);
#endif
                }
            }

            // 4) räkna ut hur många minor vi ska ha från density
            int mineCount = Mathf.RoundToInt(width * height * Mathf.Clamp01(mineDensity));
            PlaceMinesRandom(mineCount);

            // 5) räkna antal runt
            RecalcNear();

            // 6) skapa alla celler som rena GameObjects + AddComponent<Cell>()
            //    (ingen prefab!)
            float offX = -(width - 1) * 0.5f * tileSize;
            float offZ = -(height - 1) * 0.5f * tileSize;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // world-pos så att brädet hamnar centrerat
                    Vector3 pos = new Vector3(offX + x * tileSize, 0f, offZ + y * tileSize);

                    var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    go.name = $"Cell_{x}_{y}";
                    go.transform.SetParent(cellRoot, false);
                    go.transform.localPosition = pos;
                    go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // så den ligger plant
                    go.transform.localScale = Vector3.one; // Cell.Init fixar visuell scaling

                    // ta bort collider vi inte vill ha
                    var col = go.GetComponent<Collider>();
                    if (col) Destroy(col);

                    var cell = go.AddComponent<Cell>();
                    bool hasMine = mines[x, y];
                    int nearCount = near[x, y];

                    // init med våra texturer
                    cell.Init(this, x, y, hasMine, nearCount, tileSize, closedTex, openTex);

                    grid[x, y] = cell;
                }
            }

            // 7) missiles
            if (missilesEnabled)
                missiles = startMissiles;
            else
                missiles = 0;
            missileArmed = false;

            // 8) score reset
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

        // -------------------------------------------------
        // INPUT API (anropas från SmartClickInput)
        // -------------------------------------------------
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
            // vi vet att brädet är centrerat runt (0,0)
            float localX = (worldPos.x) / tileSize + (width - 1) * 0.5f;
            float localY = (worldPos.z) / tileSize + (height - 1) * 0.5f;

            int x = Mathf.RoundToInt(localX);
            int y = Mathf.RoundToInt(localY);

            if (x < 0 || y < 0 || x >= width || y >= height)
                return null;

            return grid[x, y];
        }

        public void ClickCell(Cell cell)
        {
            if (cell == null) return;
            int x = cell.x;
            int y = cell.y;

            if (cell.opened) return;

            // första klicket ska vara säkert
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
            Queue<Vector2Int> q = new Queue<Vector2Int>();
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

                // score
                score += (near[x, y] > 0) ? scorePerNumber : scorePerSafe;

                // ev. tidbonus
                if (bonusPerSafeReveal > 0f && gameTimer != null && gameTimer.IsActive())
                    gameTimer.AddTime(bonusPerSafeReveal);

                // flood bara på 0-rutor
                if (near[x, y] == 0)
                {
                    q.Enqueue(new Vector2Int(x - 1, y));
                    q.Enqueue(new Vector2Int(x + 1, y));
                    q.Enqueue(new Vector2Int(x, y - 1));
                    q.Enqueue(new Vector2Int(x, y + 1));
                }
            }
        }

        // -------------------------------------------------
        // MISSILE-API (anropas från HUD/MissileUI)
        // -------------------------------------------------
        public int MissileCount() => missiles;
        public bool IsMissileArmed() => missileArmed && missiles > 0;

        public void SetMissilesEnabled(bool enabled)
        {
            missilesEnabled = enabled;
        }

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

            // grace
            var wlm = FindObjectOfType<WinLoseManager>();
            if (wlm != null) wlm.BeginMissileGrace(0.35f);

            missiles = Mathf.Max(0, missiles - 1);

            int cx = center.x;
            int cy = center.y;

            // ta bort minor 3x3
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
                        {
                            MissileHitFX.Spawn(c.transform.position);
                        }
                    }
                }
            }

            // recalc runt 5x5
            for (int y = cy - 2; y <= cy + 2; y++)
                for (int x = cx - 2; x <= cx + 2; x++)
                    if (!(x < 0 || y < 0 || x >= width || y >= height))
                        near[x, y] = CountNear8(x, y);

            // öppna 3x3
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

        // -------------------------------------------------
        // Tema (Boot anropar detta)
        // -------------------------------------------------
        public void SetTheme(string themeId)
        {
            if (string.IsNullOrEmpty(themeId))
                themeId = "grass";

            currentTheme = themeId;

            // hämta färger från ThemeLibrary så HUD får rätt färg
            var tl = ThemeLibrary.GetTheme(themeId);
            panelColorForHUD = tl.backplateColor;
            accentColorForHUD = new Color(1f, 0.6f, 0.2f, 1f); // kan tweakas

            // om vi vill byta sprite/texturer här kan vi göra det senare
        }

        // -------------------------------------------------
        // Hjälpare för WinLoseManager
        // -------------------------------------------------
        public int CountCorrectFlags()
        {
            int correct = 0;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    if (flagged[x, y] && mines[x, y])
                        correct++;
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
