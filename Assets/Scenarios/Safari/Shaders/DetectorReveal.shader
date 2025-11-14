Shader "Safari/DetectorReveal"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" }
        LOD 100
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float2 wPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float2 _CarPos;
            float _Radius;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.wPos = mul(unity_ObjectToWorld, v.vertex).xy;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                float dist = 1 - pow(saturate((length(i.wPos - _CarPos) - _Radius) * 2), 2);
                
                return float4(col.xyz, col.a * dist);
            }
            ENDCG
        }
    }
}
