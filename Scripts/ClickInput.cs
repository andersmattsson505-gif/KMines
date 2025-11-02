using UnityEngine;

namespace KMines
{
    public class ClickInput : MonoBehaviour
    {
        public Board target;
        public Camera cam;
        public float longPressSeconds = 1.0f; // mobil
        [Tooltip("Hur mycket vi får missa med fingret (px)")]
        public float touchTolerancePx = 28f;

        float touchStart = -1f;
        bool longPressTriggered;

        void Awake()
        {
            if (cam == null) cam = Camera.main;
            if (target == null) target = FindObjectOfType<Board>();
        }

        void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            DesktopInput();
#else
            MobileInput();
#endif
        }

        void DesktopInput()
        {
            if (target == null || cam == null) return;

            // Högerklick = flagga
            if (Input.GetMouseButtonDown(1))
            {
                if (RayToBoardPointExpanded(Input.mousePosition, 0f, out var wp))
                    target.ToggleFlagAt(wp);
                return;
            }

            // Vänsterklick = reveal ELLER missil om armerad
            if (Input.GetMouseButtonDown(0))
            {
                if (RayToBoardPointExpanded(Input.mousePosition, 0f, out var wp))
                {
                    if (target.IsMissileArmed())
                        target.UseMissileAt(wp);
                    else
                        target.ClickAt(wp);
                }
            }
        }

        void MobileInput()
        {
            if (target == null || cam == null) return;
            if (Input.touchCount == 0) { touchStart = -1f; longPressTriggered = false; return; }

            var t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
            {
                touchStart = Time.time;
                longPressTriggered = false;
            }
            else if ((t.phase == TouchPhase.Stationary || t.phase == TouchPhase.Moved) && !longPressTriggered)
            {
                if (touchStart > 0f && (Time.time - touchStart) >= longPressSeconds)
                {
                    if (RayToBoardPointExpanded(t.position, touchTolerancePx, out var wp))
                        target.ToggleFlagAt(wp);
                    longPressTriggered = true;
                }
            }
            else if (t.phase == TouchPhase.Ended)
            {
                if (!longPressTriggered)
                {
                    if (RayToBoardPointExpanded(t.position, touchTolerancePx, out var wp))
                    {
                        if (target.IsMissileArmed())
                            target.UseMissileAt(wp);
                        else
                            target.ClickAt(wp);
                    }
                }
                touchStart = -1f;
                longPressTriggered = false;
            }
        }

        // försöker mittpunkten först, sen 8 kringliggande punkter
        bool RayToBoardPointExpanded(Vector2 screenPos, float padPx, out Vector3 worldPoint)
        {
            if (RayToBoardPoint(screenPos, out worldPoint))
                return true;

            if (padPx <= 0f) return false;

            // 8 runtom
            Vector2[] offs =
            {
                new Vector2(+padPx, 0),
                new Vector2(-padPx, 0),
                new Vector2(0, +padPx),
                new Vector2(0, -padPx),
                new Vector2(+padPx, +padPx),
                new Vector2(-padPx, +padPx),
                new Vector2(+padPx, -padPx),
                new Vector2(-padPx, -padPx),
            };

            foreach (var o in offs)
            {
                if (RayToBoardPoint(screenPos + o, out worldPoint))
                    return true;
            }

            return false;
        }

        bool RayToBoardPoint(Vector2 screenPos, out Vector3 worldPoint)
        {
            worldPoint = default;
            if (cam == null) return false;

            var ray = cam.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out var hit, 1000f))
            {
                worldPoint = hit.point;
                return true;
            }
            return false;
        }
    }
}
