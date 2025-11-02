using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace KMines
{
    [DisallowMultipleComponent]
    public class SmartClickInput : MonoBehaviour
    {
        [Header("Refs")]
        public Camera cam;
        public Board target;
        public WinLoseManager rules;

        [Header("Raycast")]
        public float rayDistance = 200f;

        [Header("Touch")]
        public float longPressTime = 0.55f;
        [Tooltip("Hur mycket vi får missa med fingret (px)")]
        public float touchTolerancePx = 28f;

        bool isPressing;
        float pressTimer;
        bool longPressFired;
        Vector2 pressScreenPos;

        void Awake()
        {
            if (cam == null) cam = Camera.main;
            if (target == null) target = FindObjectOfType<Board>();
            if (rules == null) rules = FindObjectOfType<WinLoseManager>();
        }

        void Update()
        {
#if ENABLE_INPUT_SYSTEM
            HandleNewInputSystem();
#else
            HandleLegacyMouseOnly();
#endif
        }

#if ENABLE_INPUT_SYSTEM
        void HandleNewInputSystem()
        {
            var mouse = Mouse.current;
            var touch = Touchscreen.current;

            bool touchDown = touch != null && touch.primaryTouch.press.isPressed;
            bool mouseDown = mouse != null && mouse.leftButton.isPressed;

            // START PRESS
            if (!isPressing && (touchDown || (mouse != null && mouse.leftButton.wasPressedThisFrame)))
            {
                // UI-block – klicka inte igenom på HUD/missil/restart
                if (IsPointerOverUI(touchDown ? (int?)touch.primaryTouch.touchId.ReadValue() : null))
                    return;

                isPressing = true;
                pressTimer = 0f;
                longPressFired = false;
                pressScreenPos = touchDown ? touch.primaryTouch.position.ReadValue() : mouse.position.ReadValue();
                return;
            }

            if (isPressing)
            {
                pressTimer += Time.deltaTime;

                // long press -> flag
                if (!longPressFired && pressTimer >= longPressTime)
                {
                    longPressFired = true;
                    DoFlagAt(pressScreenPos);
                }

                bool stillDown = touchDown || mouseDown;
                if (!stillDown)
                {
                    if (!longPressFired)
                        DoPrimaryAt(pressScreenPos);

                    isPressing = false;
                    pressTimer = 0f;
                    longPressFired = false;
                }
            }
        }
#endif

        void HandleLegacyMouseOnly()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // UI-block
                if (IsPointerOverUI(null))
                    return;

                isPressing = true;
                pressTimer = 0f;
                longPressFired = false;
                pressScreenPos = Input.mousePosition;
            }

            if (isPressing)
            {
                pressTimer += Time.deltaTime;

                if (!longPressFired && pressTimer >= longPressTime)
                {
                    longPressFired = true;
                    DoFlagAt(pressScreenPos);
                }

                if (Input.GetMouseButtonUp(0))
                {
                    if (!longPressFired)
                        DoPrimaryAt(pressScreenPos);

                    isPressing = false;
                    pressTimer = 0f;
                    longPressFired = false;
                }
            }
        }

        bool IsPointerOverUI(int? touchId)
        {
            if (EventSystem.current == null)
                return false;

            if (touchId.HasValue)
                return EventSystem.current.IsPointerOverGameObject(touchId.Value);

            return EventSystem.current.IsPointerOverGameObject();
        }

        void DoPrimaryAt(Vector2 screenPos)
        {
            if (cam == null || target == null) return;

            // extra-säker: blocka även här om vi ändå träffar UI
            if (IsPointerOverUI(null)) return;

            if (!ScreenToBoardHitExpanded(screenPos, touchTolerancePx, out Vector3 hitPos))
                return;

            if (target.IsMissileArmed())
            {
                target.UseMissileAt(hitPos);
                if (rules != null) rules.BeginMissileGrace(0.25f);
            }
            else
            {
                target.ClickAt(hitPos);
            }
        }

        void DoFlagAt(Vector2 screenPos)
        {
            if (cam == null || target == null) return;
            if (!ScreenToBoardHitExpanded(screenPos, touchTolerancePx, out Vector3 hitPos))
                return;
            target.ToggleFlagAt(hitPos);
        }

        bool ScreenToBoardHitExpanded(Vector2 screenPos, float padPx, out Vector3 hitPos)
        {
            if (ScreenToBoardHit(screenPos, out hitPos))
                return true;

            if (padPx <= 0f)
                return false;

            Vector2[] offs =
            {
                new Vector2(+padPx, 0), new Vector2(-padPx, 0),
                new Vector2(0, +padPx), new Vector2(0, -padPx),
                new Vector2(+padPx, +padPx), new Vector2(-padPx, +padPx),
                new Vector2(+padPx, -padPx), new Vector2(-padPx, -padPx),
            };

            foreach (var o in offs)
            {
                if (ScreenToBoardHit(screenPos + o, out hitPos))
                    return true;
            }

            return false;
        }

        bool ScreenToBoardHit(Vector2 screenPos, out Vector3 hitPos)
        {
            hitPos = Vector3.zero;
            if (cam == null) return false;

            Ray ray = cam.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
            {
                hitPos = hit.point;
                return true;
            }
            return false;
        }
    }
}
