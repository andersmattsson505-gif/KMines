using UnityEngine;

namespace KMines
{
    /// <summary>
    /// Mobil-kamerafit för KMines.
    /// Ser till att hela brädet syns, att kameran är ortho och tittar rakt ner.
    /// </summary>
    [DisallowMultipleComponent]
    public class MobileBoardCameraFitter : MonoBehaviour
    {
        public Camera cam;
        public Board board;

        [Header("World padding")]
        public float padding = 0.25f;

        [Header("Viewport")]
        public float viewportShrink = 0.9f;
        public float portraitAspectThreshold = 0.8f;

        public float maxOrtho = 20f;
        public bool updateEveryFrame = true;

        void Reset()
        {
            if (cam == null) cam = Camera.main;
            if (board == null) board = FindObjectOfType<Board>();
        }

        void Start() => FitNow();

        void LateUpdate()
        {
            if (updateEveryFrame)
                FitNow();
        }

        public void FitNow()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null || board == null) return;

            float w = board.width * board.tileSize + padding * 2f;
            float h = board.height * board.tileSize + padding * 2f;

            float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);

            float sizeByHeight = (h * 0.5f) / Mathf.Max(0.01f, viewportShrink);
            float sizeByWidth = ((w * 0.5f) / Mathf.Max(0.01f, viewportShrink)) / Mathf.Max(0.01f, aspect);

            float finalSize = (aspect < portraitAspectThreshold)
                ? sizeByHeight
                : Mathf.Max(sizeByHeight, sizeByWidth);

            finalSize = Mathf.Min(finalSize, maxOrtho);

            cam.orthographic = true;
            cam.orthographicSize = finalSize;
            cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            // centrera
            var bp = board.transform.position;
            var cp = cam.transform.position;
            cam.transform.position = new Vector3(bp.x, cp.y < 1f ? 10f : cp.y, bp.z);
        }

        public static void EnsureOnMainCamera(Board b, float pad)
        {
            var cam = Camera.main;
            if (cam == null || b == null) return;

            var fitter = cam.GetComponent<MobileBoardCameraFitter>();
            if (fitter == null)
                fitter = cam.gameObject.AddComponent<MobileBoardCameraFitter>();

            fitter.board = b;
            fitter.padding = pad;
            fitter.FitNow();
        }
    }
}
