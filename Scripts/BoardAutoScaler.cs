using UnityEngine;

namespace KMines
{
    /// <summary>
    /// Skalar brädet efter kamerans faktiska bredd i runtime.
    /// Poängen: vi låter kameran vara, men gör brädet större/mindre så att
    /// det får plats snyggt även på smala telefoner.
    ///
    /// Viktigt: den här kör i LateUpdate och kan alltså skriva över
    /// vad BoardViewportFitter gjort tidigare i framen.
    /// </summary>
    [DefaultExecutionOrder(200)] // körs efter Board/Boot/Fitter
    public class BoardAutoScaler : MonoBehaviour
    {
        [Tooltip("Om tomt letar vi upp Board med FindObjectOfType varje frame tills vi hittar den.")]
        public Board board;

        [Tooltip("Om tomt tar vi Camera.main")]
        public Camera targetCamera;

        [Range(0.5f, 1.1f)]
        [Tooltip("Hur stor del av kamera-bredden brädet får ta (0.80 ≈ 80%)")]
        public float screenFill = 0.80f;   // SÄNKT från 0.92f för mobil

        [Tooltip("Kör varje frame (true) eller bara första gången (false).")]
        public bool continuous = true;

        // vi väntar ofta 1–2 frames eftersom Board bygger sig i Start()
        int warmupFrames = 4;

        void LateUpdate()
        {
            if (warmupFrames > 0)
            {
                warmupFrames--;
                TryApply();
                return;
            }

            TryApply();

            if (!continuous)
                enabled = false;
        }

        void TryApply()
        {
            if (board == null)
                board = FindObjectOfType<Board>();
            if (board == null)
                return;

            if (targetCamera == null)
                targetCamera = Camera.main;
            if (targetCamera == null)
                return;

            if (!targetCamera.orthographic)
                return;

            // kamera-bredd i world units
            float camHeight = targetCamera.orthographicSize * 2f;
            float camWidth = camHeight * targetCamera.aspect;

            // brädets world-bredd före skalning
            float boardBaseWidth = board.width * board.tileSize;
            if (boardBaseWidth <= Mathf.Epsilon)
                return;

            // clamp så vi inte råkar gå över 0.9 på mobil
            float fill = Mathf.Clamp(screenFill, 0.5f, 0.9f);

            float wantedWidth = camWidth * fill;
            float scale = wantedWidth / boardBaseWidth;

            // applicera på X och Z (Z för att behålla proportionen)
            var t = board.transform;
            var s = t.localScale;
            s.x = scale;
            s.z = scale;
            t.localScale = s;

            // se också till att den står centrerad på X
            t.position = new Vector3(0f, t.position.y, t.position.z);
        }
    }
}
