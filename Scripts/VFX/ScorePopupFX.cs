using UnityEngine;
using System.Collections;

namespace KMines
{
    // Flytande "+1000" popup vid poäng
    public class ScorePopupFX : MonoBehaviour
    {
        int amount;
        TextMesh tm;

        const float LIFE = 0.5f;
        const float START_SCALE = 0.02f;
        const float END_SCALE   = 0.05f;

        public static void Spawn(Vector3 worldPos, int pts)
        {
            var go = new GameObject("ScorePopupFX");
            go.transform.position = worldPos + new Vector3(0f, 0.15f, 0f);
            go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            var fx = go.AddComponent<ScorePopupFX>();
            fx.Init(pts);
        }

        void Init(int pts)
        {
            amount = pts;

            tm = gameObject.AddComponent<TextMesh>();
            tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tm.fontSize = 200;
            tm.fontStyle = FontStyle.Bold;
            tm.characterSize = 1.0f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.text = "+" + amount.ToString();

            var mr = tm.GetComponent<MeshRenderer>();
            mr.material.renderQueue = 4000;
            mr.material.color = new Color(1f, 0.9f, 0.1f, 1f);

            transform.localScale = Vector3.one * START_SCALE;

            StartCoroutine(Co());
        }

        IEnumerator Co()
        {
            float t = 0f;

            Vector3 p0 = transform.position;
            Vector3 p1 = p0 + new Vector3(0f, 0.4f, 0f);

            Color c0 = new Color(1f, 0.9f, 0.1f, 1f);
            Color c1 = new Color(1f, 0.9f, 0.1f, 0f);

            while (t < LIFE)
            {
                t += Time.deltaTime;
                float k = t / LIFE;
                if (k > 1f) k = 1f;

                transform.position = Vector3.Lerp(p0, p1, k);

                float s = Mathf.Lerp(START_SCALE, END_SCALE, k);
                transform.localScale = Vector3.one * s;

                if (tm != null)
                {
                    var mr = tm.GetComponent<MeshRenderer>();
                    if (mr != null && mr.material != null)
                    {
                        mr.material.color = Color.Lerp(c0, c1, k);
                    }
                }

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
