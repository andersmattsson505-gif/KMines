using UnityEngine;

namespace KMines
{
    public class CameraShaker : MonoBehaviour
    {
        public static CameraShaker Instance;
        public Camera cam;
        public float defaultDuration = 0.08f;
        public float defaultMagnitude = 0.15f;

        Vector3 basePos;
        float t, mag;

        void Awake()
        {
            Instance = this;
            if (!cam) cam = Camera.main;
            if (cam) basePos = cam.transform.localPosition;
        }

        void Update()
        {
            if (!cam) return;
            if (t > 0f)
            {
                t -= Time.deltaTime;
                Vector2 r = Random.insideUnitCircle * mag;
                cam.transform.localPosition = basePos + new Vector3(r.x, r.y, 0f);
                if (t <= 0f) cam.transform.localPosition = basePos;
            }
        }

        public static void Shake(float? duration = null, float? magnitude = null)
        {
            if (Instance == null || Instance.cam == null) return;
            Instance.t = duration ?? Instance.defaultDuration;
            Instance.mag = magnitude ?? Instance.defaultMagnitude;
        }
    }
}