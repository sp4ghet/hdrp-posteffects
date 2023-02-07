#ifndef SP4GHET_COMMON
#define SP4GHET_COMMON

float3 chooseUp(float3 v) {
    //  See : http://lolengine.net/blog/2013/09/21/picking-orthogonal-vector-combing-coconuts
    return abs(v.x) > abs(v.z) ? float3(-v.y, v.x, 0.0)  : float3(0.0, -v.z, v.y);
}

float3x3 getOrthogonalBasis(float3 lookAt){
    lookAt = normalize(lookAt);
    float3 o1 = normalize(chooseUp(lookAt));
    float3 o2 = normalize(cross(o1, lookAt));
    return float3x3(o1, o2, lookAt);
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

#define LUMINANCE(col) (dot(col, float3(0.2126 ,0.7152, 0.0722)))

#endif
