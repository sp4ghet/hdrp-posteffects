#ifndef SP4GHET_GAUSSIAN_BLUR
#define SP4GHET_GAUSSIAN_BLUR

#define INV_SQRT_TWO_PI 0.3989422804 // 1 / sqrt(TAU)

#ifndef UNITY_COMMON_INCLUDED
  // This pleases the IDE
  #define SHADER_API_D3D11
  #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
  #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
  #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
  #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
  #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"
#endif

// Material properties
TEXTURE2D_X(_MainTex);
TEXTURE2D(_Source);
int _BlurSize;
float _SigmaSquared;


#include "Packages/com.sp4ghet.hdrp-posteffects/Runtime/ShaderIncludes/ShaderLib/Common.hlsl"
#include "Packages/com.sp4ghet.hdrp-posteffects/Runtime/ShaderIncludes/ShaderLib/Noise.hlsl"

float3 SampleBuffer(float2 uv){
  return SAMPLE_TEXTURE2D(_Source, s_linear_clamp_sampler, uv).xyz;
}

float gaussKern(int x, float sigmaSquared){
  return  (1.0f / sigmaSquared) * INV_SQRT_TWO_PI * exp(-0.5*x*x / sigmaSquared);
}

float gauss(float x, float y, float sigmaSquared)
{
  return  (1.0f / sigmaSquared) * INV_SQRT_TWO_PI * exp(-0.5 * (x * x + y * y) / sigmaSquared);
}

#define LUMINANCE(col) (dot(col, float3(0.2126 ,0.7152, 0.0722)))

float3 Blur(float2 uv, int blurSize){
  float onePixelX = 1.0 / _ScreenSize.x;
  float onePixelY = 1.0 / _ScreenSize.y;
  float3 c = SampleColor(uv);
  float lum = saturate(LUMINANCE(c)) + 0.01;
  if(blurSize <= 0){
    return c;
  }

  float ns = cnoise(uv);
  ns = ns*ns;

  float weight = 0;
  float sum = 0;
  float3 val = float3(0,0,0);
  // blurSize = n -> (n+1)x(n+1) kernel
  [loop]
  for (int x = -blurSize; x <= blurSize; ++x){
  [loop]
  for (int y = -blurSize; y <= blurSize; ++y)
  {
    float2 offsetUV = uv + float2(onePixelX * x, onePixelY * y);
    if(all(saturate(offsetUV) != offsetUV)){
      continue;
    }
    weight = gauss(x, y, _SigmaSquared * ns);
    val += SampleColor(offsetUV) * weight;
    sum += weight;
    }
  }

  return (val / sum);
}

float3 BlurX(float2 uv, int blurSize){
  float onePixel = 1.0 / _ScreenSize.x;
  float3 c = SampleColor(uv);
  if(blurSize <= 0){
    return c;
  }

  float ns = cnoise(uv);
  ns = ns*ns;

  float weight = 0;
  float sum = 0;
  float3 val = float3(0,0,0);
  // blurSize = 1 => 3x3
  [loop]
  for(int i=-blurSize; i<blurSize; i++){
    float2 offsetUV = uv + float2(onePixel * i, 0);
    if(all(saturate(offsetUV) != offsetUV)){
      continue;
    }
    weight = gaussKern(i, _SigmaSquared * ns);
    val += SampleColor(offsetUV) * weight;
    sum += weight;
  }

  return (val / sum);
}

float3 BlurY(float2 uv, int blurSize){
  float onePixel = 1.0 / _ScreenSize.y;
  float3 c = SampleColor(uv);
  if(blurSize <= 0){
    return c;
  }
  float ns = cnoise(uv);
  ns = ns*ns;

  float weight = 0;
  float sum = 0;
  float3 val = float3(0,0,0);
  // blurSize = 1 => 3x3
  [loop]
  for(int i=-blurSize; i<blurSize; i++){
    float2 offsetUV = uv + float2(0, onePixel * i);
    if(all(saturate(offsetUV) != offsetUV)){
      continue;
    }
    weight = gaussKern(i, _SigmaSquared * ns);
    val += SampleBuffer(offsetUV) * weight;
    sum += weight;
  }
  return (val / sum);
}

#endif //SP4GHET_GAUSSIAN_BLUR
