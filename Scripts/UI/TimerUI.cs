using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KMines
{
    // Visar nedräkning högst upp. Respekterar safe area & kör i overlay-canvas.
    [RequireComponent(typeof(Canvas))]
    public class TimerUI : MonoBehaviour
    {
        public Vector2 referenceResolution = new Vector2(1080, 1920);
        public bool useSafeArea = true;

        Canvas canvas;
        CanvasScaler scaler;
        GraphicRaycaster raycaster;

        RectTransform panelRT;
        TextMeshProUGUI timerText;

        void Awake()
        {
            canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 31000;

            scaler = GetComponent<CanvasScaler>();
            if (!scaler) scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.matchWidthOrHeight = 1f;

            raycaster = GetComponent<GraphicRaycaster>();
            if (!raycaster) raycaster = gameObject.AddComponent<GraphicRaycaster>();

            var panelGO = new GameObject("TimerPanel");
            panelGO.transform.SetParent(transform, false);
            panelRT = panelGO.AddComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.5f, 1f);
            panelRT.anchorMax = new Vector2(0.5f, 1f);
            panelRT.pivot = new Vector2(0.5f, 1f);
            panelRT.sizeDelta = new Vector2(300f, 60f);

            var textGO = new GameObject("TimerText");
            textGO.transform.SetParent(panelGO.transform, false);
            timerText = textGO.AddComponent<TextMeshProUGUI>();
            timerText.alignment = TextAlignmentOptions.Center;
            timerText.fontSize = 36;
            timerText.color = Color.yellow;
            timerText.text = "";

            var textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0f, 0f);
            textRT.anchorMax = new Vector2(1f, 1f);
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            canvas.enabled = false;
            ApplySafeArea();
        }

        void OnRectTransformDimensionsChange() => ApplySafeArea();

        void ApplySafeArea()
        {
            Rect sa = Screen.safeArea;
            float saT = useSafeArea ? (1f - (sa.yMax / Mathf.Max(1f, (float)Screen.height))) * referenceResolution.y : 0f;
            panelRT.anchoredPosition = new Vector2(0f, -(20f + saT));
        }

        string FormatTime(float seconds)
        {
            int total = Mathf.CeilToInt(seconds);
            if (total < 0) total = 0;
            int m = total / 60;
            int s = total % 60;
            return $"{m}:{s:00}";
        }

        public void ShowTimer(float secondsLeft)
        {
            canvas.enabled = true;
            UpdateTimer(secondsLeft);
        }

        public void UpdateTimer(float secondsLeft)
        {
            if (!timerText) return;
            timerText.text = "TIME " + FormatTime(secondsLeft);
        }

        public void HideTimer() { canvas.enabled = false; }
    }
}
