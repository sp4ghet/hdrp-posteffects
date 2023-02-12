using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace sp4ghet
{
    [Serializable, VolumeComponentMenuForRenderPipeline("Post-processing/sp4ghet/SplitDiopter", typeof(UniversalRenderPipeline))]
    public class SplitDiopter : VolumeComponent
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

        // [Tooltip("Blur Vignette Strength (Deal with stretched edges)")]
        // public FloatParameter blurVignetteStrength = new FloatParameter(0f);

        // Other 'Parameter' variables you might have

        // Tells when our effect should be rendered
        public bool IsActive() => intensity.value > 0;

        // I have no idea what this does yet but I'll update the post once I find an usage
        // public bool IsTileCompatible() => true;
    }
}
