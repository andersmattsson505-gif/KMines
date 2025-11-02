using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KMines
{
    /// <summary>
    /// Visor-scan med sprite-baserad glow.
    /// Läser boardens skala så svepet följer BoardAutoScaler.
    /// </summary>
    public class VisorScanEffect : MonoBehaviour
    {
        public Board board;

        [Header("Ping (mina-blipp)")]
        public float pulseTime = 1.5f;
        [Range(0f, 1f)] public float maxAlpha = 0.7f;
        public float startScale = 0.4f;
        public float endScale = 1.4f;
        public float overlayScale = 0.6f;
        public Color pulseColor = Color.white;

        [Header("Radar")]
        public float radarSweepTime = 2.0f;
        public Color radarCoreColor = new Color(0.4f, 1f, 1f, 1f);
        public Color radarGlowColor = new Color(0.2f, 1f, 1f, 0.4f);
        [Range(0.02f, 1f)] public float radarLineThickness = 0.15f;
        [Range(0f, 1f)] public float radarPulseAlphaStrength = 0.5f;
        [Range(0f, 1f)] public float radarThicknessPulseStrength = 0.25f;
        public float radarPulseSpeed = 8f;

        [Header("Trail")]
        public float trailLife = 0.7f;
        public float trailInterval = 0.015f;
        [Range(0f, 1f)] public float trailStartAlphaFactor = 0.6f;
        public float trailThicknessStartMul = 3f;
        public float trailThicknessEndMul = 1.5f;

        [Header("Width")]
        [Tooltip("1.0 = lika brett som brädet, >1 = bredare")]
        public float extraWidthMul = 3.9f;   // <-- din siffra

        Coroutine activeRadarRoutine;

        static Sprite sRadialSprite;
        static Sprite sLineSprite;

        const int SORT_TRAIL = 400;
        const int SORT_GLOW = 410;
        const int SORT_CORE = 420;
        const int SORT_PING = 430;

        public void PulseRevealMines()
        {
            if (!EnsureBoard()) return;
            EnsureSprites();

            for (int y = 0; y < board.height; y++)
            {
                for (int x = 0; x < board.width; x++)
                {
                    var cell = board.grid[x, y];
                    if (cell == null || !cell.hasMine) continue;
                    StartCoroutine(SpawnAndPulseAtCell(cell));
                }
            }
        }

        public void PulseRadarSweep()
        {
            if (!EnsureBoard()) return;
            EnsureSprites();

            if (activeRadarRoutine != null)
            {
                StopCoroutine(activeRadarRoutine);
                activeRadarRoutine = null;
            }
            activeRadarRoutine = StartCoroutine(RadarSweepRoutine());
        }

        bool EnsureBoard()
        {
            if (board == null) board = FindObjectOfType<Board>();
            return board != null;
        }

        static void EnsureSprites()
        {
            if (sRadialSprite == null) sRadialSprite = MakeRadialSprite(256);
            if (sLineSprite == null) sLineSprite = MakeLineSprite(64, 256);
        }

        static Sprite MakeRadialSprite(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            int c = size / 2;
            float rMax = c;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = x - c, dy = y - c;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float t = Mathf.Clamp01(1f - (r / rMax));
                    t = t * t * (3f - 2f * t);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, t));
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        static Sprite MakeLineSprite(int w, int h)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            float c = (h - 1) * 0.5f;
            for (int y = 0; y < h; y++)
            {
                float d = Mathf.Abs((y - c) / c);
                float t = Mathf.Clamp01(1f - d);
                t = t * t * (3f - 2f * t);
                for (int x = 0; x < w; x++)
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, t));
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), h);
        }

        IEnumerator SpawnAndPulseAtCell(Cell cell)
        {
            var go = new GameObject("VisorPulseFX");
            go.transform.SetParent(cell.transform, false);
            go.transform.localPosition = new Vector3(0f, 0.055f, 0f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sRadialSprite;
            sr.sortingOrder = SORT_PING;
            sr.color = new Color(pulseColor.r, pulseColor.g, pulseColor.b, 0f);

            float t = 0f;
            while (t < pulseTime)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / pulseTime);

                float s = Mathf.Lerp(startScale, endScale, k) * overlayScale;
                go.transform.localScale = Vector3.one * s;

                float peak = 1f - Mathf.Abs((k * 2f) - 1f);
                float a = peak * maxAlpha;
                sr.color = new Color(pulseColor.r, pulseColor.g, pulseColor.b, a);

                yield return null;
            }

            Destroy(go);
        }

        IEnumerator SpawnTrailSlice(float zPos, float totalW)
        {
            var slice = new GameObject("RadarTrailSlice");
            slice.transform.SetParent(board.transform, false);
            slice.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            slice.transform.localPosition = new Vector3(0f, 0f, zPos);

            var sr = slice.AddComponent<SpriteRenderer>();
            sr.sprite = sLineSprite;
            sr.sortingOrder = SORT_TRAIL;

            Color cStart = radarGlowColor; cStart.a *= trailStartAlphaFactor;
            sr.color = cStart;

            float startThick = radarLineThickness * trailThicknessStartMul;
            float endThick = radarLineThickness * trailThicknessEndMul;

            float t = 0f;
            while (t < trailLife)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / trailLife);

                float thick = Mathf.Lerp(startThick, endThick, k);
                float a = Mathf.Lerp(cStart.a, 0f, k);

                slice.transform.localScale = new Vector3(totalW, thick, 1f);
                var cNow = sr.color; cNow.a = a; sr.color = cNow;

                yield return null;
            }

            Destroy(slice);
        }

        IEnumerator RadarSweepRoutine()
        {
            float sx = board.transform.localScale.x;
            float sz = board.transform.localScale.z;

            float tileX = board.tileSize * sx;
            float tileZ = board.tileSize * sz;

            float halfW = (board.width - 1) * tileX * 0.5f;
            float halfH = (board.height - 1) * tileZ * 0.5f;

            float totalW = (halfW * 2f) * extraWidthMul;

            float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
            if (aspect < 0.6f)
                totalW *= 1.15f;

            var parent = new GameObject("RadarLine");
            parent.transform.SetParent(board.transform, false);

            var core = new GameObject("Core");
            core.transform.SetParent(parent.transform, false);
            core.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var coreSR = core.AddComponent<SpriteRenderer>();
            coreSR.sprite = sLineSprite;
            coreSR.sortingOrder = SORT_CORE;

            var glow = new GameObject("Glow");
            glow.transform.SetParent(parent.transform, false);
            glow.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var glowSR = glow.AddComponent<SpriteRenderer>();
            glowSR.sprite = sLineSprite;
            glowSR.sortingOrder = SORT_GLOW;

            float startZ = halfH + tileZ * 0.5f;
            float endZ = -halfH - tileZ * 0.5f;

            var pinged = new HashSet<Cell>();
            float elapsed = 0f;
            float trailTimer = 0f;

            while (elapsed < radarSweepTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / radarSweepTime);
                float zNow = Mathf.Lerp(startZ, endZ, t);

                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * radarPulseSpeed);
                float thickGlow = Mathf.Lerp(
                    radarLineThickness * (1f - radarThicknessPulseStrength),
                    radarLineThickness,
                    pulse
                );
                float alphaGlow = Mathf.Lerp(
                    radarGlowColor.a * (1f - radarPulseAlphaStrength),
                    radarGlowColor.a,
                    pulse
                );

                float coreThickness = radarLineThickness * 0.4f;

                core.transform.localScale = new Vector3(totalW, coreThickness, 1f);
                glow.transform.localScale = new Vector3(totalW, thickGlow, 1f);

                core.transform.localPosition = new Vector3(0f, 0f, zNow);
                glow.transform.localPosition = new Vector3(0f, 0f, zNow);

                var cc = radarCoreColor; cc.a = 1f; coreSR.color = cc;
                var cg = radarGlowColor; cg.a = alphaGlow; glowSR.color = cg;

                trailTimer += Time.deltaTime;
                if (trailTimer >= trailInterval)
                {
                    trailTimer = 0f;
                    StartCoroutine(SpawnTrailSlice(zNow, totalW));
                }

                for (int y = 0; y < board.height; y++)
                {
                    for (int x = 0; x < board.width; x++)
                    {
                        var cell = board.grid[x, y];
                        if (cell == null || !cell.hasMine || pinged.Contains(cell)) continue;
                        float cellZ = cell.transform.localPosition.z * sz;
                        if (Mathf.Abs(cellZ - zNow) <= board.tileSize * sz * 0.5f)
                        {
                            pinged.Add(cell);
                            StartCoroutine(SpawnAndPulseAtCell(cell));
                        }
                    }
                }

                yield return null;
            }

            Destroy(parent);
            activeRadarRoutine = null;
        }
    }
}
