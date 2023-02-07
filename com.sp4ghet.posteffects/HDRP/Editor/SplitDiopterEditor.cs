using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;


namespace sp4ghet
{
    [CustomEditor(typeof(SplitDiopter))]
    sealed class SplitDiopterEditor : VolumeComponentEditor
    {
        SerializedDataParameter intensity;
        SerializedDataParameter blurSigma;
        SerializedDataParameter blurSize;
        SerializedDataParameter speed;
        SerializedDataParameter fastBlur;
        SerializedDataParameter shiftSize;
        SerializedDataParameter shiftFbmOctaves;
        SerializedDataParameter manualShift;
        SerializedDataParameter shiftValue;
        SerializedDataParameter cutoffAngle;
        SerializedDataParameter cutoffSharpness;
        SerializedDataParameter cutoffCenter;
        SerializedDataParameter chromabIntensity;

        public override void OnEnable()
        {
            base.OnEnable();
            var o = new PropertyFetcher<SplitDiopter>(serializedObject);
            intensity = Unpack(o.Find(x => x.intensity));
            blurSigma = Unpack(o.Find(x => x.blurSigma));
            blurSize = Unpack(o.Find(x => x.blurSize));
            speed = Unpack(o.Find(x => x.speed));
            shiftSize = Unpack(o.Find(x => x.shiftSize));
            shiftFbmOctaves = Unpack(o.Find(x => x.shiftFbmOctaves));
            manualShift = Unpack(o.Find(x => x.manualShift));
            shiftValue = Unpack(o.Find(x => x.shiftValue));
            cutoffAngle = Unpack(o.Find(x => x.cutoffAngle));
            cutoffSharpness = Unpack(o.Find(x => x.cutoffSharpness));
            cutoffCenter = Unpack(o.Find(x => x.cutoffCenter));
            fastBlur = Unpack(o.Find(x => x.fastBlur));
            chromabIntensity = Unpack(o.Find(x => x.chromabIntensity));
        }

        public override void OnInspectorGUI()
        {
            PropertyField(intensity);
            PropertyField(blurSigma);
            PropertyField(blurSize);
            PropertyField(fastBlur);
            PropertyField(speed);
            PropertyField(manualShift);
            if (manualShift.value.boolValue)
            {
                PropertyField(shiftValue);
            }
            else
            {
                PropertyField(shiftSize);
                PropertyField(shiftFbmOctaves);
            }
            PropertyField(cutoffAngle);
            PropertyField(cutoffSharpness);
            PropertyField(cutoffCenter);
            PropertyField(chromabIntensity);
        }

    }
}
