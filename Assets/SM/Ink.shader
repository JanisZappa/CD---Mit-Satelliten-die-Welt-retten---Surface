Shader "Unlit/Ink"
{
    Properties
    {
        _Water ("Water", 2D) = "white" {}
        _Ramp ("Ramp", 2D) = "white" {}
        _Fields ("Fields", 2D) = "white" {}
        _Blur ("Blur", 2D) = "white" {}
        _Clouds("Clouds", 2D) = "white" {}
        _Heat("HeatMask", 2D) = "white" {}
        _HeatRamp("HeatRamp", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
                float2 uv     : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _Water, _Ramp, _Fields, _Blur, _Clouds, _Heat, _HeatRamp;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
            
                float2 animuv = uv - sin(_Time.y * 5 + uv.y * 150) * .0005;
                float u = tex2D(_Water, animuv).x;
                
                float s = tex2D(_Water, animuv + float2(0, .0015)).x; 
                float4 ink = tex2D(_Ramp, u);
                s = pow(1 - tex2D(_Ramp, max(0, (s - .2))).a, 3);
                
                float mask = step(.5, u);
                s = saturate(s + (1 - mask));
                
                float4 c  = tex2D(_Fields, uv);
               
                float anim = (sin(_Time.y  * - 3 + u * 4) * .5 + .5) * .4 * (1.0 - mask) * pow(u, 1);
                
                float4 c2 = tex2D(_Blur, uv + sin(_Time.y * 9 + uv.y * 150) * .00075 * (1 + anim * 3)) * .25;
                
                
                
                c = lerp(c2, c, saturate(mask + anim * 2) * .6 + .4);
                c.xyz *= .35 + .65 * s;
                
                float a = ink.a - anim * .4;
                
                float cloud = (1 - pow(1 - tex2D(_Clouds, i.uv * .55 + float2(_Time.y * .02, _Time.y * .002)).x, 4)) * .65 + .55;
               
                float  heat = tex2D(_Heat, uv).x;
                float4 heatColor = tex2D(_HeatRamp, heat);
                
                return float4(lerp(c.xyz, ink.xyz * a, a) * lerp(cloud, 1, a), 1);
            }
            ENDCG
        }
    }
}
