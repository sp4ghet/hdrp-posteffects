Shader "Hidden/Shader/URP/SplitDiopter"
{
	Properties
	{
	    _MainTex ("Main Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags {
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100
        ZWrite Off
        Cull Off
        ZTest Always
        Blend Off

		Pass
		{
            Name "SplitDiopter"
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            #if SHADER_API_GLES
                float4 pos = input.positionOS;
                float2 uv  = input.uv;
            #else
                float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
                float2 uv  = GetFullScreenTriangleTexCoord(input.vertexID);
            #endif

                output.positionCS = pos;
                output.texcoord   = uv;
                return output;
            }

			#pragma vertex vert
			#pragma fragment frag

            #include "Packages/com.sp4ghet.posteffects/ShaderIncludes/ShaderLib/Common.hlsl"
            #include "Packages/com.sp4ghet.posteffects/ShaderIncludes/ShaderLib/Noise.hlsl"

            TEXTURE2D_X(_CameraOpaqueTexture);
            TEXTURE2D_X(_MainTex);
            TEXTURE2D_X(_BlurTexture);
            SAMPLER(sampler_CameraOpaqueTexture);
            SAMPLER(sampler_MainTex);
            SAMPLER(sampler_BlurTexture);

			float _Intensity;
            float _Angle;
            float _ShiftSize;
            float2 _ShiftOverride;
            int _ShiftFbmOctaves;
            float _CutOffSharpness;
            float2 _CutOffCenter;
            float _ChromAbIntensity;
            float _ShaderTime;
            float _BlurVigStrength;

            float3 SampleColor(float2 uv){
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).xyz;
            }

            float3 SampleBlurred(float2 uv){
                return SAMPLE_TEXTURE2D(_BlurTexture, sampler_BlurTexture, uv).xyz;
            }

            float rand(float n){return frac(sin(n) * 43758.5453123);}

            float vignette(float2 pt, float strength){
                float invAspect = _ScreenParams.y / _ScreenParams.x;
                pt = abs(pt) - (float2(1.0,invAspect) - 0.05);
                float boxSdf = length(max(pt, 0.0)) + min(0.0, max(pt.x, pt.y));
                return smoothstep(0.04, 0.0, boxSdf);
            }

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
                float noiseTime = _ShaderTime;
                float nsr = fbm(noiseTime + 60);
                pt *= 0.9; //scale up so edges hopefully don't show up
                float rt = _ShaderTime * .5;
                float2 offset = float2(cos(rt),sin(rt)) * ((0.5 + 0.5* nsr) * _ShiftSize);
                offset -= _ShiftOverride;
                pt += offset;

                float vig = vignette(pt, _BlurVigStrength);
                float2 uvr = pt2uv(pt);
                float2 uvg = pt2uv(pt + _ChromAbIntensity);
                float2 uvb = pt2uv(pt - _ChromAbIntensity);
                float3 blur =  float3(SampleBlurred(uvr).r, SampleBlurred(uvg).g, SampleBlurred(uvb).b);
                float mask = saturate(LUMINANCE(blur) * vig);

                pt = qt;
                pt -= _CutOffCenter;
                pt = mul(r2d(_Angle), pt);
                mask *= saturate(exp(pt.x * _CutOffSharpness));

                c = lerp(c, blur, mask * _Intensity);

                return c;
            }

            float4 frag (Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);


				return float4(SplitDiopter(input.texcoord.xy), 1);
            }

			ENDHLSL
		}
	}
	FallBack "Diffuse"
}
