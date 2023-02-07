#ifndef SP4GHET_URP_COMMON
#define SP4GHET_URP_COMMON

#ifndef UNITY_COMMON_INCLUDED
  // This pleases the IDE
  #define SHADER_API_D3D11
  #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
  #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#endif

float3 SampleColor(float2 uv){
  return float3(0,0,0);
}


#endif
