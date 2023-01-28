#ifndef SP4GHET_SPLIT_DIOPTER
#define SP4GHET_SPLIT_DIOPTER

#ifndef UNITY_COMMON_INCLUDED
  // This pleases the IDE
  #define SHADER_API_D3D11
  #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

  #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
  #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
  #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
  #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"
#endif

float _Intensity;
float _Angle;
float _Speed;
float _ShiftSize;
float2 _ShiftOverride;
float _CutOffSharpness;
float2 _CutOffCenter;
TEXTURE2D_X(_MainTex);
TEXTURE2D(_BlurTexture);

#include "Packages/com.sp4ghet.hdrp-posteffects/Runtime/ShaderIncludes/ShaderLib/Common.hlsl"


float3 SampleBlurred(float2 uv){
  return SAMPLE_TEXTURE2D(_BlurTexture, s_linear_clamp_sampler, uv).xyz;
}


float3 SplitDiopter(float2 uv){
    float3 c = SampleColor(uv);

    float2 pt = uv2pt(uv);

    float scale = 5.0;
    float noiseTime = _Time.x * _Speed;
    float ns = 0.75 * sin(noiseTime + 0.3) + 0.25 * cos(noiseTime * 2);
    uv += mul(r2d(noiseTime * 4 + ns), float2(1,0)) * _ShiftSize;
    uv -= _ShiftOverride;

    float3 blur =  SampleBlurred(uv);
    float mask = saturate(LUMINANCE(blur));

    pt -= _CutOffCenter;
    pt = mul(r2d(_Angle), pt);
    mask *= saturate(exp(pt.x * _CutOffSharpness));

    c = lerp(c, blur, mask * _Intensity);

    return c;
}

#endif //#ifndef SP4GHET_SPLIT_DIOPTER
