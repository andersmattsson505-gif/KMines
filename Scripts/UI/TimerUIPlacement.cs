using UnityEngine;

namespace KMines
{
    [RequireComponent(typeof(Canvas))]
    public class TimerUIPlacement : MonoBehaviour
    {
        public int sortOrder = 31000;
        public bool forceOverlay = true;

        void OnEnable()
        {
            var canvas = GetComponent<Canvas>();
            if (forceOverlay) canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
        }
    }
}
