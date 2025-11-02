using UnityEngine;
using UnityEngine.UI;

namespace KMines
{
    [DefaultExecutionOrder(10000)]
    public class HUDTop : MonoBehaviour
    {
        [Header("Auto-wired")]
        public Board board;
        public LevelLoader loader;
        public GameTimer gameTimer;
        public GameUI gameUI;
        public VisorScanEffect scanEffect;

        [Header("Layout (px @1080x1920)")]
        public float topBarHeight = 96f;
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
        RectTransform rightClusterRT;

        Image missileIconImg;
        Text missileCountTxt;

        Image visorIconImg;
        Text visorCountTxt;

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

        void BuildHUD()
        {
            // root
            rootRT = GetComponent<RectTransform>();
            if (!rootRT) rootRT = gameObject.AddComponent<RectTransform>();
            rootRT.anchorMin = new Vector2(0f, 1f);
            rootRT.anchorMax = new Vector2(1f, 1f);
            rootRT.pivot = new Vector2(0.5f, 1f);
            rootRT.offsetMin = new Vector2(0f, -topBarHeight);
            rootRT.offsetMax = new Vector2(0f, 0f);

            // bakgrund
            var bgGO = new GameObject("TopBarBG", typeof(Image));
            bgGO.transform.SetParent(rootRT, false);
            var bgRT = bgGO.GetComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0f, 0f);
            bgRT.anchorMax = new Vector2(1f, 1f);
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            bgGO.GetComponent<Image>().color = topBarColor;

            // vänster menyknapp
            {
                var btnGO = new GameObject("MenuButton", typeof(RectTransform), typeof(Image), typeof(Button));
                btnGO.transform.SetParent(rootRT, false);

                var brt = btnGO.GetComponent<RectTransform>();
                brt.anchorMin = new Vector2(0f, 1f);
                brt.anchorMax = new Vector2(0f, 1f);
                brt.pivot = new Vector2(0f, 1f);
                brt.sizeDelta = new Vector2(56f, 56f);
                brt.anchoredPosition = new Vector2(12f, -12f);

                var img = btnGO.GetComponent<Image>();
                img.color = buttonColor;

                var btn = btnGO.GetComponent<Button>();
                btn.onClick.AddListener(OnMenuClicked);
            }

            // höger kluster
            {
                var go = new GameObject("RightCluster", typeof(RectTransform));
                go.transform.SetParent(rootRT, false);
                rightClusterRT = go.GetComponent<RectTransform>();
                rightClusterRT.anchorMin = new Vector2(1f, 1f);
                rightClusterRT.anchorMax = new Vector2(1f, 1f);
                rightClusterRT.pivot = new Vector2(1f, 1f);
                rightClusterRT.sizeDelta = new Vector2(340f, topBarHeight);
                rightClusterRT.anchoredPosition = new Vector2(-8f, 0f);

                // MISSILE (ytterst till höger)
                {
                    var row = new GameObject("MissileRow", typeof(RectTransform));
                    row.transform.SetParent(rightClusterRT, false);
                    var rrt = row.GetComponent<RectTransform>();
                    rrt.anchorMin = new Vector2(1f, 1f);
                    rrt.anchorMax = new Vector2(1f, 1f);
                    rrt.pivot = new Vector2(1f, 1f);
                    rrt.sizeDelta = new Vector2(150f, topBarHeight);
                    rrt.anchoredPosition = new Vector2(0f, 0f);

                    var iconGO = new GameObject("MissileIcon", typeof(RectTransform), typeof(Image), typeof(Button));
                    iconGO.transform.SetParent(rrt, false);
                    var iconRT = iconGO.GetComponent<RectTransform>();
                    iconRT.anchorMin = new Vector2(1f, 0.5f);
                    iconRT.anchorMax = new Vector2(1f, 0.5f);
                    iconRT.pivot = new Vector2(1f, 0.5f);
                    iconRT.sizeDelta = new Vector2(64f, 64f);
                    iconRT.anchoredPosition = new Vector2(0f, 0f);

                    missileIconImg = iconGO.GetComponent<Image>();
                    missileIconImg.preserveAspect = true;
                    var spr = Resources.Load<Sprite>(missileIconPath);
                    if (spr) missileIconImg.sprite = spr;

                    var btn = iconGO.GetComponent<Button>();
                    btn.onClick.AddListener(OnMissileClicked);

                    var txtGO = new GameObject("MissileCount", typeof(Text));
                    txtGO.transform.SetParent(rrt, false);
                    missileCountTxt = txtGO.GetComponent<Text>();
                    missileCountTxt.font = runtimeFont;
                    missileCountTxt.fontSize = 40;
                    missileCountTxt.alignment = TextAnchor.MiddleLeft;
                    missileCountTxt.color = new Color(0.85f, 0.92f, 1f, 0.9f);
                    missileCountTxt.text = "x0";

                    var txtRT = missileCountTxt.rectTransform;
                    txtRT.anchorMin = new Vector2(1f, 0.5f);
                    txtRT.anchorMax = new Vector2(1f, 0.5f);
                    txtRT.pivot = new Vector2(0f, 0.5f);
                    txtRT.sizeDelta = new Vector2(80f, 48f);
                    txtRT.anchoredPosition = new Vector2(-72f, 0f);
                }

                // VISOR (lite åt vänster)
                {
                    var row = new GameObject("VisorRow", typeof(RectTransform));
                    row.transform.SetParent(rightClusterRT, false);
                    var rrt = row.GetComponent<RectTransform>();
                    rrt.anchorMin = new Vector2(1f, 1f);
                    rrt.anchorMax = new Vector2(1f, 1f);
                    rrt.pivot = new Vector2(1f, 1f);
                    rrt.sizeDelta = new Vector2(150f, topBarHeight);
                    rrt.anchoredPosition = new Vector2(-160f, 0f);

                    var iconGO = new GameObject("VisorIcon", typeof(RectTransform), typeof(Image), typeof(Button));
                    iconGO.transform.SetParent(rrt, false);
                    var iconRT = iconGO.GetComponent<RectTransform>();
                    iconRT.anchorMin = new Vector2(1f, 0.5f);
                    iconRT.anchorMax = new Vector2(1f, 0.5f);
                    iconRT.pivot = new Vector2(1f, 0.5f);
                    iconRT.sizeDelta = new Vector2(64f, 64f);
                    iconRT.anchoredPosition = new Vector2(0f, 0f);

                    visorIconImg = iconGO.GetComponent<Image>();
                    visorIconImg.preserveAspect = true;
                    var spr = Resources.Load<Sprite>(visorIconPath);
                    if (spr) visorIconImg.sprite = spr;

                    var btn = iconGO.GetComponent<Button>();
                    btn.onClick.AddListener(OnVisorClicked);

                    var txtGO = new GameObject("VisorCount", typeof(Text));
                    txtGO.transform.SetParent(rrt, false);
                    visorCountTxt = txtGO.GetComponent<Text>();
                    visorCountTxt.font = runtimeFont;
                    visorCountTxt.fontSize = 40;
                    visorCountTxt.alignment = TextAnchor.MiddleLeft;
                    visorCountTxt.text = "x0";
                    visorCountTxt.color = visorTextDisabledColor;

                    var txtRT = visorCountTxt.rectTransform;
                    txtRT.anchorMin = new Vector2(1f, 0.5f);
                    txtRT.anchorMax = new Vector2(1f, 0.5f);
                    txtRT.pivot = new Vector2(0f, 0.5f);
                    txtRT.sizeDelta = new Vector2(80f, 48f);
                    txtRT.anchoredPosition = new Vector2(-72f, 0f);
                }
            }
        }

        void OnMenuClicked()
        {
            if (gameUI != null)
                gameUI.ShowPausePanel();
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
            if (board != null && missileCountTxt != null && missileIconImg != null)
            {
                int m = board.MissileCount();
                missileCountTxt.text = "x" + m.ToString();
                missileIconImg.color = m > 0 ? missileEnabledColor : missileDisabledColor;
            }

            if (visorCountTxt != null && visorIconImg != null)
            {
                int v = PlayerInventory.GetPulseVisorOwned();
                visorCountTxt.text = "x" + v.ToString();
                bool has = v > 0;
                visorIconImg.color = has ? visorEnabledColor : visorDisabledColor;
                visorCountTxt.color = has ? visorTextEnabledColor : visorTextDisabledColor;
            }
        }
    }
}
