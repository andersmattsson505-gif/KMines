Shader "KMines/VisorPulseRadial"
{
    Properties
    {
        _Color("Tint", Color) = (1,1,1,1)
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

        // klassisk alpha-blend
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
                // Gör en cirkulär mask baserat på avstånd från mitten
                float2 centered = i.uv - 0.5;      // -0.5..0.5
                float dist = length(centered) * 2; // 0 i mitten, ~1 ute vid kanterna
                float falloff = saturate(1.0 - dist);

                // falloff = 1 i mitten, 0 i kanten
                // multipla med färgens alpha (vi kommer ändra alpha via script)
                float a = falloff * _Color.a;

                return fixed4(_Color.rgb, a);
            }
            ENDCG
        }
    }
}
