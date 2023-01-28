#ifndef SP4GHET_MIRAGE
#define SP4GHET_MIRAGE

#ifndef UNITY_COMMON_INCLUDED
  // This pleases the IDE
  #define SHADER_API_D3D11
  #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

  #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
  #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
  #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
  #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"
#endif

// List of properties to control your post process effect
float _Intensity;
float _YCenter;
float _CNoiseScale;
float _PNoiseScale;
float _Narrowness;
float _Updraft;
float _WigglePace;
float _DepthMin;
float _DepthMax;
int _EnableReflect;
TEXTURE2D_X(_MainTex);
TEXTURE2D_X(_DepthTexture);

#include "Packages/com.sp4ghet.hdrp-posteffects/Runtime/ShaderIncludes/ShaderLib/Common.hlsl"
#include "Packages/com.sp4ghet.hdrp-posteffects/Runtime/ShaderIncludes/ShaderLib/Noise.hlsl"

float3 world_pos(float2 uv){
  float depth = SampleCameraDepth(uv);
  // depth = Linear01Depth(depth);
  #if UNITY_REVERSED_Z
    // 0~1
  #else
    // Adjust z to match NDC for OpenGL
    depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, depth);
  #endif

  return ComputeWorldSpacePosition(uv, depth, UNITY_MATRIX_I_VP) + _WorldSpaceCameraPos.xyz;
}

float3 mirage(float2 uv, float yCenter, float amplitude, float CNoiseScale, float PNoiseScale, float narrowness, float updraft, float wiggle_pace, float depth_min, float depth_max, bool enable_reflect){
    float3 c = SampleColor(uv);
    float2 pt = uv2pt(uv);
    float amp = amplitude;
    float res = CNoiseScale;
    float res2 = PNoiseScale;


    float time = _Time.y;
    pt.y -= time * 0.1 * updraft;
    float t = cnoise(float3(uv*30, time));
    float2 new_uv = uv + 0.01 * amp * float2(
        noise(float3(pt * res * 300, wiggle_pace * t)) + 0.5 * noise(float3(pt * res * 300, wiggle_pace * 2.3 * t)) + cnoise(float3(res2 * 60 * pt, 0)),
        noise(float3(pt * res * 300, wiggle_pace * t-30)) + 0.5 * noise(float3(pt * res * 300, wiggle_pace * 2.3 * t - 30)) + cnoise(float3(res2 * 60 * pt, 15)));

    float3 worldOld = world_pos(uv);
    float3 worldNew = world_pos(new_uv);
    float worldY = worldOld.y;
    float newWorldY = worldNew.y;
    float mixer = max(abs(worldY - yCenter), abs(newWorldY - yCenter));
    mixer = exp(-1 * narrowness * mixer * mixer);
    if(enable_reflect){
      mixer = uv.y - yCenter;
      mixer = exp(-100 * narrowness * mixer * mixer);

      new_uv.y = new_uv.y < yCenter ? 2*yCenter-new_uv.y : new_uv.y;
    }

    float depth = length(worldOld - _WorldSpaceCameraPos);
    depth = length(worldNew - _WorldSpaceCameraPos);
    mixer *= step(depth_min, depth);
    mixer *= step(depth, depth_max);
    c = lerp(c, SampleColor(new_uv), mixer);
    // c = float3(mixer, mixer, mixer);
    return c;
}
#endif
