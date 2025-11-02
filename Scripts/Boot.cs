using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Reflection;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace KMines
{
    [DefaultExecutionOrder(-5000)]
    public class Boot : MonoBehaviour
    {
        [Header("Fallback level defaults")]
        public int defaultWidth = 9;
        public int defaultHeight = 15;
        public float defaultTileSize = 1.0f;
        public float defaultMineDensity = 0.16f;
        public bool defaultTimed = false;
        public float defaultTimeLimitSeconds = 60f;

        [Header("Arcade defaults")]
        public int arcadeWidth = 9;
        public int arcadeHeight = 15;
        public float arcadeMineDensity = 0.20f;

        [Header("Boss defaults")]
        public int bossWidth = 9;
        public int bossHeight = 14;
        public float bossMineDensity = 0.22f;
        public float bossTimeLimitSeconds = 30f;

        void Start()
        {
            // --- CAMERA ---
            var cam = Camera.main;
            if (!cam)
            {
                var camGO = new GameObject("Main Camera");
                cam = camGO.AddComponent<Camera>();
                cam.tag = "MainCamera";
            }

            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.07f, 0.09f, 1f);
            cam.transform.position = new Vector3(0f, 10f, 0f);
            cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 200f;

            // --- EVENT SYSTEM ---
            if (!FindObjectOfType<EventSystem>())
            {
                var esGO = new GameObject("EventSystem");
                esGO.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
                esGO.AddComponent<InputSystemUIInputModule>();
#else
                esGO.AddComponent<StandaloneInputModule>();
#endif
            }

            // --- BOARD ---
            var boardGO = new GameObject("Board");
            // lite ner så vi får plats med top-bar
            boardGO.transform.position = new Vector3(0f, 0f, -0.8f);
            var board = boardGO.AddComponent<Board>();

            // --- GAME UI (win/lose) ---
            var uiGO = new GameObject("GameUI");
            var gameUI = uiGO.AddComponent<GameUI>();
            gameUI.SetBoard(board);

            // --- LEVEL LOADER ---
            var loaderObj = new GameObject("LevelLoader");
            var loader = loaderObj.AddComponent<LevelLoader>();
            loader.board = board;

            // --- WIN/LOSE MANAGER ---
            var rulesGO = new GameObject("WinLoseManager");
            var rules = rulesGO.AddComponent<WinLoseManager>();
            rules.board = board;
            rules.gameUI = gameUI;
            rules.levelLoader = loader;

            // --- TIMER UI + TIMER LOGIC ---
            var timerUiGO = new GameObject("TimerUI");
            timerUiGO.AddComponent<TimerUIPlacement>();
            var timerUI = timerUiGO.AddComponent<TimerUI>();

            var timerGO = new GameObject("GameTimer");
            var gameTimer = timerGO.AddComponent<GameTimer>();
            gameTimer.rules = rules;
            gameTimer.timerUI = timerUI;
            loader.gameTimer = gameTimer;
            rules.gameTimer = gameTimer;

            // --- INPUT ---
            var inputGO = new GameObject("Input");
            var smart = inputGO.AddComponent<SmartClickInput>();
            smart.target = board;
            smart.cam = cam;
            smart.rules = rules;

            // --- GAMLA HUD:en (behövs för vissa script) ---
            var hudGO = new GameObject("KaelenHUD");
            var hud = hudGO.AddComponent<KaelenHUD>();
            TryAssignBoard(hud, board);

            // --- MAIN CANVAS / se till att det finns en "Canvas" ---
            Canvas targetCanvas = null;
            {
                var all = FindObjectsOfType<Canvas>();
                for (int i = 0; i < all.Length; i++)
                {
                    if (all[i].gameObject.name == "Canvas")
                    {
                        targetCanvas = all[i];
                        break;
                    }
                }

                if (targetCanvas == null && all.Length > 0)
                    targetCanvas = all[0];

                if (targetCanvas == null)
                {
                    var cGO = new GameObject("Canvas");
                    targetCanvas = cGO.AddComponent<Canvas>();
                    targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    cGO.AddComponent<CanvasScaler>();
                    cGO.AddComponent<GraphicRaycaster>();
                }
            }

            // --- BG: metall (läggs FÖRST i canvas) ---
            {
                var bgGo = new GameObject("BG_Metal", typeof(RectTransform), typeof(Image));
                bgGo.transform.SetParent(targetCanvas.transform, false);

                var bgRt = (RectTransform)bgGo.transform;
                bgRt.anchorMin = Vector2.zero;
                bgRt.anchorMax = Vector2.one;
                bgRt.pivot = new Vector2(0.5f, 0.5f);
                bgRt.offsetMin = Vector2.zero;
                bgRt.offsetMax = Vector2.zero;

                var bgImg = bgGo.GetComponent<Image>();
                var spr = Resources.Load<Sprite>("Art/ui_bg/bg_metal_base");
                if (spr) bgImg.sprite = spr;
                bgImg.preserveAspect = false;
                bgImg.color = Color.white;

                // längst bak
                bgGo.transform.SetAsFirstSibling();
            }

            // --- HUDTop (NY: visor + missile i samma rad) ---
            var hudTopGO = new GameObject("HUDTop", typeof(RectTransform));
            hudTopGO.transform.SetParent(targetCanvas.transform, false);
            var hudTopRT = hudTopGO.GetComponent<RectTransform>();
            hudTopRT.anchorMin = new Vector2(0f, 1f);
            hudTopRT.anchorMax = new Vector2(1f, 1f);
            hudTopRT.pivot = new Vector2(0.5f, 1f);
            // nu 120 px så den matchar HUDTop.cs
            hudTopRT.offsetMin = new Vector2(0f, -120f);
            hudTopRT.offsetMax = new Vector2(0f, 0f);
            var hudTop = hudTopGO.AddComponent<HUDTop>();
            hudTop.board = board;
            hudTop.loader = loader;
            hudTop.gameTimer = gameTimer;
            hudTop.gameUI = gameUI;

            // --- Visor Scan Effect (delas av HUDTop) ---
            var visorScanGO = new GameObject("VisorScanEffect");
            var visorScan = visorScanGO.AddComponent<VisorScanEffect>();
            visorScan.board = board;
            hudTop.scanEffect = visorScan;

            // --- TimerUI in i canvas så den syns ---
            timerUiGO.transform.SetParent(targetCanvas.transform, false);
            timerUiGO.transform.SetAsLastSibling(); // ovanpå bakgrunden

            // --- OM INGA LEVELS: skapa 1 default ---
            if (loader.levels == null || loader.levels.Length == 0)
            {
                loader.levels = new LevelDef[1];
                loader.levels[0] = new LevelDef
                {
                    width = defaultWidth,
                    height = defaultHeight,
                    tileSize = defaultTileSize,
                    mineDensity = defaultMineDensity,
                    timed = defaultTimed,
                    timeLimitSeconds = defaultTimeLimitSeconds
                };
                loader.currentIndex = 0;
            }

            // --- LÄS KONFIG FRÅN MENY ---
            GameSessionConfig cfg;
            if (GameModeSettings.hasConfig)
            {
                cfg = GameModeSettings.current;
            }
            else
            {
                int idx = Mathf.Clamp(loader.currentIndex, 0, loader.levels.Length - 1);
                cfg = new GameSessionConfig
                {
                    mode = GameModeType.Campaign,
                    levelIndex = idx,
                    allowMissiles = true,
                    timed = loader.levels[idx].timed,
                    timeLimitSeconds = loader.levels[idx].timeLimitSeconds,
                    timeBonusPerSafeReveal = 0f,
                    useCustomBoardSize = false,
                    customWidth = 0,
                    customHeight = 0,
                    customMineDensity = 0f,
                    tilesetTheme = "grass"
                };
            }

            // --- TEMA ---
            string finalTheme = cfg.tilesetTheme;
            if (cfg.mode == GameModeType.Arcade &&
                (string.IsNullOrEmpty(finalTheme) || finalTheme == "random"))
            {
                string[] pool = { "grass", "desert", "ice", "toxic", "boss_scorpion" };
                finalTheme = pool[Random.Range(0, pool.Length)];
            }

            board.SetTheme(finalTheme);
            board.SetMissilesEnabled(cfg.allowMissiles);
            board.gameTimer = gameTimer;
            board.bonusPerSafeReveal = cfg.timeBonusPerSafeReveal;

            // --- LÄGE ---
            switch (cfg.mode)
            {
                case GameModeType.Campaign:
                    {
                        int li = Mathf.Clamp(cfg.levelIndex, 0, loader.levels.Length - 1);
                        loader.currentIndex = li;
                        loader.LoadLevel(li);
                        break;
                    }
                case GameModeType.Arcade:
                    {
                        // vi tvingar 8x14 här
                        int w = 8;
                        int h = 14;
                        float dens = cfg.useCustomBoardSize ? cfg.customMineDensity : arcadeMineDensity;

                        var arc = new LevelDef
                        {
                            width = w,
                            height = h,
                            tileSize = defaultTileSize,
                            mineDensity = dens,
                            timed = cfg.timed,
                            timeLimitSeconds = cfg.timeLimitSeconds
                        };

                        loader.levels = new LevelDef[1] { arc };
                        loader.currentIndex = 0;
                        loader.LoadLevel(0);

                        board.SetMissilesEnabled(cfg.allowMissiles);
                        board.SetTheme(finalTheme);

                        if (cfg.timed)
                            gameTimer.StartLevelTimer(true, cfg.timeLimitSeconds);
                        break;
                    }
                case GameModeType.Boss:
                    {
                        var boss = new LevelDef
                        {
                            width = bossWidth,
                            height = bossHeight,
                            tileSize = defaultTileSize,
                            mineDensity = bossMineDensity,
                            timed = true,
                            timeLimitSeconds = (cfg.timeLimitSeconds > 0f ? cfg.timeLimitSeconds : bossTimeLimitSeconds)
                        };

                        loader.levels = new LevelDef[1] { boss };
                        loader.currentIndex = 0;
                        loader.LoadLevel(0);

                        board.SetMissilesEnabled(false);
                        board.SetTheme(finalTheme);

                        gameTimer.StartLevelTimer(true, boss.timeLimitSeconds);
                        break;
                    }
            }

            // --- VIEWPORT FITTER ---
            var fitGO = new GameObject("ViewportFitter");
            var fitter = fitGO.AddComponent<BoardViewportFitter>();
            fitter.cam = cam;
            fitter.board = board;

            if (hudTop != null)
            {
                fitter.uiTopPx = hudTop.topBarHeight;
                fitter.uiLeftPx = hudTop.sideGutterWidth;
                fitter.uiRightPx = hudTop.sideGutterWidth;
            }
            else
            {
                fitter.uiTopPx = hud.topBarHeight;
                fitter.uiLeftPx = hud.sideGutterWidth;
                fitter.uiRightPx = hud.sideGutterWidth;
            }

            fitter.uiBottomPx = 0f;
            fitter.referenceResolution = new Vector2(1080f, 1920f);
            fitter.extraWorldPadding = 0.20f;
            fitter.updateEveryFrame = true;
            fitter.FitNow();
        }

        static void TryAssignBoard(Component targetComponent, Board boardRef)
        {
            if (targetComponent == null || boardRef == null) return;

            var t = targetComponent.GetType();

            var f = t.GetField("board", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                 ?? t.GetField("Board", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null && f.FieldType == typeof(Board))
                f.SetValue(targetComponent, boardRef);

            var p = t.GetProperty("board", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                 ?? t.GetProperty("Board", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null && p.PropertyType == typeof(Board) && p.CanWrite)
                p.SetValue(targetComponent, boardRef, null);
        }
    }
}
