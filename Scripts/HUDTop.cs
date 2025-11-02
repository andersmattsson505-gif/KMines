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
        public float topBarHeight = 88f;
        public float sideGutterWidth = 68f;

        [Header("Colors")]
        public Color topBarColor = new Color(0.05f, 0.06f, 0.08f, 1f);
        public Color buttonColor = new Color(0.65f, 0.65f, 0.8f, 1f);
        public Color missileEnabledColor = Color.white;
        public Color missileDisabledColor = new Color(1f, 1f, 1f, 0.3f);
        public Color visorEnabledColor = Color.white;
        public Color visorDisabledColor = new Color(1f, 1f, 1f, 0.3f);
        public Color visorTextEnabledColor = Color.white;
        public Color visorTextDisabledColor = new Color(1f, 1f, 1f, 0.4f);

        [Header("Icons (Resources/Art/...)")]
        public string missileIconPath = "Art/missile_logo";
        public string visorIconPath = "Art/visor_pulse";

        RectTransform rootRT;
        RectTransform topBarRT;
        RectTransform leftGutterRT;
        RectTransform rightClusterRT;

        Image missileIconImg;
        Text missileCountTxt;
        Button missileButton;

        Image visorIconImg;
        Text visorCountTxt;
        Button visorButton;

        Font runtimeFont;

        void Awake()
        {
            if (!board) board = FindObjectOfType<Board>();
            if (!loader) loader = FindObjectOfType<LevelLoader>();
            if (!gameTimer) gameTimer = FindObjectOfType<GameTimer>();
            if (!gameUI) gameUI = FindObjectOfType<GameUI>();
            if (!scanEffect) scanEffect = FindObjectOfType<VisorScanEffect>();

            runtimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        void Start()
        {
            BuildHUD();
        }

        // -----------------------
        // BUILD
        // -----------------------
        void BuildHUD()
        {
            // Canvas-root (detta gameobjectet)
            rootRT = GetComponent<RectTransform>();
            if (!rootRT)
                rootRT = gameObject.AddComponent<RectTransform>();

            rootRT.anchorMin = new Vector2(0f, 1f);
            rootRT.anchorMax = new Vector2(1f, 1f);
            rootRT.pivot = new Vector2(0.5f, 1f);
            rootRT.offsetMin = new Vector2(0f, -topBarHeight);
            rootRT.offsetMax = new Vector2(0f, 0f);

            // ---------- Top bar background ----------
            {
                var go = new GameObject("TopBar");
                go.transform.SetParent(rootRT, false);

                topBarRT = go.AddComponent<RectTransform>();
                topBarRT.anchorMin = new Vector2(0f, 0f);
                topBarRT.anchorMax = new Vector2(1f, 1f);
                topBarRT.pivot = new Vector2(0.5f, 0.5f);
                topBarRT.offsetMin = new Vector2(0f, -topBarHeight);
                topBarRT.offsetMax = new Vector2(0f, 0f);

                var img = go.AddComponent<Image>();
                img.color = topBarColor;
            }

            // ---------- Left gutter (hamburger) ----------
            {
                var go = new GameObject("LeftGutter");
                go.transform.SetParent(rootRT, false);

                leftGutterRT = go.AddComponent<RectTransform>();
                leftGutterRT.anchorMin = new Vector2(0f, 0f);
                leftGutterRT.anchorMax = new Vector2(0f, 1f);
                leftGutterRT.pivot = new Vector2(0f, 1f);
                leftGutterRT.sizeDelta = new Vector2(sideGutterWidth, 0f);
                leftGutterRT.anchoredPosition = new Vector2(0f, 0f);

                var btnGO = new GameObject("MenuButton", typeof(Image), typeof(Button));
                btnGO.transform.SetParent(leftGutterRT, false);

                var btnRT = btnGO.GetComponent<RectTransform>();
                btnRT.anchorMin = new Vector2(0f, 1f);
                btnRT.anchorMax = new Vector2(0f, 1f);
                btnRT.pivot = new Vector2(0f, 1f);
                btnRT.sizeDelta = new Vector2(48f, 48f);
                btnRT.anchoredPosition = new Vector2(12f, -12f);

                var img = btnGO.GetComponent<Image>();
                img.color = buttonColor;

                var btn = btnGO.GetComponent<Button>();
                btn.onClick.AddListener(OnMenuClicked);
            }

            // ---------- Right cluster (missile + visor) ----------
            {
                var go = new GameObject("RightCluster");
                go.transform.SetParent(rootRT, false);

                rightClusterRT = go.AddComponent<RectTransform>();
                rightClusterRT.anchorMin = new Vector2(1f, 0f);
                rightClusterRT.anchorMax = new Vector2(1f, 1f);
                rightClusterRT.pivot = new Vector2(1f, 1f);
                rightClusterRT.sizeDelta = new Vector2(280f, 0f);
                rightClusterRT.anchoredPosition = new Vector2(-12f, 0f);

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
                    var missileSprite = Resources.Load<Sprite>(missileIconPath);
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
                    // nära ikonen
                    txtRT.anchoredPosition = new Vector2(-8f, 0f);
                }

                // ----- Visor row -----
                {
                    var row = new GameObject("VisorRow").AddComponent<RectTransform>();
                    row.transform.SetParent(rightClusterRT, false);
                    row.anchorMin = new Vector2(1f, 1f);
                    row.anchorMax = new Vector2(1f, 1f);
                    row.pivot = new Vector2(1f, 1f);
                    // lägg på samma höjd men längre åt vänster
                    row.anchoredPosition = new Vector2(-140f, 0f);
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

                    var visorSprite = Resources.Load<Sprite>(visorIconPath);
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
                    // exakt samma rad som missile
                    vTxtRT.anchoredPosition = new Vector2(-8f, 0f);
                }
            }
        }

        // -------------------------------------------------
        // Button callbacks
        // -------------------------------------------------
        void OnMenuClicked()
        {
            if (gameUI != null)
                gameUI.TogglePause();
        }

        void OnMissileClicked()
        {
            if (board == null) return;
            if (board.MissileCount() <= 0) return;

            board.ArmMissile();
        }

        void OnVisorClicked()
        {
            if (scanEffect != null)
                scanEffect.PulseRadarSweep();
        }

        void Update()
        {
            if (!board) return;

            // ----- MISSILE STATUS -----
            if (missileCountTxt != null && missileIconImg != null)
            {
                int m = board.MissileCount();
                missileCountTxt.text = "x" + m.ToString();
                bool hasAny = m > 0;
                missileIconImg.color = hasAny ? missileEnabledColor : missileDisabledColor;
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
