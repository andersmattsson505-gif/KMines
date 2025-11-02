using UnityEngine;
using UnityEngine.UI;

namespace KMines
{
    /// <summary>
    /// Äldre/auto missile-ikon + count uppe t.h., egen overlay-canvas.
    /// NU: om det finns en HUDTop i scenen så stänger vi av oss själva,
    /// så vi inte får dubbla ikoner.
    /// </summary>
    [DefaultExecutionOrder(32760)]
    public class MissileUI : MonoBehaviour
    {
        public Board board;

        Canvas canvas;
        Text countTxt;
        Image iconImg;
        RectTransform buttonRT;

        Rect lastSafe = Rect.zero;
        float lastScale = -1f;

        [Header("Layout")]
        public float posXFromRight = 20f;
        public float posYFromTop = 20f;
        public Vector2 iconSize = new Vector2(96f, 96f);

        void Awake()
        {
            // om vi redan har vår nya HUD -> ta bort denna
            if (FindObjectOfType<HUDTop>() != null)
            {
                Destroy(gameObject);
                return;
            }

            if (!board) board = FindObjectOfType<Board>();

            // egen liten overlay-canvas
            var go = new GameObject("MissileCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32767; // över allt annat

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            BuildUI(go.GetComponent<RectTransform>());
            ApplySafeArea();
        }

        void Update()
        {
            if (canvas == null) return;

            if (!board)
            {
                board = FindObjectOfType<Board>();
                if (!board) return;
            }

            // uppdatera safe-area vid behov
            if (Screen.safeArea != lastSafe || canvas.scaleFactor != lastScale)
                ApplySafeArea();

            // missile-count
            if (countTxt)
            {
                int count = board.MissileCount();
                countTxt.text = "x" + count.ToString();
            }

            // ikonfärg beroende på om den är armerad
            if (iconImg)
            {
                bool armed = board.IsMissileArmed();
                iconImg.color = armed ? new Color(1f, 0.95f, 0.6f, 1f) : Color.white;
            }
        }

        void BuildUI(RectTransform root)
        {
            // osynlig panel för att kunna flytta båda
            var panel = new GameObject("MissilePanel", typeof(Image)).GetComponent<RectTransform>();
            panel.SetParent(root, false);
            panel.anchorMin = new Vector2(1f, 1f);
            panel.anchorMax = new Vector2(1f, 1f);
            panel.pivot = new Vector2(1f, 1f);
            panel.sizeDelta = new Vector2(220f, 110f);
            panel.GetComponent<Image>().enabled = false;

            // knappen / ikonen
            buttonRT = new GameObject("MissileButton", typeof(Image), typeof(Button)).GetComponent<RectTransform>();
            buttonRT.SetParent(panel, false);
            buttonRT.anchorMin = new Vector2(1f, 1f);
            buttonRT.anchorMax = new Vector2(1f, 1f);
            buttonRT.pivot = new Vector2(1f, 1f);
            buttonRT.sizeDelta = iconSize;

            iconImg = buttonRT.GetComponent<Image>();
            iconImg.type = Image.Type.Simple;
            iconImg.preserveAspect = true;
            var spr = Resources.Load<Sprite>("Art/missile_logo");
            if (spr) iconImg.sprite = spr;
            else Debug.LogWarning("[KM] Missile icon not found at Resources/Art/missile_logo.png (Sprite).");

            var btn = buttonRT.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnMissileButton);

            // count-texten
            var txtGO = new GameObject("MissileCount", typeof(Text));
            txtGO.transform.SetParent(panel, false);
            countTxt = txtGO.GetComponent<Text>();
            countTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            countTxt.fontSize = 36;
            countTxt.alignment = TextAnchor.MiddleLeft;
            countTxt.color = new Color(0.85f, 0.92f, 1f, 0.9f);
            var tr = countTxt.rectTransform;
            tr.anchorMin = new Vector2(1f, 1f);
            tr.anchorMax = new Vector2(1f, 1f);
            tr.pivot = new Vector2(1f, 1f);
            tr.sizeDelta = new Vector2(120f, 48f);
        }

        void OnMissileButton()
        {
            if (!board) return;
            if (board.IsMissileArmed()) return;
            if (board.MissileCount() <= 0) return;
            board.ArmMissile();
        }

        void ApplySafeArea()
        {
            if (canvas == null || buttonRT == null) return;

            Rect safe = Screen.safeArea;
            float s = canvas.scaleFactor;

            float right = (Screen.width - safe.xMax) / s;
            float top = (Screen.height - safe.yMax) / s;

            // panelen
            var panel = buttonRT.parent as RectTransform;
            if (panel != null)
            {
                panel.anchoredPosition = new Vector2(-(right + posXFromRight), -(top + posYFromTop));
            }

            // texten (läggs rakt under ikonen)
            if (countTxt != null)
            {
                var tr = countTxt.rectTransform;
                tr.anchoredPosition = new Vector2(-(right + posXFromRight) - (iconSize.x * 0.1f),
                                                  -(top + posYFromTop) - (iconSize.y * 0.25f));
            }

            lastSafe = safe;
            lastScale = s;
        }
    }
}
