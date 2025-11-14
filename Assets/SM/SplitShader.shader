Shader "Unlit/SplitShader"
{
    Properties
    {
        _MainTex  ("Texture",  2D) = "white" {}
        _InfraRed ("InfraRed", 2D) = "white" {}
        _CalcMap ("CalcMap", 2D) = "white" {}
        _HeatRamp ("HeatRamp", 2D) = "white" {}
        _Water    ("Water",    2D) = "white" {}
        _Ramp     ("Ramp",     2D) = "white" {}
        _Outline ("Outline",     2D) = "white" {}
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
            #pragma multi_compile SHOWMAP_OFF SHOWMAP_ON
            #include "UnityCG.cginc"
            #include "Window.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 wPos : TEXCOORD1;
            };


            sampler2D _MainTex, _InfraRed, _HeatRamp, _Water, _Ramp, _CalcMap, _Outline;
            float Vis;

            v2f vert (appdata v)
            {
                float lr = floor(v.uv.z);
                
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = float4(v.uv);
                o.wPos   = float4(v.vertex.xyz, lr);
                
                return o;
            }
            
            
            fixed4 frag (v2f i) : SV_Target
            {
                //return float4(step(.1, 1 - tex2D(_Water, i.uv.xy).x), 0, tex2D(_CalcMap, i.uv.xy).x, 1);
            
                float2 uv         = i.uv.xy;
                float2 animuv     = uv - sin(_Time.y * 5 + uv.y * 150) * .0005 * .75;
                float  water      = tex2D(_Water, animuv).x;
                float  waterMask  = step(.5, water);
                float2 wateruv    = uv + sin(_Time.y * 9 + uv.y * 150) * .00075 * .5 * (1 - waterMask);
                fixed4 col        = tex2D(_MainTex, wateruv);
                
                float windowmask = GetWindowMask(i.wPos.xyz);
                
                float4 watercolor = tex2D(_Ramp, water);
                float alpha = watercolor.a * (1 - windowmask * .15);
                
                #if SHOWMAP_ON
                fixed4 red        = tex2D(_CalcMap, wateruv);
                return float4(lerp(red.xyz, watercolor.xyz * alpha, alpha), 1);
                #else
                fixed4 red        = tex2D(_InfraRed, wateruv);
                #endif
                
                
            //  Result  //
                float3 result = lerp(col, red, windowmask * red.a).xyz;
                
                result += tex2D(_Outline, fmod(i.uv.xy * 2, 1)).a;
                
            //  Debug Stripes  //
                //result += step(.5, fmod(uv.y * 10, 1)) * .015 + (step(.5, fmod(uv.y * 2, 1)) * .9 - .1) * .075;
                //result += step(.5, fmod(uv.x * 10, 1)) * .015 + (step(.5, fmod(uv.x * 2, 1)) * .9 - .1) * .075;
               
            //  Add Shadow  //
                float shadow = tex2D(_Water, animuv + float2(0, .0015)).x; 
                      shadow = pow(1 - tex2D(_Ramp, max(0, (shadow - .2))).a, 3);
                      shadow = saturate(shadow + (1 - waterMask));
                      shadow = shadow * (1 - windowmask * .25);
                      shadow = (.65 + .35 * shadow) * (.35 + .65 * waterMask);
                      
                //result *= shadow;
                
            //  Add Water  //
               
                result = lerp(result, watercolor.xyz * alpha, alpha);
                
            //  Add Border  //
                float2 uv2    = float2(fmod(i.uv.z, 1), fmod(i.uv.w, 1)) * 2;
               
                return float4(result, Vis);
            }
            ENDCG
        }
    }
}
