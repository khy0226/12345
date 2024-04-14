void AdditionalLights_float(float3 WorldPosition, float3 WorldNormal, float3 WorldView, out float3 Direction, out float3 Color, out float DistanceAtten, out float ShadowAtten)
{
    float3 direction = 0;
    float3 color = 0;
    float distance = 0;
    float shadow = 0;

#ifndef SHADERGRAPH_PREVIEW
    WorldNormal = normalize(WorldNormal);
    WorldView = SafeNormalize(WorldView);
    int pixelLightCount = GetAdditionalLightsCount();
    for (int i = 0; i < pixelLightCount; ++i)
    {
        Light light = GetAdditionalLight(i, WorldPosition);
//        half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
//        diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal);
        direction = light.direction;
        color = light.color;
        distance = light.distanceAttenuation;
        shadow = light.shadowAttenuation;
    }
#endif

    Direction = direction;
    Color = color;
    DistanceAtten = distance;
    ShadowAtten = shadow;
}

void AdditionalLights_half(half3 WorldPosition, half3 WorldNormal, half3 WorldView, out half3 Direction, out half3 Color, out half DistanceAtten, out half ShadowAtten)
{
    half3 direction = 0;
    half3 color = 0;
    half distance = 0;
    half shadow = 0;

#ifndef SHADERGRAPH_PREVIEW
    WorldNormal = normalize(WorldNormal);
    WorldView = SafeNormalize(WorldView);
    int pixelLightCount = GetAdditionalLightsCount();
    for (int i = 0; i < pixelLightCount; ++i)
    {
        Light light = GetAdditionalLight(i, WorldPosition);
//        half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
//        diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal);
        direction = light.direction;
        color = light.color;
        distance = light.distanceAttenuation;
        shadow = light.shadowAttenuation;
    }
#endif

    Direction = direction;
    Color = color;
    DistanceAtten = distance;
    ShadowAtten = shadow;
}