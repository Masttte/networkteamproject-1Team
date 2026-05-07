using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/CustomScreen")]
    public sealed class CustomScreenVol : VolumeComponent, IPostProcessComponent
    {
        [Header("Core")]
        public ClampedFloatParameter _intensity        = new ClampedFloatParameter(0f, -5f, 5f);
        
        [Header("Distortion")]
        public ClampedFloatParameter _distortionScale  = new ClampedFloatParameter(0f, 0f, 7f);
        
        [Header("YCbCr")]
        public ClampedFloatParameter _applyToY        = new ClampedFloatParameter(0f, 0f, 20f);
        public ClampedFloatParameter _applyToGlitch   = new ClampedFloatParameter(0f, 0f, 20f);
        [InspectorName("Distort Tint")] // applyToY로 어긋난 영역에만 곱해지는 색조
        public ColorParameter _distortYTint = new ColorParameter(Color.white, false);
        
        [Header("Animation")]
        public NoInterpClampedFloatParameter _fps  = new NoInterpClampedFloatParameter(0f, 0f, 60f);
        public ClampedFloatParameter  _fpsBreak    = new ClampedFloatParameter(0f, 0f, 1f);
        
        [Header("Channel Shift")]
        [InspectorName("Pow")]
        public ClampedFloatParameter _channelShiftPow    = new ClampedFloatParameter(0f, -10f, 10f);
        [InspectorName("Spread")]
        public ClampedFloatParameter _channelShiftSpread = new ClampedFloatParameter(0f, 0f, 1f);
        [InspectorName("Tint")] 
        public ColorParameter _glitchTint = new ColorParameter(Color.white, false); // 글리치 색조 컨트롤 추가

        // =======================================================================
        public bool IsActive() => active && (_intensity != 0f); //_intensity가 0이 아닐때 효과 발동

        public bool IsTileCompatible() => true;
    }
}