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

        [Header("Offsets")]
        public float verticalBoardOffsetWorld = 0f;

        void LateUpdate()
        {
            if (updateEveryFrame)
                FitNow();
        }

        public void FitNow()
        {
            if (cam == null || board == null)
                return;

            // board size in world
            float W = board.width * board.tileSize;
            float H = board.height * board.tileSize;

            // screen size (px)
            float scrW = Screen.width;
            float scrH = Screen.height;

            // UI -> i pixlar @ referens
            float scaleX = scrW / referenceResolution.x;
            float scaleY = scrH / referenceResolution.y;

            float topPx = uiTopPx * scaleY;
            float bottomPx = uiBottomPx * scaleY;
            float leftPx = uiLeftPx * scaleX;
            float rightPx = uiRightPx * scaleX;

#if UNITY_ANDROID || UNITY_IOS
            if (includeSafeArea)
            {
                var sa = Screen.safeArea;
                // minska med det som redan är borta från safe area
                float lostLeft = sa.xMin;
                float lostRight = scrW - sa.xMax;
                float lostTop = scrH - sa.yMax;
                float lostBottom = sa.yMin;

                leftPx += lostLeft;
                rightPx += lostRight;
                topPx += lostTop;
                bottomPx += lostBottom;
            }
#endif

            // available fraction (0..1)
            float availFracW = 1f - ((leftPx + rightPx) / Mathf.Max(1f, scrW));
            float availFracH = 1f - ((topPx + bottomPx) / Mathf.Max(1f, scrH));

            // shrink a bit so board doesn't hug the sides
            availFracW *= viewportShrink;
            availFracH *= viewportShrink;

            float aspect = (float)scrW / Mathf.Max(1, scrH);

            // how big ortho must be to fit height
            float sizeByHeight = ((H * 0.5f) + extraWorldPadding) / Mathf.Max(0.01f, availFracH);

            // how big ortho must be to fit width
            float sizeByWidth = (((W * 0.5f) + extraWorldPadding) / Mathf.Max(0.01f, availFracW)) / Mathf.Max(0.01f, aspect);

            // VIKTIGT: mobiler med smalt format ska också ta MAX
            float finalSize = Mathf.Max(sizeByHeight, sizeByWidth);

            // clamp
            finalSize = Mathf.Min(finalSize, maxOrtho);
            cam.orthographicSize = finalSize;

            // center camera over board + our offset
            var p = cam.transform.position;
            var bp = board.transform.position;

            float off = verticalBoardOffsetWorld;
            if (off == 0f && board)
                off = board.tileSize * 0.5f; // fallback: en halv ruta

            cam.transform.position = new Vector3(bp.x, p.y, bp.z + off);
        }
    }
}
