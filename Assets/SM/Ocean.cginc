#include "UnityCG.cginc"
sampler2D _MainTex;


float3 GetN(float2 uv, float t)
{
    
     
    float3 N = (tex2D(_MainTex, uv * .655 + float2(.1 + t * .955 * -.6 * .5,.6 + -t * .955 * .4)).xyz * 2 - 1) * .5 +
               (tex2D(_MainTex, uv * .64 + float2(.2 + -t* .64 * .8 * 1.2 * .5,.5 + -t* .64 * .9 * -1.2)).xyz * 2 - 1)  * .5 +
               (tex2D(_MainTex, uv * .334 + float2(.3 + t* .334 * .8 * 1.2 * .5,.4 + -t* .334 * .9* 1.2)).xyz * 2 - 1)  * .5 +
               (tex2D(_MainTex, uv * 1.534 + float2(.4 + -t* 1.534 * .8 * -1.2 * .5,.3 + -t* 1.534 * .3* -1.2)).xyz * 2 - 1)  * .5 +
               (tex2D(_MainTex, uv * .412 + float2(.5 + t * .412 * -.5 * .4 * .5,.2 + -t * 1.412 * .25 * 1.4)).xyz * 2 - 1) * .5 +
               (tex2D(_MainTex, uv * .7 + float2(.6 + t* .7 * .65 * 1.6 * .5,.1 + -t* .7 * .45 * 1.6)).xyz * 2 - 1) * .5;
                
                N.y *=-1;
                
    return normalize(N);
}