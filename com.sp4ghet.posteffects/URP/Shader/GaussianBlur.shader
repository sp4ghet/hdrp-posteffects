Shader "Hidden/Shader/URP/GaussianBlur"
{
	Properties
	{
	    _MainTex ("Main Texture", 2D) = "white" {}
        _Source("Source Texture", 2D) = "white" {}
        _BlurSize("Kernel Radius", Int) = 1
	}

HLSLINCLUDE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #define INV_SQRT_TWO_PI 0.3989422804 // 1 / sqrt(TAU)
            TEXTURE2D_X(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D_X(_Source);
            SAMPLER(sampler_Source);
            int _BlurSize;
            float _SigmaSquared;

            #include "Packages/com.sp4ghet.posteffects/ShaderIncludes/ShaderLib/Common.hlsl"
            #include "Packages/com.sp4ghet.posteffects/ShaderIncludes/ShaderLib/Noise.hlsl"

            float3 SampleColor(float2 uv){
                return SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, uv).xyz;
            }

            float3 SampleBuffer(float2 uv){
                return SAMPLE_TEXTURE2D_X(_Source, sampler_Source, uv).xyz;
            }

            float gaussKern(int x, float sigmaSquared){
            return  (1.0f / sigmaSquared) * INV_SQRT_TWO_PI * exp(-0.5*x*x / sigmaSquared);
            }

            float gauss(float x, float y, float sigmaSquared)
            {
            return  (1.0f / sigmaSquared) * INV_SQRT_TWO_PI * exp(-0.5 * (x * x + y * y) / sigmaSquared);
            }

            #define LUMINANCE(col) (dot(col, float3(0.2126 ,0.7152, 0.0722)))

            float3 BlurX(float2 uv, int blurSize){
                float onePixel = 1.0 / _ScreenSize.x;
                float3 c = SampleColor(uv);
                if(blurSize <= 0){
                    return c;
                }

                float ns = cnoise(uv + _Time.x);
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
                float ns = cnoise(uv + _Time.x);
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
                    val += SampleColor(offsetUV) * weight;
                    sum += weight;
                }
                return (val / sum);
            }

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

            float4 BlurXPass(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                return float4(BlurX(input.texcoord, _BlurSize), 1);
            }

            float4 BlurYPass(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                return float4(BlurY(input.texcoord, _BlurSize), 1);
            }

            float4 BlurPass(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                return float4(Blur(input.texcoord.xy, _BlurSize), 1);
            }
    ENDHLSL


	SubShader
	{
		Tags {
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100
        ZWrite Off
        ZTest Always
        Blend Off
        Cull Off


		Pass
		{
            Name "BlurX"

            HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment BlurXPass
            ENDHLSL
		}

        Pass
		{
            Name "BlurY"

            HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment BlurYPass
			ENDHLSL
		}

        Pass
		{
            Name "Blur"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment BlurPass
            ENDHLSL
		}
	}
	FallBack Off
}
