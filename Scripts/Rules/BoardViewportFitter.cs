
using UnityEngine;

namespace KMines
{
    [DisallowMultipleComponent]
    public class BoardViewportFitter : MonoBehaviour
    {
        [Header("Refs")]
        public Camera cam;
        public Board board;

        [Header("UI margins (px @ 1080x1920)")]
        public float uiTopPx = 96f;
        public float uiBottomPx = 0f;
        public float uiLeftPx = 120f;
        public float uiRightPx = 120f;

        [Header("Reference resolution")]
        public Vector2 referenceResolution = new Vector2(1080f, 1920f);

        [Header("Options")]
        public bool includeSafeArea = true;
        public float extraWorldPadding = 0.25f;
        public bool updateEveryFrame = true;

        [Header("Desktop/mobile")]
        [Tooltip("Shrink available space so board never touches edges.")]
        public float viewportShrink = 0.9f;

        [Tooltip("Below this aspect we treat as portrait and fit by height.")]
        public float portraitAspectThreshold = 0.8f;

        public float maxOrtho = 20f;

        void Reset()
        {
            cam = Camera.main;
            if (!board) board = FindObjectOfType<Board>();
        }

        void Start() => FitNow();

        void LateUpdate()
        {
            if (updateEveryFrame) FitNow();
        }

        public void FitNow()
        {
            if (!cam || !board) return;

            float W = Mathf.Max(0.001f, board.width * board.tileSize);
            float H = Mathf.Max(0.001f, board.height * board.tileSize);

            float fTop = uiTopPx / Mathf.Max(1f, referenceResolution.y);
            float fBottom = uiBottomPx / Mathf.Max(1f, referenceResolution.y);
            float fLeft = uiLeftPx / Mathf.Max(1f, referenceResolution.x);
            float fRight = uiRightPx / Mathf.Max(1f, referenceResolution.x);

            if (includeSafeArea)
            {
                Rect sa = Screen.safeArea;
                float sw = Mathf.Max(1f, (float)Screen.width);
                float sh = Mathf.Max(1f, (float)Screen.height);
                fLeft += sa.xMin / sw;
                fRight += 1f - (sa.xMax / sw);
                fBottom += sa.yMin / sh;
                fTop += 1f - (sa.yMax / sh);
            }

            float availFracW = Mathf.Clamp01(1f - fLeft - fRight);
            float availFracH = Mathf.Clamp01(1f - fTop - fBottom);

            // shrink a bit so board doesn't hug the sides
            availFracW *= viewportShrink;
            availFracH *= viewportShrink;

            float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);

            float sizeByHeight = ((H * 0.5f) + extraWorldPadding) / Mathf.Max(0.01f, availFracH);
            float sizeByWidth = (((W * 0.5f) + extraWorldPadding) / Mathf.Max(0.01f, availFracW)) / Mathf.Max(0.01f, aspect);

            float finalSize;
            if (aspect < portraitAspectThreshold)
                finalSize = sizeByHeight;
            else
                finalSize = Mathf.Max(sizeByHeight, sizeByWidth);

            finalSize = Mathf.Min(finalSize, maxOrtho);

            cam.orthographicSize = finalSize;

            // center camera over board
            var p = cam.transform.position;
            var bp = board.transform.position;
            cam.transform.position = new Vector3(bp.x, p.y, bp.z);
        }
    }
}
