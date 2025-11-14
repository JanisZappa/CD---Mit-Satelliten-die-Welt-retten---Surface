float3 Sun, Earth;

float Extra()
{
    float dist = length(_WorldSpaceCameraPos - Earth);
    return 1 - saturate((dist - 518) / (684.0 - 518));
}