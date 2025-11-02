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

        [Tooltip("Below this aspect we treat as portrait and fit by height. 0.3 = phones måste också ta hänsyn till bredd.")]
        public float portraitAspectThreshold = 0.3f;

        public float maxOrtho = 20f;

        [Header("Offsets")]
        [Tooltip("Positive value pushes board DOWN on screen (camera centers a bit higher).")]
        public float verticalBoardOffsetWorld = 0f;

        void Reset()
        {
            cam = Camera.main;
            if (!board) board = FindObjectOfType<Board>();

            if (board && verticalBoardOffsetWorld == 0f)
                verticalBoardOffsetWorld = board.tileSize * 0.5f;
        }

        void Start() => FitNow();

        void LateUpdate()
        {
            if (updateEveryFrame)
                FitNow();
        }

        public void FitNow()
        {
            if (!cam || !board) return;

            // board i world
            float W = Mathf.Max(0.001f, board.width * board.tileSize);
            float H = Mathf.Max(0.001f, board.height * board.tileSize);

            // UI → fraktioner
            float fTop = uiTopPx / Mathf.Max(1f, referenceResolution.y);
            float fBottom = uiBottomPx / Mathf.Max(1f, referenceResolution.y);
            float fLeft = uiLeftPx / Mathf.Max(1f, referenceResolution.x);
            float fRight = uiRightPx / Mathf.Max(1f, referenceResolution.x);

            // safe area?
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

            // hur mycket av skärmen vi får använda
            float availFracW = Mathf.Clamp01(1f - fLeft - fRight);
            float availFracH = Mathf.Clamp01(1f - fTop - fBottom);

            // liten säkerhetsmarginal
            availFracW *= viewportShrink;
            availFracH *= viewportShrink;

            float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);

            // “fit by height”
            float sizeByHeight = ((H * 0.5f) + extraWorldPadding) / Mathf.Max(0.01f, availFracH);

            // “fit by width”
            float sizeByWidth = (((W * 0.5f) + extraWorldPadding) / Mathf.Max(0.01f, availFracW))
                                / Mathf.Max(0.01f, aspect);

            float finalSize;
            // FÖRR: if (aspect < 0.8f) → fit by height ⇒ klipper sidorna på mobil
            // NU: vi sänker till 0.3f så mobiler också får ta hänsyn till bredd
            if (aspect < portraitAspectThreshold)
                finalSize = sizeByHeight;
            else
                finalSize = Mathf.Max(sizeByHeight, sizeByWidth);

            finalSize = Mathf.Min(finalSize, maxOrtho);
            cam.orthographicSize = finalSize;

            // centrera kameran över brädet + vårt offset
            var p = cam.transform.position;
            var bp = board.transform.position;

            float off = verticalBoardOffsetWorld;
            if (off == 0f && board)
                off = board.tileSize * 0.5f; // fallback

            cam.transform.position = new Vector3(bp.x, p.y, bp.z + off);
        }
    }
}
