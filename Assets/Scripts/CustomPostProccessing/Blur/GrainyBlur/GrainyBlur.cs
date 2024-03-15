using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CPP.EFFECTS{
    [VolumeComponentMenu(("Custom Post Processing/Blur/Grainy Blur"))]
    public class GrainyBlur : CustomPostProcessing{
        public ClampedFloatParameter BlurSpread = new ClampedFloatParameter(0.6f, 0.0f, 30.0f);
        public ClampedIntParameter Iteration = new ClampedIntParameter(0, 0, 8);
        public ClampedFloatParameter RTDownScaling = new ClampedFloatParameter(2.0f, 1.0f, 8.0f);

        private const string mShaderName = "Hidden/PostProcessing/GrainyBlur";

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

            RenderingUtils.ReAllocateIfNeeded(ref mTempRT0, descriptor, name: mTempRT0Name, wrapMode: TextureWrapMode.Clamp, filterMode: FilterMode.Bilinear);
        }


        public override void Render(CommandBuffer cmd, ref RenderingData renderingData, in RTHandle source, in RTHandle destination) {
            if (mMaterial == null) return;

            cmd.SetGlobalFloat(mIterationKeyword, Iteration.value);
            cmd.SetGlobalFloat(mBlurSizeKeyword, BlurSpread.value);

            if (RTDownScaling.value > 1.0f) {
                // DownSample
                Draw(cmd, source, mTempRT0);
                // GrainyBlur
                Draw(cmd, mTempRT0, destination, 0);
            }
            else {
                Draw(cmd, source, destination, 0);
            }
        }

        public override void Dispose(bool disposing) {
            base.Dispose(disposing);
            CoreUtils.Destroy(mMaterial);

            mTempRT0?.Release();
        }
    }
}