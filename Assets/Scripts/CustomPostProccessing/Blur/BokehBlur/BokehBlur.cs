using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CPP.EFFECTS{
    [VolumeComponentMenu(("Custom Post Processing/Blur/Bokeh Blur"))]
    public class BokehBlur : CustomPostProcessing{
        public ClampedFloatParameter BlurSpread = new ClampedFloatParameter(0.6f, 0.0f, 10.0f);
        public ClampedIntParameter Iteration = new ClampedIntParameter(0, 0, 128);
        public ClampedFloatParameter RTDownScaling = new ClampedFloatParameter(2.0f, 1.0f, 8.0f);

        private const string mShaderName = "Hidden/PostProcessing/BokehBlur";

        private int mBlurSizeKeyword = Shader.PropertyToID("_BlurSize"),
            mIterationKeyword = Shader.PropertyToID("_Iteration");

        public override bool IsActive() => mMaterial != null && Iteration.value != 0;

        public override CustomPostProcessInjectionPoint InjectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

        public override int OrderInInjectionPoint => 5;

        private string mTempRT0Name => "_TemporaryRenderTexture0";

        private RTHandle mTempRT0;

        public override void Setup() {
            if (mMaterial == null)
                mMaterial = CoreUtils.CreateEngineMaterial(mShaderName);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
            var descriptor = GetCameraRenderTextureDescriptor(renderingData);
            descriptor.width = (int)(descriptor.width / RTDownScaling.value);
            descriptor.height = (int)(descriptor.height / RTDownScaling.value);

            RenderingUtils.ReAllocateIfNeeded(ref mTempRT0, descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: mTempRT0Name);
        }

        public override void Render(CommandBuffer cmd, ref RenderingData renderingData, in RTHandle source, in RTHandle destination) {
            if (mMaterial == null) return;

            // DownSample
            Draw(cmd, source, mTempRT0);

            // BokehBlur
            cmd.SetGlobalFloat(mBlurSizeKeyword, BlurSpread.value);
            cmd.SetGlobalFloat(mIterationKeyword, Iteration.value);
            Draw(cmd, mTempRT0, destination, 0);
        }

        public override void Dispose(bool disposing) {
            base.Dispose(disposing);
            CoreUtils.Destroy(mMaterial);

            mTempRT0?.Release();
        }
    }
}