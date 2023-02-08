using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using GraphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat;

namespace sp4ghet
{
    public class MiragePass : ScriptableRenderPass
    {
        ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Mirage");
        Material m_Material;
        RTHandle m_CameraColorTarget;
        Mirage m_component;

        public MiragePass(RenderPassEvent renderPassEvent, Material material)
        {
            m_Material = material;
            this.renderPassEvent = renderPassEvent;
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
            m_CameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
        }

        static int maintexID = Shader.PropertyToID("_MainTex");

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cameraData = renderingData.cameraData;
            if ((renderingData.cameraData.cameraType & (CameraType.Game | CameraType.SceneView)) == 0)
                return;
            if (m_Material == null)
                return;
            m_component = VolumeManager.instance.stack.GetComponent<Mirage>();
            if (m_component == null)
            {
                Debug.LogError("No volume in stack");
                return;
            }

            var camera = cameraData.camera;

            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                var descriptor = cameraData.cameraTargetDescriptor;
                descriptor.depthStencilFormat = GraphicsFormat.None;
                descriptor.depthBufferBits = 0;

                cmd.GetTemporaryRT(maintexID, descriptor);
                cmd.Blit(m_CameraColorTarget.nameID, maintexID);

                m_Material.SetFloat("_Intensity", m_component.intensity.value);
                m_Material.SetFloat("_YCenter", m_component.yCenter.value);
                m_Material.SetFloat("_CNoiseScale", m_component.cNoiseScale.value);
                m_Material.SetFloat("_PNoiseScale", m_component.pNoiseScale.value);
                m_Material.SetFloat("_Narrowness", m_component.narrowness.value);
                m_Material.SetFloat("_Updraft", m_component.updraft.value);
                m_Material.SetFloat("_WigglePace", m_component.wigglePace.value);
                m_Material.SetFloat("_DepthMin", m_component.depthMin.value);
                m_Material.SetFloat("_DepthMax", m_component.depthMax.value);
                m_Material.SetInteger("_EnableReflect", m_component.enableReflect.value ? 1 : 0);

                cmd.Blit(maintexID, m_CameraColorTarget.nameID, m_Material, 0);
            }
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();


            CommandBufferPool.Release(cmd);

        }
    }
}
