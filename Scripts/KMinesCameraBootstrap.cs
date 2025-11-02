using System.Collections;
using UnityEngine;

namespace KMines
{
    /// <summary>
    /// Ligger i scenen från start.
    /// Väntar på Board + Main Camera och kopplar på mobil-fittern.
    /// Här tvingar vi också kameran att visa ALLA lager (cullingMask = Everything)
    /// och att titta rakt ner – annars får vi bara bakplattan på Android.
    /// </summary>
    [DefaultExecutionOrder(-900)]
    public class KMinesCameraBootstrap : MonoBehaviour
    {
        [Tooltip("Extra padding runt brädet (world units).")]
        public float padding = 0.25f;

        IEnumerator Start()
        {
            Board board = null;
            Camera cam = null;

            // 1) vänta in board
            while (board == null)
            {
                board = FindObjectOfType<Board>();
                yield return null;
            }

            // 2) vänta tills board verkligen byggt klart (grid != null)
            while (board.grid == null)
                yield return null;

            // 3) vänta in kamera (kan skapas i runtime på Android)
            while (cam == null)
            {
                cam = Camera.main;
                yield return null;
            }

            // 4) gör mobil-fit
            MobileBoardCameraFitter.EnsureOnMainCamera(board, padding);

            // 🔴 kritiskt för Android: visa ALLT och titta nedåt
            cam.cullingMask = ~0;                  // Everything
            cam.orthographic = true;
            cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            if (cam.transform.position.y < 1f)
                cam.transform.position = new Vector3(board.transform.position.x, 10f, board.transform.position.z);

            // 5) refitta efter en liten stund ifall UI hann ändra kameran
            yield return new WaitForSeconds(0.4f);
            MobileBoardCameraFitter.EnsureOnMainCamera(board, padding);
            cam.cullingMask = ~0;
        }
    }
}
