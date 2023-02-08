using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace sp4ghet
{
    public class MirageRendererFeature : ScriptableRendererFeature
    {
        public RenderPassEvent m_renderPassEvent = RenderPassEvent.AfterRendering;

        private MiragePass m_pass;
        Material m_Material;
        const string kShaderName = "Hidden/Shader/URP/Mirage";

        public override void Create()
        {
            if (Shader.Find(kShaderName) != null)
                m_Material = new Material(Shader.Find(kShaderName));
            else
                Debug.LogError($"Unable to find shader '{kShaderName}'. Post Process Volume Mirage is unable to load.");

            this.name = "Mirage";
            m_pass = new MiragePass(m_renderPassEvent, m_Material);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if ((renderingData.cameraData.cameraType & (CameraType.Game | CameraType.SceneView)) == 0)
            {
                return;
            }
            m_pass.ConfigureInput(ScriptableRenderPassInput.Color);
            m_pass.ConfigureInput(ScriptableRenderPassInput.Depth);
            renderer.EnqueuePass(m_pass);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            if ((renderingData.cameraData.cameraType & (CameraType.Game | CameraType.SceneView)) == 0)
            {
                return;
            }


            m_pass.SetTarget(renderer.cameraColorTargetHandle);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(m_Material);
        }
    }
}
