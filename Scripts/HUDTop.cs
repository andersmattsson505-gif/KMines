using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

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

        [Header("Layout")]
        public float topBarHeight = 120f;
        public float sideGutterWidth = 68f;   // <-- tillbaka pga Boot.cs

        [Header("Sizes")]
        public float hitSize = 110f;
        public float iconSize = 96f;
        public int countFontSize = 44;

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

        [Header("Icons")]
        public string missileIconPath = "Art/missile_logo";
        public string visorIconPath = "Art/visor_pulse";

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
            var rt = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(0f, -topBarHeight);
            rt.offsetMax = new Vector2(0f, 0f);

            // bg
            var bg = new GameObject("BG", typeof(Image));
            bg.transform.SetParent(rt, false);
            var bgRT = bg.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().color = topBarColor;

            // menu
            {
                var btn = new GameObject("MenuButton", typeof(RectTransform), typeof(Image), typeof(Button));
                btn.transform.SetParent(rt, false);
                var brt = btn.GetComponent<RectTransform>();
                brt.anchorMin = new Vector2(0f, 1f);
                brt.anchorMax = new Vector2(0f, 1f);
                brt.pivot = new Vector2(0f, 1f);
                brt.sizeDelta = new Vector2(60f, 60f);
                brt.anchoredPosition = new Vector2(14f, -14f);
                btn.GetComponent<Image>().color = buttonColor;
                btn.GetComponent<Button>().onClick.AddListener(OnMenuClicked);
            }

            // right cluster
            var cluster = new GameObject("RightCluster", typeof(RectTransform));
            cluster.transform.SetParent(rt, false);
            var cr = cluster.GetComponent<RectTransform>();
            cr.anchorMin = new Vector2(1f, 1f);
            cr.anchorMax = new Vector2(1f, 1f);
            cr.pivot = new Vector2(1f, 1f);
            cr.sizeDelta = new Vector2(460f, topBarHeight);
            cr.anchoredPosition = new Vector2(-12f, 0f);

            // MISSILE (samma struktur som visor)
            {
                var btn = new GameObject("MissileButton", typeof(RectTransform), typeof(Image), typeof(Button));
                btn.transform.SetParent(cr, false);
                var brt = btn.GetComponent<RectTransform>();
                brt.anchorMin = new Vector2(1f, 0.5f);
                brt.anchorMax = new Vector2(1f, 0.5f);
                brt.pivot = new Vector2(1f, 0.5f);
                brt.sizeDelta = new Vector2(hitSize, hitSize);
                brt.anchoredPosition = new Vector2(-20f, 0f);

                var bImg = btn.GetComponent<Image>();
                bImg.color = new Color(1f, 1f, 1f, 0f);
                bImg.raycastTarget = true;

                btn.GetComponent<Button>().onClick.AddListener(OnMissileClicked);

                var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                icon.transform.SetParent(btn.transform, false);
                var irt = icon.GetComponent<RectTransform>();
                irt.anchorMin = new Vector2(0.5f, 0.5f);
                irt.anchorMax = new Vector2(0.5f, 0.5f);
                irt.pivot = new Vector2(0.5f, 0.5f);
                irt.sizeDelta = new Vector2(iconSize, iconSize);
                irt.anchoredPosition = Vector2.zero;

                missileIconImg = icon.GetComponent<Image>();
                missileIconImg.preserveAspect = true;
                var mspr = Resources.Load<Sprite>(missileIconPath);
                if (mspr) missileIconImg.sprite = mspr;

                var tgo = new GameObject("MissileCount", typeof(Text));
                tgo.transform.SetParent(btn.transform, false);
                missileCountTxt = tgo.GetComponent<Text>();
                missileCountTxt.font = runtimeFont;
                missileCountTxt.fontSize = countFontSize;
                missileCountTxt.alignment = TextAnchor.MiddleRight;
                missileCountTxt.color = new Color(0.85f, 0.92f, 1f, 0.9f);
                missileCountTxt.text = "x0";

                var trt = missileCountTxt.rectTransform;
                trt.anchorMin = new Vector2(0f, 0.5f);
                trt.anchorMax = new Vector2(0f, 0.5f);
                trt.pivot = new Vector2(1f, 0.5f);
                trt.sizeDelta = new Vector2(72f, 50f);
                trt.anchoredPosition = new Vector2(-(iconSize * 0.5f) - 8f, 0f);
            }

            // VISOR
            {
                var btn = new GameObject("VisorButton", typeof(RectTransform), typeof(Image), typeof(Button));
                btn.transform.SetParent(cr, false);
                var brt = btn.GetComponent<RectTransform>();
                brt.anchorMin = new Vector2(1f, 0.5f);
                brt.anchorMax = new Vector2(1f, 0.5f);
                brt.pivot = new Vector2(1f, 0.5f);
                brt.sizeDelta = new Vector2(hitSize, hitSize);
                brt.anchoredPosition = new Vector2(-(20f + hitSize + 20f), 0f);

                var bImg = btn.GetComponent<Image>();
                bImg.color = new Color(1f, 1f, 1f, 0f);
                bImg.raycastTarget = true;

                btn.GetComponent<Button>().onClick.AddListener(OnVisorClicked);

                var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                icon.transform.SetParent(btn.transform, false);
                var irt = icon.GetComponent<RectTransform>();
                irt.anchorMin = new Vector2(0.5f, 0.5f);
                irt.anchorMax = new Vector2(0.5f, 0.5f);
                irt.pivot = new Vector2(0.5f, 0.5f);
                irt.sizeDelta = new Vector2(iconSize, iconSize);
                irt.anchoredPosition = Vector2.zero;

                visorIconImg = icon.GetComponent<Image>();
                visorIconImg.preserveAspect = true;
                var vspr = Resources.Load<Sprite>(visorIconPath);
                if (vspr) visorIconImg.sprite = vspr;

                var tgo = new GameObject("VisorCount", typeof(Text));
                tgo.transform.SetParent(btn.transform, false);
                visorCountTxt = tgo.GetComponent<Text>();
                visorCountTxt.font = runtimeFont;
                visorCountTxt.fontSize = countFontSize;
                visorCountTxt.alignment = TextAnchor.MiddleRight;
                visorCountTxt.text = "x0";
                visorCountTxt.color = visorTextDisabledColor;

                var trt = visorCountTxt.rectTransform;
                trt.anchorMin = new Vector2(0f, 0.5f);
                trt.anchorMax = new Vector2(0f, 0.5f);
                trt.pivot = new Vector2(1f, 0.5f);
                trt.sizeDelta = new Vector2(72f, 50f);
                trt.anchoredPosition = new Vector2(-(iconSize * 0.5f) - 8f, 0f);
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

            if (board.IsMissileArmed())
            {
                var fi = typeof(Board).GetField("missileArmed", BindingFlags.Instance | BindingFlags.NonPublic);
                if (fi != null) fi.SetValue(board, false);
                return;
            }

            board.ArmMissile();
        }

        void OnVisorClicked()
        {
            bool ok = PlayerInventory.TryConsumePulseVisor();
            if (!ok) return;

            if (scanEffect != null)
                scanEffect.PulseRadarSweep();
        }

        void Update()
        {
            // missile
            if (board != null && missileIconImg != null && missileCountTxt != null)
            {
                int m = board.MissileCount();
                missileCountTxt.text = "x" + m;

                bool armed = board.IsMissileArmed();
                missileIconImg.color = armed
                    ? missileArmedColor
                    : (m > 0 ? missileEnabledColor : missileDisabledColor);
            }

            // visor
            if (visorIconImg != null && visorCountTxt != null)
            {
                int v = PlayerInventory.GetPulseVisorOwned();
                visorCountTxt.text = "x" + v;

                bool has = v > 0;
                visorIconImg.color = has ? visorEnabledColor : visorDisabledColor;
                visorCountTxt.color = has ? visorTextEnabledColor : visorTextDisabledColor;
            }
        }
    }
}
