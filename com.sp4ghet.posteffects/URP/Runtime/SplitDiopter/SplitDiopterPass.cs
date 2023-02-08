using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using GraphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat;

namespace sp4ghet
{
    public class SplitDiopterPass : ScriptableRenderPass
    {
        ProfilingSampler m_ProfilingSampler = new ProfilingSampler("SplitDiopter");
        Material m_Material;
        Material m_BlurMat;
        RTHandle m_BlurXBuffer, m_BlurYBuffer;
        RTHandle m_CameraColorTarget;
        RTHandle m_tempColorTarget;
        float m_ShaderTime = 0f;
        SplitDiopter m_component;

        public SplitDiopterPass(RenderPassEvent renderPassEvent, Material material, Material blurMat)
        {
            m_Material = material;
            m_BlurMat = blurMat;
            this.renderPassEvent = renderPassEvent;

        }

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


        public void SetTarget(RTHandle colorHandle)
        {
            m_CameraColorTarget = colorHandle;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            ConfigureTarget(m_CameraColorTarget);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            base.OnCameraSetup(cmd, ref renderingData);
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            m_CameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            base.FrameCleanup(cmd);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cameraData = renderingData.cameraData;
            if ((renderingData.cameraData.cameraType & (CameraType.Game | CameraType.SceneView)) == 0)
                return;
            if (m_Material == null)
                return;
            m_component = VolumeManager.instance.stack.GetComponent<SplitDiopter>();
            if (m_component == null)
            {
                Debug.LogError("No volume in stack");
                return;
            }

            var camera = cameraData.camera;
            const GraphicsFormat rtFormat = GraphicsFormat.R16G16B16A16_SFloat;
            if (m_tempColorTarget == null)
            {
                m_tempColorTarget = RTHandles.Alloc(camera.pixelWidth, camera.pixelHeight, colorFormat: rtFormat, name: "TempTarget", enableRandomWrite: true);
            }

            if (m_BlurYBuffer == null)
            {
                m_BlurYBuffer = RTHandles.Alloc(camera.pixelWidth, camera.pixelHeight, colorFormat: rtFormat, name: "BlurYBuffer", enableRandomWrite: true);
            }

            // only use XBuffer if we need it
            if (m_component.fastBlur.value && (m_BlurXBuffer == null))
            {
                m_BlurXBuffer = RTHandles.Alloc(camera.pixelWidth, camera.pixelHeight, colorFormat: rtFormat, name: "BlurXBuffer", enableRandomWrite: true);
            }

            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                var descriptor = cameraData.cameraTargetDescriptor;
                descriptor.depthStencilFormat = GraphicsFormat.None;
                descriptor.depthBufferBits = 0;

                cmd.GetTemporaryRT(ShaderIDs.MainTex, descriptor);
                cmd.Blit(m_CameraColorTarget, ShaderIDs.MainTex);

                // BlurPass
                m_BlurMat.SetInt(ShaderIDs.BlurSize, m_component.blurSize.value);
                m_BlurMat.SetFloat(ShaderIDs.SigmaSquared, m_component.blurSigma.value * m_component.blurSigma.value + 0.0001f);
                if (m_component.fastBlur.value)
                {
                    cmd.GetTemporaryRT(ShaderIDs.BlurSourceTexture, descriptor);
                    cmd.GetTemporaryRT(ShaderIDs.BlurTexture, descriptor);
                    cmd.SetRenderTarget(ShaderIDs.BlurSourceTexture);
                    cmd.Blit(ShaderIDs.MainTex, ShaderIDs.BlurSourceTexture, m_BlurMat, m_BlurMat.FindPass("BlurX"));

                    cmd.SetRenderTarget(ShaderIDs.BlurTexture);
                    cmd.Blit(ShaderIDs.BlurSourceTexture, ShaderIDs.BlurTexture, m_BlurMat, m_BlurMat.FindPass("BlurY"));
                }
                else
                {
                    cmd.GetTemporaryRT(ShaderIDs.BlurTexture, descriptor);
                    cmd.Blit(ShaderIDs.MainTex, ShaderIDs.BlurTexture, m_BlurMat, m_BlurMat.FindPass("Blur"));
                }


                m_ShaderTime += Time.deltaTime * m_component.speed.value;
                m_Material.SetFloat(ShaderIDs.Intensity, m_component.intensity.value);
                float angle = m_component.cutoffAngle.value % 360f;
                m_Material.SetFloat(ShaderIDs.Angle, Mathf.Deg2Rad * angle);
                m_Material.SetFloat(ShaderIDs.ShaderTime, m_ShaderTime);
                // ignore automated shifting if manually shifting
                float shiftAmount = m_component.manualShift.value ? 0 : m_component.shiftSize.value;
                m_Material.SetFloat(ShaderIDs.ShiftSize, shiftAmount);
                // ignore manual shifting if auto shifting
                Vector4 shiftOverride = m_component.manualShift.value ? new Vector4(m_component.shiftValue.value.x, m_component.shiftValue.value.y, 0f, 0f) : Vector4.zero;
                m_Material.SetVector(ShaderIDs.ShiftOverride, shiftOverride);
                int fbmOctaves = m_component.manualShift.value ? 0 : m_component.shiftFbmOctaves.value;
                m_Material.SetInt(ShaderIDs.ShiftFbmOctaves, fbmOctaves);
                m_Material.SetFloat(ShaderIDs.ChromAbIntensity, m_component.chromabIntensity.value);

                m_Material.SetFloat(ShaderIDs.CutOffSharpness, Mathf.Max(0f, m_component.cutoffSharpness.value));
                m_Material.SetVector(ShaderIDs.CutOffCenter, new Vector4(m_component.cutoffCenter.value.x, m_component.cutoffCenter.value.y, 0f, 0f));
                cmd.Blit(ShaderIDs.MainTex, m_CameraColorTarget.nameID, m_Material, 0);
            }
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            // cmd.ReleaseTemporaryRT(ShaderIDs.MainTex);
            // cmd.ReleaseTemporaryRT(ShaderIDs.BlurTexture);
            // cmd.ReleaseTemporaryRT(ShaderIDs.BlurSourceTexture);
            m_BlurXBuffer?.Release();
            m_BlurYBuffer?.Release();
            m_tempColorTarget?.Release();
            m_BlurXBuffer = null;
            m_BlurYBuffer = null;
            m_tempColorTarget = null;

            CommandBufferPool.Release(cmd);

        }

    }
}
