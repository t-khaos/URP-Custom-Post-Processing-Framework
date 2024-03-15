using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CPP.EFFECTS{
    [VolumeComponentMenu(("Custom Post Processing/Blur/Tilt Shift Blur"))]
    public class TiltShiftBlur : CustomPostProcessing{
        private const int MAXITERATION = 8;

        public ClampedFloatParameter AreaOffset = new ClampedFloatParameter(0.0f, 0.0f, 1.0f);
        public ClampedFloatParameter AreaSize = new ClampedFloatParameter(0.5f, 0.0f, 1.0f);
        public ClampedFloatParameter AreaSpread = new ClampedFloatParameter(0.5f, 0.0f, 1.0f);
        public ClampedFloatParameter BlurSpread = new ClampedFloatParameter(0.6f, 0.0f, 10.0f);
        public ClampedIntParameter Iteration = new ClampedIntParameter(0, 0, 8);
        public ClampedFloatParameter RTDownScaling = new ClampedFloatParameter(2.0f, 1.0f, 8.0f);

        private const string mShaderName = "Hidden/PostProcessing/TiltShiftBlur";

        private int mBlurSizeKeyword = Shader.PropertyToID("_BlurSize"),
            mIterationKeyword = Shader.PropertyToID("_Iteration"),
            mAreaOffsetKeyword = Shader.PropertyToID("_AreaOffset"),
            mAreaSizeKeyword = Shader.PropertyToID("_AreaSize"),
            mAreaSpreadKeyword = Shader.PropertyToID("_AreaSpread");

        public override bool IsActive() => mMaterial != null && Iteration.value != 0 && AreaSize != 0;

        public override CustomPostProcessInjectionPoint InjectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

        public override int OrderInInjectionPoint => 5;

        private string mTempRT0Name => "_TemporaryRenderTexture";

        private RTHandle[] mTempRT = new RTHandle[MAXITERATION + 1];

        public override void Setup() {
            if (mMaterial == null) {
                mMaterial = CoreUtils.CreateEngineMaterial(mShaderName);
            }
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
            var descriptor = GetCameraRenderTextureDescriptor(renderingData);
            descriptor.width = (int)(descriptor.width / RTDownScaling.value);
            descriptor.height = (int)(descriptor.height / RTDownScaling.value);

            for (int i = 0; i <= Iteration.value; i++)
                RenderingUtils.ReAllocateIfNeeded(ref mTempRT[i], descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: mTempRT0Name);
        }

        public override void Render(CommandBuffer cmd, ref RenderingData renderingData, in RTHandle source, in RTHandle destination) {
            if (mMaterial == null) return;

            // DownSample
            Draw(cmd, source, mTempRT[0]);

            // TiltShiftBlur
            cmd.SetGlobalFloat(mAreaOffsetKeyword, AreaOffset.value);
            cmd.SetGlobalFloat(mAreaSizeKeyword, AreaSize.value);
            cmd.SetGlobalFloat(mAreaSpreadKeyword, AreaSpread.value);
            cmd.SetGlobalFloat(mIterationKeyword, Iteration.value);

            for (int i = 0; i < Iteration.value; i++) {
                cmd.SetGlobalFloat(mBlurSizeKeyword, 1.0f + i * BlurSpread.value);
                Draw(cmd, mTempRT[i], mTempRT[i + 1], 0);
            }

            Draw(cmd, mTempRT[Iteration.value], destination);
        }

        public override void Dispose(bool disposing) {
            base.Dispose(disposing);
            CoreUtils.Destroy(mMaterial);

            for (int i = 0; i <= MAXITERATION; i++)
                mTempRT[i]?.Release();
        }
    }
}