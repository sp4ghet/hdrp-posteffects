#ifndef SP4GHET_COMMON
#define SP4GHET_COMMON

float3 SampleColor(float2 uv){
  return SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, ClampAndScaleUVForBilinearPostProcessTexture(uv)).xyz;
}

float2 uv2pt(float2 uv){

  float2 pt = (uv - .5) * 2.;
  pt.y *= _ScreenParams.y / _ScreenParams.x;
  return pt;
}

float2 pt2uv(float2 pt){
  pt.y *= _ScreenParams.x / _ScreenParams.y;
  pt += 1.;
  pt *= 0.5;
  return pt;
}

float2x2 r2d(float t){
  float c = cos(t), s = sin(t);
  return float2x2(c,s, -s, c);
}

inline float Linear01Depth( float z )
{
    return 1.0 / (_ZBufferParams.x * z + _ZBufferParams.y);
}

#define LUMINANCE(col) (dot(col, float3(0.2126 ,0.7152, 0.0722)))

#endif
