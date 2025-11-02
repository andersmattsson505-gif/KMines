using UnityEngine;
using UnityEngine.UI;

namespace KMines
{
    [DefaultExecutionOrder(10000)]
    public class HUDTop : MonoBehaviour
    {
        [Header("Auto-wired if empty (Boot fyller i, men vi fallbackar med FindObjectOfType)")]
        public Board board;
        public LevelLoader loader;
        public GameTimer gameTimer;
        public GameUI gameUI;
        public VisorScanEffect scanEffect;

        [Header("Layout (px @1080x1920 ref)")]
        public float topBarHeight = 96f;
        public float sideGutterWidth = 180f;

        [Header("Colors")]
        public Color topBarColor = new Color(0.07f, 0.09f, 0.11f, 1f);
        public Color gutterColor = new Color(0.02f, 0.03f, 0.04f, 1f);
        public Color accentColor = new Color(0.1f, 0.6f, 0.9f, 1f);

        public Color scoreTextColor = Color.white;
        public Color timerTextColor = Color.yellow;

        public Color missileNormalColor = Color.white;
        public Color missileArmedColor = new Color(1f, 0.95f, 0.6f, 1f);

        public Color visorEnabledColor = Color.white;
        public Color visorDisabledColor = new Color(0.4f, 0.4f, 0.4f, 0.7f);
        public Color visorTextEnabledColor = Color.white;
        public Color visorTextDisabledColor = new Color(0.6f, 0.6f, 0.6f, 0.7f);

        // Runtime refs
        Canvas canvas;
        RectTransform rootRT;

        RectTransform topBarRT;
        RectTransform leftGutterRT;
        RectTransform rightGutterRT;
        RectTransform accentLineRT;

        Text scoreLine;
        Text timerText;

        Button pauseButton;
        Text pauseGlyphText;

        Button missileButton;
        Image missileIconImg;
        Text missileCountTxt;

        Button visorButton;
        Image visorIconImg;
        Text visorCountTxt;

        Font runtimeFont;

        void Awake()
        {
            runtimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            BuildHUD();
        }

        // -------------------------------------------------
        // Bygg hela HUD Canvas + element
        // -------------------------------------------------
        void BuildHUD()
        {
            // Canvas
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20000; // under GameUI (50000), över brädet

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();

            rootRT = canvas.GetComponent<RectTransform>();
            rootRT.anchorMin = new Vector2(0f, 0f);
            rootRT.anchorMax = new Vector2(1f, 1f);
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;
            rootRT.pivot = new Vector2(0.5f, 0.5f);

            // ---------- Top bar ----------
            {
                var go = new GameObject("TopBar");
                go.transform.SetParent(rootRT, false);

                topBarRT = go.AddComponent<RectTransform>();
                topBarRT.anchorMin = new Vector2(0f, 1f);
                topBarRT.anchorMax = new Vector2(1f, 1f);
                topBarRT.pivot = new Vector2(0.5f, 1f);
                topBarRT.offsetMin = new Vector2(0f, -topBarHeight);
                topBarRT.offsetMax = new Vector2(0f, 0f);

                var img = go.AddComponent<Image>();
                img.color = topBarColor;
            }

            // ---------- Left gutter ----------
            {
                var go = new GameObject("LeftGutter");
                go.transform.SetParent(rootRT, false);

                leftGutterRT = go.AddComponent<RectTransform>();
                leftGutterRT.anchorMin = new Vector2(0f, 0f);
                leftGutterRT.anchorMax = new Vector2(0f, 1f);
                leftGutterRT.pivot = new Vector2(0f, 1f);
                leftGutterRT.offsetMin = new Vector2(0f, 0f);
                leftGutterRT.offsetMax = new Vector2(sideGutterWidth, 0f);

                var img = go.AddComponent<Image>();
                img.color = gutterColor;
            }

            // ---------- Right gutter ----------
            {
                var go = new GameObject("RightGutter");
                go.transform.SetParent(rootRT, false);

                rightGutterRT = go.AddComponent<RectTransform>();
                rightGutterRT.anchorMin = new Vector2(1f, 0f);
                rightGutterRT.anchorMax = new Vector2(1f, 1f);
                rightGutterRT.pivot = new Vector2(1f, 1f);
                rightGutterRT.offsetMin = new Vector2(-sideGutterWidth, 0f);
                rightGutterRT.offsetMax = new Vector2(0f, 0f);

                var img = go.AddComponent<Image>();
                img.color = gutterColor;
            }

            // ---------- Accent line just under top bar ----------
            {
                var go = new GameObject("AccentLine");
                go.transform.SetParent(rootRT, false);

                accentLineRT = go.AddComponent<RectTransform>();
                accentLineRT.anchorMin = new Vector2(0f, 1f);
                accentLineRT.anchorMax = new Vector2(1f, 1f);
                accentLineRT.pivot = new Vector2(0.5f, 1f);
                accentLineRT.offsetMin = new Vector2(0f, -topBarHeight - 2f);
                accentLineRT.offsetMax = new Vector2(0f, -topBarHeight);

                var img = go.AddComponent<Image>();
                img.color = accentColor;
            }

            // ---------- Pause (hamburger) knapp ----------
            {
                var pauseGO = new GameObject("PauseButton");
                pauseGO.transform.SetParent(topBarRT, false);

                var pauseRT = pauseGO.AddComponent<RectTransform>();
                pauseRT.anchorMin = new Vector2(0f, 1f);
                pauseRT.anchorMax = new Vector2(0f, 1f);
                pauseRT.pivot = new Vector2(0f, 1f);
                pauseRT.sizeDelta = new Vector2(60f, 60f);
                pauseRT.anchoredPosition = new Vector2(16f, -16f);

                var pauseImg = pauseGO.AddComponent<Image>();
                pauseImg.color = new Color(0.9f, 0.9f, 0.9f, 1f);

                pauseButton = pauseGO.AddComponent<Button>();
                pauseButton.targetGraphic = pauseImg;
                pauseButton.onClick.AddListener(OnPauseClicked);

                var glyphGO = new GameObject("Glyph");
                glyphGO.transform.SetParent(pauseGO.transform, false);

                var glyphRT = glyphGO.AddComponent<RectTransform>();
                glyphRT.anchorMin = Vector2.zero;
                glyphRT.anchorMax = Vector2.one;
                glyphRT.offsetMin = Vector2.zero;
                glyphRT.offsetMax = Vector2.zero;
                glyphRT.pivot = new Vector2(0.5f, 0.5f);

                pauseGlyphText = glyphGO.AddComponent<Text>();
                pauseGlyphText.text = "?";
                pauseGlyphText.alignment = TextAnchor.MiddleCenter;
                pauseGlyphText.fontSize = 36;
                pauseGlyphText.color = Color.black;
                pauseGlyphText.font = runtimeFont;
            }

            // ---------- ScoreLine text ----------
            {
                var go = new GameObject("ScoreLine");
                go.transform.SetParent(topBarRT, false);

                var rt = go.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 1f);
                rt.anchoredPosition = new Vector2(90f, -28f);
                rt.sizeDelta = new Vector2(800f, 40f);

                scoreLine = go.AddComponent<Text>();
                scoreLine.font = runtimeFont;
                scoreLine.fontSize = 36;
                scoreLine.color = scoreTextColor;
                scoreLine.alignment = TextAnchor.MiddleLeft;
                scoreLine.text = "Score 0   Lvl 1   Best 0";
            }

            // ---------- Timer text ----------
            {
                var go = new GameObject("TimerText");
                go.transform.SetParent(topBarRT, false);

                var rt = go.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = new Vector2(0f, -28f);
                rt.sizeDelta = new Vector2(300f, 40f);

                timerText = go.AddComponent<Text>();
                timerText.font = runtimeFont;
                timerText.fontSize = 36;
                timerText.color = timerTextColor;
                timerText.alignment = TextAnchor.MiddleCenter;
                timerText.text = "";
                timerText.enabled = false;
            }

            // ---------- RightCluster ----------
            var rightClusterRT = new GameObject("RightCluster").AddComponent<RectTransform>();
            {
                rightClusterRT.transform.SetParent(topBarRT, false);
                rightClusterRT.anchorMin = new Vector2(1f, 1f);
                rightClusterRT.anchorMax = new Vector2(1f, 1f);
                rightClusterRT.pivot = new Vector2(1f, 1f);
                rightClusterRT.anchoredPosition = new Vector2(-16f, -16f);
                rightClusterRT.sizeDelta = new Vector2(260f, 80f);
            }

            // ----- Missile row -----
            {
                var row = new GameObject("MissileRow").AddComponent<RectTransform>();
                row.transform.SetParent(rightClusterRT, false);
                row.anchorMin = new Vector2(1f, 1f);
                row.anchorMax = new Vector2(1f, 1f);
                row.pivot = new Vector2(1f, 1f);
                row.anchoredPosition = new Vector2(0f, 0f);
                row.sizeDelta = new Vector2(240f, 32f);

                var mBtnGO = new GameObject("MissileButton", typeof(Image), typeof(Button));
                mBtnGO.transform.SetParent(row, false);
                var mBtnRT = mBtnGO.GetComponent<RectTransform>();
                mBtnRT.anchorMin = new Vector2(1f, 1f);
                mBtnRT.anchorMax = new Vector2(1f, 1f);
                mBtnRT.pivot = new Vector2(1f, 1f);
                mBtnRT.sizeDelta = new Vector2(32f, 32f);
                mBtnRT.anchoredPosition = new Vector2(0f, 0f);

                missileIconImg = mBtnGO.GetComponent<Image>();
                missileIconImg.raycastTarget = true;
                missileIconImg.preserveAspect = true;
                var missileSprite = Resources.Load<Sprite>("Art/missile_logo");
                if (missileSprite != null)
                    missileIconImg.sprite = missileSprite;

                missileButton = mBtnGO.GetComponent<Button>();
                missileButton.onClick.AddListener(OnMissileClicked);

                var txtGO = new GameObject("MissileCount", typeof(Text));
                txtGO.transform.SetParent(row, false);
                missileCountTxt = txtGO.GetComponent<Text>();
                missileCountTxt.font = runtimeFont;
                missileCountTxt.fontSize = 32;
                missileCountTxt.alignment = TextAnchor.MiddleRight;
                missileCountTxt.color = new Color(0.85f, 0.92f, 1f, 0.9f);
                missileCountTxt.text = "x0";
                var txtRT = missileCountTxt.rectTransform;
                txtRT.anchorMin = new Vector2(1f, 1f);
                txtRT.anchorMax = new Vector2(1f, 1f);
                txtRT.pivot = new Vector2(1f, 1f);
                txtRT.sizeDelta = new Vector2(120f, 32f);
                txtRT.anchoredPosition = new Vector2(-40f, 0f);
            }

            // ----- Visor row -----
            {
                var row = new GameObject("VisorRow").AddComponent<RectTransform>();
                row.transform.SetParent(rightClusterRT, false);
                row.anchorMin = new Vector2(1f, 1f);
                row.anchorMax = new Vector2(1f, 1f);
                row.pivot = new Vector2(1f, 1f);
                row.anchoredPosition = new Vector2(0f, -36f);
                row.sizeDelta = new Vector2(240f, 32f);

                var vBtnGO = new GameObject("VisorButton", typeof(Image), typeof(Button));
                vBtnGO.transform.SetParent(row, false);
                var vBtnRT = vBtnGO.GetComponent<RectTransform>();
                vBtnRT.anchorMin = new Vector2(1f, 1f);
                vBtnRT.anchorMax = new Vector2(1f, 1f);
                vBtnRT.pivot = new Vector2(1f, 1f);
                vBtnRT.sizeDelta = new Vector2(32f, 32f);
                vBtnRT.anchoredPosition = new Vector2(0f, 0f);

                visorIconImg = vBtnGO.GetComponent<Image>();
                visorIconImg.raycastTarget = true;
                visorIconImg.preserveAspect = true;

                var visorSprite = Resources.Load<Sprite>("Art/visor_pulse");
                if (visorSprite != null)
                    visorIconImg.sprite = visorSprite;

                visorButton = vBtnGO.GetComponent<Button>();
                visorButton.onClick.AddListener(OnVisorClicked);

                var vTxtGO = new GameObject("VisorCount", typeof(Text));
                vTxtGO.transform.SetParent(row, false);

                visorCountTxt = vTxtGO.GetComponent<Text>();
                visorCountTxt.font = runtimeFont;
                visorCountTxt.fontSize = 32;
                visorCountTxt.alignment = TextAnchor.MiddleRight;
                visorCountTxt.text = "x0";
                visorCountTxt.color = visorTextDisabledColor;

                var vTxtRT = visorCountTxt.rectTransform;
                vTxtRT.anchorMin = new Vector2(1f, 1f);
                vTxtRT.anchorMax = new Vector2(1f, 1f);
                vTxtRT.pivot = new Vector2(1f, 1f);
                vTxtRT.sizeDelta = new Vector2(120f, 32f);
                vTxtRT.anchoredPosition = new Vector2(-40f, -4f);
            }
        }

        // -------------------------------------------------
        // Button callbacks
        // -------------------------------------------------
        void OnPauseClicked()
        {
            if (gameUI != null)
            {
                gameUI.ShowPausePanel();
            }
        }

        void OnMissileClicked()
        {
            if (board == null) return;

            // redan armerad? gör inget (Board disarmar efter skott)
            if (board.IsMissileArmed())
                return;

            // slut på missiler? gör inget
            if (board.MissileCount() <= 0)
                return;

            // annars arma
            board.ArmMissile();
        }

        void OnVisorClicked()
        {
            // dra en charge
            bool ok = PlayerInventory.TryConsumePulseVisor();
            if (!ok) return;

            // kör scaneffekt
            if (scanEffect != null)
            {
                scanEffect.PulseRadarSweep();
            }
        }

        // -------------------------------------------------
        // Helpers
        // -------------------------------------------------
        string FormatTime(float seconds)
        {
            int total = Mathf.CeilToInt(seconds);
            if (total < 0) total = 0;
            int m = total / 60;
            int s = total % 60;
            return $"{m}:{s:00}";
        }

        // -------------------------------------------------
        // LateUpdate = vi uppdaterar text/räknare varje frame
        // -------------------------------------------------
        void LateUpdate()
        {
            // auto-wire
            if (board == null) board = FindObjectOfType<Board>();
            if (loader == null) loader = FindObjectOfType<LevelLoader>();
            if (gameTimer == null) gameTimer = FindObjectOfType<GameTimer>();
            if (gameUI == null) gameUI = FindObjectOfType<GameUI>();
            if (scanEffect == null) scanEffect = FindObjectOfType<VisorScanEffect>();

            // ----- SCORE / MODE / BEST -----
            if (scoreLine != null)
            {
                int scoreNow = (board != null) ? board.score : 0;

                GameModeType mode = GameModeType.Campaign;
                int levelIndex = 0;

                if (GameModeSettings.hasConfig)
                {
                    mode = GameModeSettings.current.mode;
                    levelIndex = GameModeSettings.current.levelIndex;
                }
                else
                {
                    if (loader != null)
                        levelIndex = loader.currentIndex;
                }

                string hudText = "Score " + scoreNow.ToString();

                switch (mode)
                {
                    case GameModeType.Campaign:
                        {
                            int li = 0;
                            if (loader != null) li = loader.currentIndex;
                            int shownLevelNum = li + 1;

                            int best = HighScoreStore.GetHighScore(li);

                            hudText =
                                "Score " + scoreNow.ToString() +
                                "   Lvl " + shownLevelNum.ToString() +
                                "   Best " + best.ToString();
                            break;
                        }

                    case GameModeType.Arcade:
                        {
                            int bestArc = HighScoreStore.GetArcadeHighScore();
                            hudText =
                                "Score " + scoreNow.ToString() +
                                "   Arcade Best " + bestArc.ToString();
                            break;
                        }

                    case GameModeType.Boss:
                        {
                            int bestBoss = HighScoreStore.GetBossHighScore();
                            hudText =
                                "Score " + scoreNow.ToString() +
                                "   Boss Best " + bestBoss.ToString();
                            break;
                        }
                }

                scoreLine.text = hudText;
            }

            // ----- TIMER -----
            if (timerText != null)
            {
                if (gameTimer != null && gameTimer.IsActive())
                {
                    timerText.enabled = true;
                    timerText.text = "TIME " + FormatTime(gameTimer.GetTimeLeft());
                }
                else
                {
                    timerText.enabled = false;
                }
            }

            // ----- MISSILE STATUS -----
            if (board != null)
            {
                if (missileCountTxt != null)
                {
                    missileCountTxt.text = "x" + board.MissileCount().ToString();
                }

                if (missileIconImg != null)
                {
                    bool armed = board.IsMissileArmed();
                    missileIconImg.color = armed
                        ? missileArmedColor
                        : missileNormalColor;
                }
            }

            // ----- VISOR STATUS -----
            if (visorCountTxt != null && visorIconImg != null)
            {
                int visorCount = PlayerInventory.GetPulseVisorOwned();
                bool hasAny = visorCount > 0;

                visorCountTxt.text = "x" + visorCount.ToString();
                visorIconImg.color = hasAny ? visorEnabledColor : visorDisabledColor;
                visorCountTxt.color = hasAny ? visorTextEnabledColor : visorTextDisabledColor;
            }
        }
    }
}
