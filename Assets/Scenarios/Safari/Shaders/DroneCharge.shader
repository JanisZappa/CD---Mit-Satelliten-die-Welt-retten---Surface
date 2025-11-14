Shader "Safari/DroneCharge"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" }
        ZWrite Off
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
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            float battery;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv - .5;
                float l = length(uv);
                float d = 1 - max(saturate((l - .465) * 100), 1 - saturate((l - .395) * 100));
                
                uv = normalize(uv);
                float a = fmod(atan2(uv.y, uv.x) / (3.14159265359 * 2) + .5 + .25, 1);
                
                float charge = (sin(_Time.y) * .5 + .5);
                      charge = 1 - battery;
                      charge = charge * 1.1 - .05;
                      a = 1 - saturate((charge - a) * 1000);
                
                return float4(i.color.xyz, d * a * i.color.a);
            }
            ENDCG
        }
    }
}
