
using UnityEngine;

namespace KMines
{
    [DisallowMultipleComponent]
    public class Cell : MonoBehaviour
    {
        public int x, y;
        public bool hasMine;
        public int near;
        public bool opened { get; private set; }

        Board owner;

        MeshRenderer mr;
        Collider col;
        Material mat;
        Texture2D texClosed, texOpen;

        SpriteRenderer ringSR;
        GameObject textGO;
        SpriteRenderer mineSR;

        const float DIGIT_SCALE = 0.0092f;
        const float RING_SCALE  = 0.54f;
        const float MINE_SCALE  = 0.60f;
        const float LIFT_RING_Y = 0.050f;
        const float LIFT_TEXT_Y = 0.065f;

        public void Init(Board owner,
                         int cx, int cy,
                         bool mine,
                         int nearCount,
                         float tileSize,
                         Texture2D tClosed,
                         Texture2D tOpen)
        {
            this.owner = owner;
            x = cx;
            y = cy;
            hasMine = mine;
            near = nearCount;
            texClosed = tClosed;
            texOpen   = tOpen;

            mr  = GetComponent<MeshRenderer>();
            col = GetComponent<Collider>();

            // 1) välj en shader som ALLTID finns i build
            Shader sh = Shader.Find("Unlit/Texture");
            if (sh == null) sh = Shader.Find("Sprites/Default");
            if (sh == null) sh = Shader.Find("UI/Default");

            mat = new Material(sh);
            if (texClosed != null)
            {
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", texClosed);
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", texClosed);
            }
            mr.sharedMaterial = mat;
            mr.enabled = true;

            // 🔺 Rita efter bakgrunden
            mr.sharedMaterial.renderQueue = 2450;

            // 🔺 Se till att den ligger på samma layer som board (mobila kameran kan culla custom layers)
            if (owner != null)
                gameObject.layer = owner.gameObject.layer;
            else
                gameObject.layer = 0; // Default

            // större träffyta
            BoxCollider box = col as BoxCollider;
            if (box == null) box = gameObject.AddComponent<BoxCollider>();
            box.size   = new Vector3(1.35f, 0.4f, 1.35f);
            box.center = Vector3.zero;

            // ring
            var ringTex = Resources.Load<Texture2D>("Art/ring_white");
            if (ringTex != null)
            {
                var ringGO = new GameObject("Ring");
                ringGO.transform.SetParent(transform, false);
                ringGO.transform.localPosition = new Vector3(0f, LIFT_RING_Y, 0f);
                ringGO.transform.localScale    = Vector3.one * RING_SCALE;

                ringSR = ringGO.AddComponent<SpriteRenderer>();
                ringSR.sprite  = Sprite.Create(ringTex, new Rect(0,0,ringTex.width, ringTex.height), new Vector2(0.5f,0.5f), 512f);
                ringSR.material = new Material(Shader.Find("Sprites/Default"));
                ringSR.sortingOrder = 110;
                ringSR.enabled = false;
            }

            // text
            textGO = new GameObject("Digit", typeof(TextMesh));
            textGO.transform.SetParent(transform, false);
            textGO.transform.localPosition = new Vector3(0f, LIFT_TEXT_Y, 0f);
            textGO.transform.localScale    = Vector3.one * DIGIT_SCALE;
            var tm = textGO.GetComponent<TextMesh>();
            tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tm.fontSize = 360;
            tm.fontStyle = FontStyle.Bold;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            textGO.SetActive(false);

            // mina
            var mineTex = Resources.Load<Texture2D>("Art/mine_red");
            if (mineTex != null)
            {
                var mineGO = new GameObject("MineIcon");
                mineGO.transform.SetParent(transform, false);
                mineGO.transform.localPosition = new Vector3(0f, LIFT_RING_Y, 0f);
                mineGO.transform.localScale    = Vector3.one * MINE_SCALE;

                mineSR = mineGO.AddComponent<SpriteRenderer>();
                mineSR.sprite = Sprite.Create(mineTex, new Rect(0,0,mineTex.width, mineTex.height), new Vector2(0.5f,0.5f), 512f);
                mineSR.material = new Material(Shader.Find("Sprites/Default"));
                mineSR.sortingOrder = 200;
                mineSR.enabled = false;
            }
        }

        public void Open(bool isMine)
        {
            if (opened) return;
            opened = true;

            if (isMine)
            {
                if (col) col.enabled = false;
                if (textGO) textGO.SetActive(false);
                if (ringSR) ringSR.enabled = false;
                if (mineSR) mineSR.enabled = true;
                return;
            }

            if (near == 0)
            {
                if (mr) mr.enabled = false;
                if (col) col.enabled = false;
                if (textGO) textGO.SetActive(false);
                if (ringSR) ringSR.enabled = false;
                if (mineSR) mineSR.enabled = false;
            }
            else
            {
                if (mr) mr.enabled = true;
                if (col) col.enabled = true;

                var c = Palette.ForCount(near);

                if (ringSR)
                {
                    ringSR.color = new Color(c.r, c.g, c.b, 1f);
                    ringSR.enabled = true;
                }

                if (textGO)
                {
                    var tm = textGO.GetComponent<TextMesh>();
                    tm.text = near.ToString();
                    tm.color = c;
                    textGO.SetActive(true);
                }
            }
        }
    }
}
