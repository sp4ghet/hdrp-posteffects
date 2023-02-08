using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace sp4ghet
{
    [Serializable, VolumeComponentMenuForRenderPipeline("Post-processing/sp4ghet/Mirage", typeof(UniversalRenderPipeline))]
    public class Mirage : VolumeComponent
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

        // Other 'Parameter' variables you might have

        // Tells when our effect should be rendered
        public bool IsActive() => intensity.value > 0;

        // I have no idea what this does yet but I'll update the post once I find an usage
        // public bool IsTileCompatible() => true;
    }
}
