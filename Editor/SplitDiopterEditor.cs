using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;


namespace sp4ghet
{
    [CustomEditor(typeof(SplitDiopter))]
    sealed class SplitDiopterEditor : VolumeComponentEditor
    {
        SerializedDataParameter intensity;
        SerializedDataParameter blurSigma;
        SerializedDataParameter blurSize;
        SerializedDataParameter speed;
        SerializedDataParameter shiftSize;
        SerializedDataParameter manualShift;
        SerializedDataParameter shiftValue;
        SerializedDataParameter cutoffAngle;
        SerializedDataParameter cutoffSharpness;
        SerializedDataParameter cutoffCenter;

        public override void OnEnable()
        {
            base.OnEnable();
            var o = new PropertyFetcher<SplitDiopter>(serializedObject);
            intensity = Unpack(o.Find(x => x.intensity));
            blurSigma = Unpack(o.Find(x => x.blurSigma));
            blurSize = Unpack(o.Find(x => x.blurSize));
            speed = Unpack(o.Find(x => x.speed));
            shiftSize = Unpack(o.Find(x => x.shiftSize));
            manualShift = Unpack(o.Find(x => x.manualShift));
            shiftValue = Unpack(o.Find(x => x.shiftValue));
            cutoffAngle = Unpack(o.Find(x => x.cutoffAngle));
            cutoffSharpness = Unpack(o.Find(x => x.cutoffSharpness));
            cutoffCenter = Unpack(o.Find(x => x.cutoffCenter));
        }

        public override void OnInspectorGUI()
        {
            PropertyField(intensity);
            PropertyField(blurSigma);
            PropertyField(blurSize);
            PropertyField(speed);
            PropertyField(manualShift);
            if (manualShift.value.boolValue)
            {
                PropertyField(shiftValue);
            }
            else
            {
                PropertyField(shiftSize);
            }
            PropertyField(cutoffAngle);
            PropertyField(cutoffSharpness);
            PropertyField(cutoffCenter);
        }

    }
}
