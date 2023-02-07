#ifndef SP4GHET_HDRP_COMMON
#define SP4GHET_HDRP_COMMON

#ifndef UNITY_COMMON_INCLUDED
  // This pleases the IDE
  #define SHADER_API_D3D11
  #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
  #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
  #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
  #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
  #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"

  TEXTURE2D_X(_MainTex);
#endif

float3 SampleColor(float2 uv){
  return SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, ClampAndScaleUVForBilinearPostProcessTexture(uv)).xyz;
}


#endif
