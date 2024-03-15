using System;
using CPP;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CPP.EFFECTS{
    [VolumeComponentMenu("Custom Post Processing/Blur/Dual Kawase Blur")]
    public class DualKawaseBlur : CustomPostProcessing{
        private const int MAXITERATION = 20;

        #region Properties Defination

        public ClampedFloatParameter BlurSpread = new ClampedFloatParameter(0.6f, 0.0f, 3.0f);
        public ClampedIntParameter Iteration = new ClampedIntParameter(0, 0, MAXITERATION);
        public ClampedFloatParameter RTDownScaling = new ClampedFloatParameter(2.0f, 1.0f, 8.0f);

        #endregion

        private const string mShaderName = "Hidden/PostProcessing/DualKawaseBlur";

        private int mBlurSizeKeyword = Shader.PropertyToID("_BlurSize");


        public override bool IsActive() => mMaterial != null && Iteration.value != 0;

        public override CustomPostProcessInjectionPoint InjectionPoint =>
            CustomPostProcessInjectionPoint.AfterPostProcess;

        public override int OrderInInjectionPoint => 5;

        private string mTempRTName => "_TemporaryRenderTexture";

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
            RenderingUtils.ReAllocateIfNeeded(ref mTempRT[0], descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: mTempRTName + "0");

            // Allocate RenderTexture
            for (int i = 1; i <= Iteration.value; i++) {
                descriptor.width = Math.Max(descriptor.width / 2, 1);
                descriptor.height = Math.Max(descriptor.height / 2, 1);

                RenderingUtils.ReAllocateIfNeeded(ref mTempRT[i], descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: mTempRTName + i);
            }
        }

        public override void Render(CommandBuffer cmd, ref RenderingData renderingData, in RTHandle source, in RTHandle destination) {
            if (mMaterial == null) return;

            Draw(cmd, source, mTempRT[0]);

            // DownSample
            for (int i = 0; i < Iteration.value; i++) {
                cmd.SetGlobalFloat(mBlurSizeKeyword, 1.0f + i * BlurSpread.value);
                Draw(cmd, mTempRT[i], mTempRT[i + 1], 0);
            }

            // UpSample
            for (int i = Iteration.value; i > 1; i--) {
                cmd.SetGlobalFloat(mBlurSizeKeyword, 1.0f + i * BlurSpread.value);
                Draw(cmd, mTempRT[i], mTempRT[i - 1], 1);
            }

            Draw(cmd, mTempRT[1], destination, 1);
        }

        public override void Dispose(bool disposing) {
            base.Dispose(disposing);
            CoreUtils.Destroy(mMaterial);

            for (int i = 0; i < MAXITERATION; i++) {
                mTempRT[i]?.Release();
            }
        }
    }
}