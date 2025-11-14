Shader "Unlit/Oil"
{
    Properties
    {
        _MainTex ("WaveNormal", 2D) = "white" {}
        _Drawings ("Drawings", 2D) = "white" {}
        _Oil ("Oil", 2D) = "black" {}
        _Eraser ("Eraser", 2D) = "black" {}
        _ShipA ("ShipA", 2D) = "white" {}
        _ShipB ("ShipB", 2D) = "white" {}
        _Dark ("Dark", COLOR) = (1,1,1,1)
        _Light ("Light", COLOR) = (1,1,1,1)
        _Outline ("Outline", COLOR) = (1,1,1,1)
        _OutlineGood ("OutlineGood", COLOR) = (1,1,1,1)
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

            #include "Ocean.cginc"
            #include "GridPBR.cginc"
            #include "Window.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 oceanuv : TEXCOORD2;
                float4 vertex : SV_POSITION;
                float3 wPos : TEXCOORD1;
            };

            sampler2D _Drawings, _Oil, _Eraser, _ShipA, _ShipB;
            float Offset;
            float Vis;
            float4 _Outline, _OutlineGood, _Dark, _Light;
            float4 shipPos;
            
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                float2 uv = v.uv * 7;
                       uv.x *= .75;
                       uv.y *= -1;
                       uv += Offset;
                
                o.oceanuv = uv;
                o.uv = v.uv;
                o.wPos = v.vertex;
                return o;
            }
            

            fixed4 frag (v2f i) : SV_Target
            {
                float3 wPos = i.wPos;
                
                float windowmask = GetWindowMask(wPos);
               
            
                float3 N = GetN(i.oceanuv, _Time.y * .04 + Offset);
                
                float3 L = normalize(float3(.5, 1.4, 4.5));
                float3 V = normalize(float3(0, 0, 100) - wPos);
                float dtl2 = dot(V, N);
                
                float2 duv = i.uv + N.xy * -.04;
                float4 sub = tex2D(_Drawings, duv) * pow(dtl2, 20);
           
                float4 oil = tex2D(_Oil, duv) * pow(dtl2, 20);
                float d = dot(N, L);
                
            //  Tanker  //
                float2 offsetA = shipPos.xy;//float2(-.3, .3);
                float2 offsetB = shipPos.zw;//float2(.1, -.2);
                float scale = 2.25;
                float2 uvA = (i.uv -.5 + offsetA) * (scale + sin(_Time.y * 2) * .0155) + .5 + float2(1, 1.2) * sin(_Time.y * .4643 + .511) * .002;
                float2 uvB = (i.uv -.5 + offsetB) * (scale + sin(_Time.y * 2 + .5) * .0155) + .5 + float2(1, 1.2) * sin(_Time.y * .4643 + 1.2511) * .002;
                float4 tankerA =  tex2D(_ShipA, uvA);
                float4 tankerB =  tex2D(_ShipB, uvB);
                       tankerA = float4(tankerA.xyz * tankerA.a + tankerB.xyz * tankerB.a, tankerA.a + tankerB.a);
                       tankerA.xyz = pow(tankerA.xyz, 1.3);
                
                float shipMask = (1 - windowmask * .6) * (1 - windowmask * (sin(i.uv.y*1400 + _Time.y *20)*.5+ .5) * .35);
                       
                float tankerAS = (tex2D(_ShipA, uvA + float2(.08, .15) * .1 + N.xy * -.04).a + tex2D(_ShipB, uvB + float2(.08, .15) * .1 + N.xy * -.04).a) * shipMask;
                       
                       
                float3 result = PBR((.125).xxx, wPos, N, L, V, float3(dtl2, .025, .5), d, d, d) * (pow(saturate(d), 4) * .975 + .025);
                
                       result = lerp(_Dark, _Light, result.x) * (pow(1 - oil.y, 20) * .5 + .5) + result.x * .75 * (1 - tankerAS * .25);
                       result *= 1 - tankerAS * .25;
                       result = pow(result, 1.5);
                       result *= 1 + (1 - pow(dtl2, 200)) * .5;
                       result = lerp(result, float4(.1, .6, .3, 1) * (sin(pow(oil.x, 2) * 300 - _Time.y * 2) * .15 + .85), (oil.z * .25 + .75 * (1 - pow(1 - oil.x, 70)) * 1) * windowmask * .45);
                       
                       result = lerp(result, _OutlineGood, max( pow(1.0 - saturate(sub.z * 1.25 - .0125), 200), sub.w * (.4 + .15 * pow(sin((duv.y - duv.x) * 130 + _Time.y * -2) * .5 + .5, 4)) ) * .25);
                       result = lerp(result, _Outline,     max( pow(1.0 - saturate(sub.x * 1.25 - .0125), 200), sub.y * (.4 + .15 * pow(sin((duv.y + duv.x) * 130 + _Time.y * -2) * .5 + .5, 4)) ) * .25);
                       
                          
                          sub = tex2D(_Drawings, i.uv + N.xy * .0014);
                          
                       result = lerp(result, _OutlineGood, (pow(1.0 - saturate(sub.z * 1.25 - .0125), 200)) * .25);   
                       result = lerp(result, _Outline,     (pow(1.0 - saturate(sub.x * 1.25 - .0125), 200)) * .25);
                       result = pow(result, 1.75) * 1.75;
                   
                float e = 1.0 - pow(1.0 - pow(tex2D(_Eraser, duv).x * pow(dtl2, 20), 20), 2); 
                       result = lerp(result, 1, e * .7);
                       result = lerp(result, tankerA.xyz, tankerA.a * shipMask);
                     
                return float4(result, Vis);  
                return lerp(float4(result, Vis), float4((windowmask).xxx, 1), .5);
            }
            ENDCG
        }
    }
}
