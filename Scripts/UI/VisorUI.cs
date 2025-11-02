using UnityEngine;
using UnityEngine.UI;

namespace KMines
{
    /// <summary>
    /// Äldre/auto visor-knapp. NU: stänger av sig om HUDTop finns.
    /// </summary>
    [DefaultExecutionOrder(25000)]
    public class VisorUI : MonoBehaviour
    {
        [Header("Layout (reference 1080x1920)")]
        public float positionXFromRight = 40f;
        public float positionYFromTop = 160f;
        public float iconSize = 96f;
        public int countFontSize = 36;

        [Header("Appearance")]
        public Sprite visorSprite;
        public Color enabledColor = Color.white;
        public Color disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.7f);
        public Color textColorEnabled = Color.white;
        public Color textColorDisabled = new Color(0.6f, 0.6f, 0.6f, 0.7f);

        [Header("Effect hookup")]
        public VisorScanEffect scanEffect;

        Canvas canvas;
        RectTransform buttonRT;
        Image iconImage;
        Text countText;
        Font runtimeFont;

        Rect lastSafe = Rect.zero;
        float lastScale = -1f;
        bool built;

        void Awake()
        {
            // om nya HUDen redan finns → den visar visor → stäng denna
            if (FindObjectOfType<HUDTop>() != null)
            {
                Destroy(gameObject);
                return;
            }

            runtimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        void Start()
        {
            BuildIfNeeded();
            ApplySafeArea();
        }

        void BuildIfNeeded()
        {
            if (built) return;
            built = true;

            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 25000;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();

            // root
            var rootRT = canvas.GetComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            // button
            var btnGO = new GameObject("VisorButton");
            btnGO.transform.SetParent(rootRT, false);
            buttonRT = btnGO.AddComponent<RectTransform>();
            buttonRT.sizeDelta = new Vector2(iconSize, iconSize);
            buttonRT.anchorMin = new Vector2(1f, 1f);
            buttonRT.anchorMax = new Vector2(1f, 1f);
            buttonRT.pivot = new Vector2(1f, 1f);

            iconImage = btnGO.AddComponent<Image>();
            iconImage.raycastTarget = true;
            if (visorSprite == null) visorSprite = Resources.Load<Sprite>("Art/visor_pulse");
            iconImage.sprite = visorSprite;

            var button = btnGO.AddComponent<Button>();
            button.onClick.AddListener(OnVisorClicked);

            // Count text
            var textGO = new GameObject("CountText");
            textGO.transform.SetParent(buttonRT, false);
            var textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = new Vector2(1f, 0f);
            textRT.anchorMax = new Vector2(1f, 0f);
            textRT.pivot = new Vector2(1f, 0f);
            textRT.sizeDelta = new Vector2(iconSize, iconSize * 0.5f);
            textRT.anchoredPosition = new Vector2(0f, 0f);

            countText = textGO.AddComponent<Text>();
            countText.font = runtimeFont;
            countText.fontSize = countFontSize;
            countText.alignment = TextAnchor.LowerRight;
            countText.text = "x0";
            countText.color = textColorDisabled;
        }

        void LateUpdate()
        {
            if (!built) return;

            if (Screen.safeArea != lastSafe || canvas.scaleFactor != lastScale)
                ApplySafeArea();

            int visorCount = PlayerInventory.GetPulseVisorOwned();
            if (countText) countText.text = "x" + visorCount.ToString();

            bool hasAny = visorCount > 0;
            if (iconImage) iconImage.color = hasAny ? enabledColor : disabledColor;
            if (countText) countText.color = hasAny ? textColorEnabled : textColorDisabled;
        }

        void ApplySafeArea()
        {
            if (canvas == null || buttonRT == null) return;

            Rect safe = Screen.safeArea;
            float s = canvas.scaleFactor;

            float rightPad = (Screen.width - safe.xMax) / s;
            float topPad = (Screen.height - safe.yMax) / s;

            buttonRT.anchoredPosition = new Vector2(-(rightPad + positionXFromRight),
                                                    -(topPad + positionYFromTop));

            lastSafe = safe;
            lastScale = s;
        }

        void OnVisorClicked()
        {
            bool ok = PlayerInventory.TryConsumePulseVisor();
            if (!ok)
            {
                Debug.Log("[VisorUI] No visor charges left.");
                return;
            }

            if (scanEffect != null)
                scanEffect.PulseRadarSweep();
            else
                Debug.LogWarning("[VisorUI] No VisorScanEffect assigned.");
        }
    }
}
