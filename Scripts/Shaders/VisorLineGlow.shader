Shader "KMines/VisorLineGlow"
{
    Properties
    {
        _Color("Tint", Color) = (0.2,1,1,0.4)
    }

    SubShader
    {
        Tags {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
        }
        LOD 100

        Cull Off
        ZWrite Off
        Lighting Off
        Fog { Mode Off }

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // i.uv.y går 0..1 över quadens "tjocklek".
                // vi vill ha starkast i mitten (0.5) och mjuk fade mot kanterna.
                float d = abs(i.uv.y - 0.5) * 2.0; // 0 i mitten, ~1 vid kanten
                float falloff = saturate(1.0 - d); // 1 i mitten -> 0 i kanten

                // gör den ännu mjukare med en kvadratkurva
                falloff = falloff * falloff;

                fixed4 col = _Color;
                col.a *= falloff;

                return col;
            }
            ENDCG
        }
    }
}
