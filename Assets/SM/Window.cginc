float3 window;

float rectangle(float2 samplePosition, float2 halfSize)
{
    float2 componentWiseEdgeDistance = abs(samplePosition) - halfSize;
    float  outsideDistance           = length(max(componentWiseEdgeDistance, 0));
    float  insideDistance            = min(max(componentWiseEdgeDistance.x, componentWiseEdgeDistance.y), 0);
    return outsideDistance + insideDistance;
}


float GetWindowMask(float3 wPos)
{
    return saturate(step(rectangle((wPos.xy - window.xy), float2(1.23375, .855) * window.z * .85), .04));
}