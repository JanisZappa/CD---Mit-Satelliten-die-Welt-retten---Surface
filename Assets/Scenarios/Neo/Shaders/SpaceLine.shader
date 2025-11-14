Shader "Custom/SpaceLine"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                half2 texcoord  : TEXCOORD0;
                float4 sPos  : TEXCOORD1;
            };

            sampler2D _MainTex;
            float Swipe;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex   = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.sPos     = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 texcol = tex2D(_MainTex, i.texcoord);
                float2 uv = i.sPos.xy / i.sPos.w;

                float s = Swipe;
                     // s = .5;
                //texcol.a *= step(uv.x, s);
                float multi = 30;
                float range = 1.0 / multi;
                texcol.a *= 1.0 - pow(saturate(max(0, (uv.x + range) - s) * multi), 2);
                return texcol;
            }
            ENDCG
        }
    }
}