using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;
namespace sp4ghet
{

    [Serializable, VolumeComponentMenu("Post-processing/sp4ghet/MirageEffect")]
    public sealed class MirageEffect : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        [Tooltip("Controls the intensity of the effect.")]
        public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);
        [Tooltip("Center of y axis for effect. (World space for no reflect, screen space for reflect)")]
        public FloatParameter yCenter = new FloatParameter(0f);
        [Tooltip("Cyclic Noise Scale.")]
        public FloatParameter cNoiseScale = new FloatParameter(0.8f);
        [Tooltip("Perlin noise scale.")]
        public FloatParameter pNoiseScale = new FloatParameter(0.4f);
        [Tooltip("Movement speed.")]
        public FloatParameter wigglePace = new FloatParameter(6f);
        [Tooltip("Updraft speed.")]
        public FloatParameter updraft = new FloatParameter(1f);

        [Tooltip("Larger number = smaller affected area.")]
        public FloatParameter narrowness = new FloatParameter(0f);

        [Tooltip("Minimum depth for effect.")]
        public FloatParameter depthMin = new FloatParameter(0f);

        [Tooltip("Maximum depth for effect.")]
        public FloatParameter depthMax = new FloatParameter(0f);

        [Tooltip("Enable reflection effect.")]
        public BoolParameter enableReflect = new BoolParameter(false);

        Material m_Material;

        public bool IsActive()
        {
            bool active = m_Material != null;
            active = active && intensity.value > 0f;
            return active;
        }

        // Do not forget to add this post process in the Custom Post Process Orders list (Project Settings > Graphics > HDRP Global Settings).
        public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

        const string kShaderName = "Hidden/Shader/MirageEffect";

        public override void Setup()
        {
            if (Shader.Find(kShaderName) != null)
                m_Material = new Material(Shader.Find(kShaderName));
            else
                Debug.LogError($"Unable to find shader '{kShaderName}'. Post Process Volume MirageEffect is unable to load.");
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
        {
            if (m_Material == null)
                return;

            m_Material.SetFloat("_Intensity", intensity.value);
            m_Material.SetTexture("_MainTex", source);
            m_Material.SetFloat("_YCenter", yCenter.value);
            m_Material.SetFloat("_CNoiseScale", cNoiseScale.value);
            m_Material.SetFloat("_PNoiseScale", pNoiseScale.value);
            m_Material.SetFloat("_Narrowness", narrowness.value);
            m_Material.SetFloat("_Updraft", updraft.value);
            m_Material.SetFloat("_WigglePace", wigglePace.value);
            m_Material.SetFloat("_DepthMin", depthMin.value);
            m_Material.SetFloat("_DepthMax", depthMax.value);
            m_Material.SetInteger("_EnableReflect", enableReflect.value ? 1 : 0);
            HDUtils.DrawFullScreen(cmd, m_Material, destination, shaderPassId: 0);
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(m_Material);
        }
    }
}
