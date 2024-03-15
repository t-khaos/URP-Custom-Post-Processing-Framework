using CPP;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CPP.EFFECTS{
    [VolumeComponentMenu("Custom Post Processing/Blur/Box Blur")]
    public class BoxBlur : CustomPostProcessing{
        #region Properties Defination

        public ClampedFloatParameter BlurSpread = new ClampedFloatParameter(0.6f, 0.0f, 3.0f);
        public ClampedIntParameter Iteration = new ClampedIntParameter(0, 0, 20);
        public ClampedFloatParameter RTDownScaling = new ClampedFloatParameter(2.0f, 1.0f, 8.0f);

        #endregion

        private const string mShaderName = "Hidden/PostProcessing/BoxBlur";

        private int mBlurSizeKeyword = Shader.PropertyToID("_BlurSize");


        public override bool IsActive() => mMaterial != null && Iteration.value != 0;

        public override CustomPostProcessInjectionPoint InjectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

        public override int OrderInInjectionPoint => 5;

        private string mTempRT0Name => "_TemporaryRenderTexture0";
        private string mTempRT1Name => "_TemporaryRenderTexture1";

        private RTHandle mTempRT0;
        private RTHandle mTempRT1;

        public override void Setup() {
            if (mMaterial == null)
                mMaterial = CoreUtils.CreateEngineMaterial(mShaderName);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
            var descriptor = GetCameraRenderTextureDescriptor(renderingData);
            descriptor.width = (int)(descriptor.width / RTDownScaling.value);
            descriptor.height = (int)(descriptor.height / RTDownScaling.value);

            RenderingUtils.ReAllocateIfNeeded(ref mTempRT0, descriptor, name: mTempRT0Name, wrapMode: TextureWrapMode.Clamp, filterMode: FilterMode.Bilinear);
            RenderingUtils.ReAllocateIfNeeded(ref mTempRT1, descriptor, name: mTempRT1Name, wrapMode: TextureWrapMode.Clamp, filterMode: FilterMode.Bilinear);
        }

        public override void Render(CommandBuffer cmd, ref RenderingData renderingData, in RTHandle source, in RTHandle destination) {
            if (mMaterial == null) return;

            Draw(cmd, source, mTempRT0);

            for (int i = 0; i < Iteration.value; i++) {
                cmd.SetGlobalFloat(mBlurSizeKeyword, 1.0f + BlurSpread.value);
                Draw(cmd, mTempRT0, mTempRT1, 0);
                Draw(cmd, mTempRT1, mTempRT0, 0);
            }

            Draw(cmd, mTempRT0, destination);

            mTempRT0 = null;
            mTempRT1 = null;
        }

        public override void Dispose(bool disposing) {
            base.Dispose(disposing);
            CoreUtils.Destroy(mMaterial);

            mTempRT0?.Release();
            mTempRT1?.Release();
        }
    }
}