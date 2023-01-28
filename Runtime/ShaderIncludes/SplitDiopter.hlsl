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

TEXTURE2D_X(_MainTex);
TEXTURE2D(_BlurTexture);
float _Intensity;
float _Angle;
float _Speed;
float _ShiftSize;
float2 _ShiftOverride;
int _ShiftFbmOctaves;
float _CutOffSharpness;
float2 _CutOffCenter;

#include "Packages/com.sp4ghet.hdrp-posteffects/Runtime/ShaderIncludes/ShaderLib/Common.hlsl"


float3 SampleBlurred(float2 uv){
  return SAMPLE_TEXTURE2D(_BlurTexture, s_linear_clamp_sampler, uv).xyz;
}

float rand(float n){return frac(sin(n) * 43758.5453123);}

float2 noise(float p){
	float id = floor(p);
  float t = frac(p);
  float p0 = rand(id);
  float p1 = rand(id + 1);
  float u = t*t*t*(t*(t*6.-15.)+10.);
  float v = smoothstep(0,1,t);
  return float2(lerp(p0, p1, u), lerp(p0,p1, v));
}

float fbm(float p){
  float ret = 0;
  float weight = 1;
  [loop]
  for(int i=0; i<_ShiftFbmOctaves; i++){
    ret += noise(exp2(i) * p).x / exp2(i);
    weight += 1.0 / exp2(i);
  }
  return ret / weight;
}

float3 SplitDiopter(float2 uv){
    float3 c = SampleColor(uv);

    float2 pt = uv2pt(uv),
           qt = uv2pt(uv);

    float scale = 5.0;
    float noiseTime = _Time.x * _Speed;
    float nsx = fbm(noiseTime) - 0.25;
    float nsy = fbm(noiseTime + 30) - 0.25;
    pt *= 0.9; //scale up so edges hopefully don't show up
    float2 offset = float2(nsx,nsy) * _ShiftSize;
    offset -= _ShiftOverride;
    pt += offset;
    uv = pt2uv(pt);

    float3 blur =  SampleBlurred(uv);
    float mask = saturate(LUMINANCE(blur));

    pt = qt;
    pt -= _CutOffCenter;
    pt = mul(r2d(_Angle), pt);
    mask *= saturate(exp(pt.x * _CutOffSharpness));

    c = lerp(c, blur, mask * _Intensity);

    return c;
}

#endif //#ifndef SP4GHET_SPLIT_DIOPTER
