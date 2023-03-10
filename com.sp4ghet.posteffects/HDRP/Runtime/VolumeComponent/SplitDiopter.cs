using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;
using GraphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat;
namespace sp4ghet
{
    [Serializable, VolumeComponentMenu("Post-processing/sp4ghet/SplitDiopter")]
    public sealed class SplitDiopter : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        [Tooltip("Controls the intensity of the effect.")]
        public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);

        [Tooltip("Controls the blur sigma.")]
        public ClampedFloatParameter blurSigma = new ClampedFloatParameter(10f, 0f, 100f);

        [Tooltip("Control the blur kernel size")]
        public IntParameter blurSize = new IntParameter(10);

        [Tooltip("Control the shifting speed")]
        public FloatParameter speed = new FloatParameter(1);

        [Tooltip("Make blur faster but less visually complex")]
        public BoolParameter fastBlur = new BoolParameter(true);

        [Tooltip("The amount of shifting")]
        public ClampedFloatParameter shiftSize = new ClampedFloatParameter(0.04f, 0f, 0.5f);
        [Tooltip("FBM Octaves for autoshift")]
        public ClampedIntParameter shiftFbmOctaves = new ClampedIntParameter(3, 0, 10);

        [Tooltip("Shift manually")]
        public BoolParameter manualShift = new BoolParameter(false);

        [Tooltip("Shift the image this much (manually)")]
        public Vector2Parameter shiftValue = new Vector2Parameter(Vector2.zero);

        [Tooltip("The angle for the split diopter cutoff (default is x-axis)")]
        public FloatParameter cutoffAngle = new FloatParameter(0f);

        [Tooltip("The cutoff intensity for the split diopter cutoff")]
        public FloatParameter cutoffSharpness = new FloatParameter(15f);

        [Tooltip("Shift the center of the cutoff")]
        public Vector2Parameter cutoffCenter = new Vector2Parameter(Vector2.zero);

        [Tooltip("Chromatic Aberration Intensity")]
        public ClampedFloatParameter chromabIntensity = new ClampedFloatParameter(0f, 0f, 0.015f);


        Material m_Material;
        Material m_BlurMat;
        RTHandle m_BlurXBuffer, m_BlurYBuffer;

        float m_ShaderTime = 0f;

        public bool IsActive()
        {
            bool active = m_Material != null && intensity.value > 0f;
            return active;
        }

        // Do not forget to add this post process in the Custom Post Process Orders list (Project Settings > Graphics > HDRP Global Settings).
        public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

        const string kShaderName = "Hidden/Shader/SplitDiopter";
        const string kBlurShaderName = "Hidden/Shader/GaussianBlur";

        static class ShaderIDs
        {
            internal static readonly int Intensity = Shader.PropertyToID("_Intensity");
            internal static readonly int MainTex = Shader.PropertyToID("_MainTex");
            internal static readonly int BlurSourceTexture = Shader.PropertyToID("_Source");
            internal static readonly int BlurTexture = Shader.PropertyToID("_BlurTexture");
            internal static readonly int BlurSize = Shader.PropertyToID("_BlurSize");
            internal static readonly int SigmaSquared = Shader.PropertyToID("_SigmaSquared");
            internal static readonly int Angle = Shader.PropertyToID("_Angle");
            internal static readonly int ShiftOverride = Shader.PropertyToID("_ShiftOverride");
            internal static readonly int ShiftFbmOctaves = Shader.PropertyToID("_ShiftFbmOctaves");
            internal static readonly int ShiftSize = Shader.PropertyToID("_ShiftSize");
            internal static readonly int CutOffSharpness = Shader.PropertyToID("_CutOffSharpness");
            internal static readonly int CutOffCenter = Shader.PropertyToID("_CutOffCenter");
            internal static readonly int ChromAbIntensity = Shader.PropertyToID("_ChromAbIntensity");
            internal static readonly int ShaderTime = Shader.PropertyToID("_ShaderTime");

        }


        public override void Setup()
        {
            if (Shader.Find(kShaderName) != null)
                m_Material = new Material(Shader.Find(kShaderName));
            else
                Debug.LogError($"Unable to find shader '{kShaderName}'. Post Process Volume SplitDiopter is unable to load.");

            if (Shader.Find(kBlurShaderName) != null)
            {
                m_BlurMat = new Material(Shader.Find(kBlurShaderName));
            }
            else
            {
                Debug.LogError($"Unable to find shader '{kBlurShaderName}'. Post Process Volume SplitDiopter is unable to load.");
            }

            m_BlurXBuffer?.Release();
            m_BlurXBuffer = null;

            m_BlurYBuffer?.Release();
            m_BlurYBuffer = null;
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
        {
            if (m_Material == null)
                return;

            const GraphicsFormat rtFormat = GraphicsFormat.R16G16B16A16_SFloat;
            if (m_BlurYBuffer == null)
            {
                m_BlurYBuffer = RTHandles.Alloc(camera.actualWidth, camera.actualHeight, colorFormat: rtFormat);
            }


            // BlurPass
            m_BlurMat.SetTexture(ShaderIDs.MainTex, source);
            m_BlurMat.SetInt(ShaderIDs.BlurSize, blurSize.value);
            m_BlurMat.SetFloat(ShaderIDs.SigmaSquared, blurSigma.value * blurSigma.value + 0.0001f);
            if (fastBlur.value)
            {
                // only use XBuffer if we need it
                if (m_BlurXBuffer == null)
                {
                    m_BlurXBuffer = RTHandles.Alloc(camera.actualWidth, camera.actualHeight, colorFormat: rtFormat);
                }
                m_BlurMat.SetTexture(ShaderIDs.BlurSourceTexture, m_BlurXBuffer.rt);
                HDUtils.DrawFullScreen(cmd, m_BlurMat, m_BlurXBuffer, shaderPassId: m_BlurMat.FindPass("BlurX"));
                HDUtils.DrawFullScreen(cmd, m_BlurMat, m_BlurYBuffer, shaderPassId: m_BlurMat.FindPass("BlurY"));
            }
            else
            {
                HDUtils.DrawFullScreen(cmd, m_BlurMat, m_BlurYBuffer, shaderPassId: m_BlurMat.FindPass("Blur"));
            }

            m_ShaderTime += Time.deltaTime * speed.value;
            m_Material.SetTexture(ShaderIDs.MainTex, source);
            m_Material.SetTexture(ShaderIDs.BlurTexture, m_BlurYBuffer.rt);
            m_Material.SetFloat(ShaderIDs.Intensity, intensity.value);
            float angle = cutoffAngle.value % 360f;
            m_Material.SetFloat(ShaderIDs.Angle, Mathf.Deg2Rad * angle);
            m_Material.SetFloat(ShaderIDs.ShaderTime, m_ShaderTime);
            // ignore automated shifting if manually shifting
            float shiftAmount = manualShift.value ? 0 : shiftSize.value;
            m_Material.SetFloat(ShaderIDs.ShiftSize, shiftAmount);
            // ignore manual shifting if auto shifting
            Vector4 shiftOverride = manualShift.value ? new Vector4(shiftValue.value.x, shiftValue.value.y, 0f, 0f) : Vector4.zero;
            m_Material.SetVector(ShaderIDs.ShiftOverride, shiftOverride);
            int fbmOctaves = manualShift.value ? 0 : shiftFbmOctaves.value;
            m_Material.SetInt(ShaderIDs.ShiftFbmOctaves, fbmOctaves);
            m_Material.SetFloat(ShaderIDs.ChromAbIntensity, chromabIntensity.value);

            m_Material.SetFloat(ShaderIDs.CutOffSharpness, Mathf.Max(0f, cutoffSharpness.value));
            m_Material.SetVector(ShaderIDs.CutOffCenter, new Vector4(cutoffCenter.value.x, cutoffCenter.value.y, 0f, 0f));
            HDUtils.DrawFullScreen(cmd, m_Material, destination, shaderPassId: 0);

        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(m_Material);
            CoreUtils.Destroy(m_BlurMat);

            m_BlurXBuffer?.Release();
            m_BlurXBuffer = null;
            m_BlurYBuffer?.Release();
            m_BlurYBuffer = null;
        }
    }

}
