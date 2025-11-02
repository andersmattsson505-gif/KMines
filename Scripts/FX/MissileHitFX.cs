using UnityEngine;
using System.Collections;

namespace KMines
{
    /// <summary>
    /// Enkel "platt" missile-hit-effekt som ritar en sprite ovanpå brädet och tonar ut den.
    /// Nu bakåtkompatibel:
    /// - Board kan kalla fx.SpawnAt(...)
    /// - äldre kod kan kalla MissileHitFX.Spawn(...)
    /// </summary>
    public class MissileHitFX : MonoBehaviour
    {
        // ---------------------------------------------
        // GAMMALT API (static) – används i gamla scener
        // ---------------------------------------------
        public static void Spawn(Vector3 worldPos)
        {
            var go = new GameObject("MissileHitFX");
            go.transform.position = worldPos + new Vector3(0f, 0.02f, 0f);
            var fx = go.AddComponent<MissileHitFX>();
            fx.Run();
        }

        // ---------------------------------------------
        // NYTT API (instance) – Board ropar hit
        // ---------------------------------------------
        public void SpawnAt(Vector3 worldPos)
        {
            // flytta det här existerande FX-objektet dit och kör
            transform.position = worldPos + new Vector3(0f, 0.02f, 0f);
            Run();
        }

        void Run()
        {
            StartCoroutine(Co());
        }

        IEnumerator Co()
        {
            // hur länge effekten syns
            float dur = 0.25f;

            // child-objekt för själva ikonen
            var iconGO = new GameObject("FXIcon");
            iconGO.transform.SetParent(transform, false);
            iconGO.transform.rotation = Quaternion.Euler(90, 0, 0); // platt mot brädet
            iconGO.transform.localScale = Vector3.one * 0.35f;

            // ladda minans sprite
            // vi försöker först en Sprite direkt, annars bygger vi en Sprite av en Texture2D
            Sprite spr = Resources.Load<Sprite>("Art/mine_red");
            if (spr == null)
            {
                var tex = Resources.Load<Texture2D>("Art/mine_red");
                if (tex != null)
                {
                    spr = Sprite.Create(
                        tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f),
                        tex.width
                    );
                }
            }

            var sr = iconGO.AddComponent<SpriteRenderer>();
            if (spr != null)
                sr.sprite = spr;

            // högt sortingOrder så den hamnar över cellgrafiken
            sr.sortingOrder = 200;

            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = t / dur;

                // skala upp över tid
                float s = Mathf.Lerp(0.35f, 0.7f, k);
                iconGO.transform.localScale = Vector3.one * s;

                // tona ut alpha över tid
                float a = Mathf.Lerp(0.95f, 0f, k);

                var c = sr.color;
                c.a = a;
                sr.color = c;

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
