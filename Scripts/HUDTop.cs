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
        public float topBarHeight = 120f;     // var 96
        public float sideGutterWidth = 68f;

        [Header("Icon sizes")]
        public float iconSize = 96f;          // var 64
        public int countFontSize = 44;        // var 36/40

        [Header("Colors")]
        public Color topBarColor = new Color(0.05f, 0.06f, 0.08f, 1f);
        public Color buttonColor = new Color(0.65f, 0.65f, 0.8f, 1f);

        public Color missileEnabledColor = Color.white;
        public Color missileDisabledColor = new Color(1f, 1f, 1f, 0.25f);
        public Color missileArmedColor = new Color(1f, 0.75f, 0.35f, 1f);

        public Color visorEnabledColor = Color.white;
        public Color visorDisabledColor = new Color(1f, 1f, 1f, 0.25f);
        public Color visorTextEnabledColor = Color.white;
        public Color visorTextDisabledColor = new Color(1f, 1f, 1f, 0.35f);

        [Header("Icons (Resources/Art/...)")]
        public string missileIconPath = "Art/missile_logo";
        public string visorIconPath = "Art/visor_pulse";

        RectTransform rootRT;
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
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
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
                brt.sizeDelta = new Vector2(60f, 60f);
                brt.anchoredPosition = new Vector2(14f, -14f);

                var img = btnGO.GetComponent<Image>();
                img.color = buttonColor;

                btnGO.GetComponent<Button>().onClick.AddListener(OnMenuClicked);
            }

            // höger kluster
            var clusterGO = new GameObject("RightCluster", typeof(RectTransform));
            clusterGO.transform.SetParent(rootRT, false);
            var clusterRT = clusterGO.GetComponent<RectTransform>();
            clusterRT.anchorMin = new Vector2(1f, 1f);
            clusterRT.anchorMax = new Vector2(1f, 1f);
            clusterRT.pivot = new Vector2(1f, 1f);
            clusterRT.sizeDelta = new Vector2(400f, topBarHeight);
            clusterRT.anchoredPosition = new Vector2(-12f, 0f);

            // MISSILE
            {
                var row = new GameObject("MissileRow", typeof(RectTransform));
                row.transform.SetParent(clusterRT, false);
                var rrt = row.GetComponent<RectTransform>();
                rrt.anchorMin = new Vector2(1f, 1f);
                rrt.anchorMax = new Vector2(1f, 1f);
                rrt.pivot = new Vector2(1f, 1f);
                rrt.sizeDelta = new Vector2(180f, topBarHeight);
                rrt.anchoredPosition = new Vector2(0f, 0f);

                var iconGO = new GameObject("MissileIcon", typeof(RectTransform), typeof(Image), typeof(Button));
                iconGO.transform.SetParent(rrt, false);
                var iconRT = iconGO.GetComponent<RectTransform>();
                iconRT.anchorMin = new Vector2(1f, 0.5f);
                iconRT.anchorMax = new Vector2(1f, 0.5f);
                iconRT.pivot = new Vector2(1f, 0.5f);
                iconRT.sizeDelta = new Vector2(iconSize, iconSize);
                iconRT.anchoredPosition = new Vector2(0f, 0f);

                missileIconImg = iconGO.GetComponent<Image>();
                missileIconImg.preserveAspect = true;
                var spr = Resources.Load<Sprite>(missileIconPath);
                if (spr) missileIconImg.sprite = spr;

                iconGO.GetComponent<Button>().onClick.AddListener(OnMissileClicked);

                var txtGO = new GameObject("MissileCount", typeof(Text));
                txtGO.transform.SetParent(rrt, false);
                missileCountTxt = txtGO.GetComponent<Text>();
                missileCountTxt.font = runtimeFont;
                missileCountTxt.fontSize = countFontSize;
                missileCountTxt.alignment = TextAnchor.MiddleLeft;
                missileCountTxt.color = new Color(0.85f, 0.92f, 1f, 0.9f);
                missileCountTxt.text = "x0";

                var txtRT = missileCountTxt.rectTransform;
                txtRT.anchorMin = new Vector2(1f, 0.5f);
                txtRT.anchorMax = new Vector2(1f, 0.5f);
                txtRT.pivot = new Vector2(0f, 0.5f);
                txtRT.sizeDelta = new Vector2(90f, 56f);
                txtRT.anchoredPosition = new Vector2(-(iconSize + 12f), 0f);
            }

            // VISOR
            {
                var row = new GameObject("VisorRow", typeof(RectTransform));
                row.transform.SetParent(clusterRT, false);
                var rrt = row.GetComponent<RectTransform>();
                rrt.anchorMin = new Vector2(1f, 1f);
                rrt.anchorMax = new Vector2(1f, 1f);
                rrt.pivot = new Vector2(1f, 1f);
                rrt.sizeDelta = new Vector2(180f, topBarHeight);
                rrt.anchoredPosition = new Vector2(-(180f), 0f);

                var iconGO = new GameObject("VisorIcon", typeof(RectTransform), typeof(Image), typeof(Button));
                iconGO.transform.SetParent(rrt, false);
                var iconRT = iconGO.GetComponent<RectTransform>();
                iconRT.anchorMin = new Vector2(1f, 0.5f);
                iconRT.anchorMax = new Vector2(1f, 0.5f);
                iconRT.pivot = new Vector2(1f, 0.5f);
                iconRT.sizeDelta = new Vector2(iconSize, iconSize);
                iconRT.anchoredPosition = new Vector2(0f, 0f);

                visorIconImg = iconGO.GetComponent<Image>();
                visorIconImg.preserveAspect = true;
                var spr = Resources.Load<Sprite>(visorIconPath);
                if (spr) visorIconImg.sprite = spr;

                iconGO.GetComponent<Button>().onClick.AddListener(OnVisorClicked);

                var txtGO = new GameObject("VisorCount", typeof(Text));
                txtGO.transform.SetParent(rrt, false);
                visorCountTxt = txtGO.GetComponent<Text>();
                visorCountTxt.font = runtimeFont;
                visorCountTxt.fontSize = countFontSize;
                visorCountTxt.alignment = TextAnchor.MiddleLeft;
                visorCountTxt.text = "x0";
                visorCountTxt.color = visorTextDisabledColor;

                var txtRT = visorCountTxt.rectTransform;
                txtRT.anchorMin = new Vector2(1f, 0.5f);
                txtRT.anchorMax = new Vector2(1f, 0.5f);
                txtRT.pivot = new Vector2(0f, 0.5f);
                txtRT.sizeDelta = new Vector2(90f, 56f);
                txtRT.anchoredPosition = new Vector2(-(iconSize + 12f), 0f);
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
            // ge visuell feedback direkt
            board.ArmMissile();
        }

        void OnVisorClicked()
        {
            // vi kör alltid scaneffekten när man trycker – enklast att se
            if (scanEffect != null)
                scanEffect.PulseRadarSweep();
        }

        void Update()
        {
            // MISSILE UI
            if (board != null && missileIconImg != null && missileCountTxt != null)
            {
                int m = board.MissileCount();
                bool armed = board.IsMissileArmed();
                missileCountTxt.text = "x" + m.ToString();

                if (armed)
                    missileIconImg.color = missileArmedColor;
                else
                    missileIconImg.color = (m > 0 ? missileEnabledColor : missileDisabledColor);
            }

            // VISOR UI
            if (visorIconImg != null && visorCountTxt != null)
            {
                // du har VisorCheatInit i scenen så vi låtsas att vi alltid har minst 1
                int v = PlayerInventory.GetPulseVisorOwned();
                if (v < 1) v = 1;

                visorCountTxt.text = "x" + v.ToString();
                visorIconImg.color = visorEnabledColor;
                visorCountTxt.color = visorTextEnabledColor;
            }
        }
    }
}
