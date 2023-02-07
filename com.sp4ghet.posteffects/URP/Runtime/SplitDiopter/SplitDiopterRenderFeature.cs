using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace sp4ghet
{
    public class SplitDiopterRenderFeature : ScriptableRendererFeature
    {
        public RenderPassEvent m_renderPassEvent = RenderPassEvent.AfterRendering;

        private SplitDiopterPass m_pass;
        Material m_Material;
        Material m_BlurMat;

        const string kShaderName = "Hidden/Shader/URP/SplitDiopter";
        const string kBlurShaderName = "Hidden/Shader/URP/GaussianBlur";
        public override void Create()
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

            this.name = "SplitDiopter";
            m_pass = new SplitDiopterPass(m_renderPassEvent, m_Material, m_BlurMat);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if ((renderingData.cameraData.cameraType & (CameraType.Game | CameraType.SceneView)) == 0)
            {
                return;
            }
            m_pass.ConfigureInput(ScriptableRenderPassInput.Color);
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
            CoreUtils.Destroy(m_BlurMat);
        }
    }
}
