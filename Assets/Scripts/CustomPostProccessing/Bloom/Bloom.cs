using System;
using CPP;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CPP.EFFECTS{
    [VolumeComponentMenu(("Custom Post Processing/Blur/Bloom"))]
    public class Bloom : CustomPostProcessing{
        private enum PassEnum{
            BloomBlur,
            BloomCombine,
            BloomPrefilter,
            BloomPrefilterFirefiles,
            BloomScatterFinalPass
        }

        public enum BloomModeEnum{
            Addtive,
            Scatter
        }

        // 参数声明
        #region Parameters Defination

        private const int MAXITERATION = 16;

        public ClampedIntParameter Iteration = new ClampedIntParameter(0, 0, MAXITERATION);
        public MinFloatParameter Threshold = new MinFloatParameter(0.0f, 0.0f); // 亮度阈值
        public ClampedFloatParameter ThresholdKnee = new ClampedFloatParameter(0.0f, 0.0f, 1.0f); // 控制亮度阈值函数的膝盖形状因子
        public BloomModeParameter BloomMode = new BloomModeParameter(BloomModeEnum.Addtive);
        public MinFloatParameter Intensity = new MinFloatParameter(0.0f, 0.0f);
        public BoolParameter FadeFireFlies = new BoolParameter(false);
        public ClampedFloatParameter BlurSpread = new ClampedFloatParameter(2.0f, 0.2f, 3.0f);
        public ClampedFloatParameter RTDownScaling = new ClampedFloatParameter(1.0f, 1.0f, 8.0f);
        public MinIntParameter DownScalingLimit = new MinIntParameter(1, 1);

        #endregion

        // 判断是否激活
        public override bool IsActive() => mMaterial != null && Iteration.value != 0;

        // 注入点
        public override CustomPostProcessInjectionPoint InjectionPoint => CustomPostProcessInjectionPoint.BeforePostProcess;

        // 在注入点中的序号
        public override int OrderInInjectionPoint => 1;

        // 其他变量
        private const string mShaderName = "Hidden/PostProcessing/Bloom";
        
        private const string mBloomAddtiveKeyword = "_BLOOMADDTIVE",
            mBloomScatterKeyword = "_BLOOMSCATTER";

        private int mBloomBlurSizeId = Shader.PropertyToID("_BloomBlurSize"),
            mBloomSourceTextureId = Shader.PropertyToID("_BloomSourceTexture"),
            mBloomThresholdId = Shader.PropertyToID("_BloomThreshold"),
            mBloomIntensityId = Shader.PropertyToID("_BloomIntensity");

        private const string mTempRTName = "_TemporaryRenderTexture",
            mPrefilteredRTName = "_BloomPrefilteredTexture";

        private RTHandle[] mTempRT = new RTHandle[MAXITERATION * 2 + 1];
        private RTHandle mPrefilteredRT;

        // 初始化实例
        public override void Setup() {
            if (mMaterial == null) {
                mMaterial = CoreUtils.CreateEngineMaterial(mShaderName);
            }
        }

        // 当相机初始化时执行
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
            var descriptor = GetCameraRenderTextureDescriptor(renderingData);
            descriptor.width = (int)(descriptor.width / RTDownScaling.value);
            descriptor.height = (int)(descriptor.height / RTDownScaling.value);

            // 分配 RTHandle
            RenderingUtils.ReAllocateIfNeeded(ref mPrefilteredRT, descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: mPrefilteredRTName);
            for (int i = 0; i < Iteration.value; ++i) {
                RenderingUtils.ReAllocateIfNeeded(ref mTempRT[i * 2], descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: mTempRTName + (i * 2));
                RenderingUtils.ReAllocateIfNeeded(ref mTempRT[i * 2 + 1], descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: mTempRTName + (i * 2 + 1));

                descriptor.width = Math.Max(descriptor.width / 2, DownScalingLimit.value);
                descriptor.height = Math.Max(descriptor.height / 2, DownScalingLimit.value);
            }
        }

        // 具体渲染逻辑
        public override void Render(CommandBuffer cmd, ref RenderingData renderingData, in RTHandle source, in RTHandle destination) {
            if (mMaterial == null) return;

            // Threshold Prefiltered
            Vector4 threshold;
            threshold.x = Mathf.GammaToLinearSpace(Threshold.value);
            threshold.y = threshold.x * ThresholdKnee.value;
            threshold.z = 2.0f * threshold.y;
            threshold.w = 0.25f / (threshold.y + 0.00001f);
            threshold.y -= threshold.x;
            cmd.SetGlobalVector(mBloomThresholdId, threshold);

            Draw(cmd, source, mPrefilteredRT, FadeFireFlies.value ? (int)PassEnum.BloomPrefilterFirefiles : (int)PassEnum.BloomPrefilter);

            // DownSample
            for (int i = 0; i < Iteration.value; ++i) {
                // Vertical
                cmd.SetGlobalVector(mBloomBlurSizeId, new Vector4(0.0f, BlurSpread.value, 0.0f, 0.0f));
                Draw(cmd, i == 0 ? mPrefilteredRT : mTempRT[i], mTempRT[i * 2], (int)PassEnum.BloomBlur);

                // Horizontal
                cmd.SetGlobalVector(mBloomBlurSizeId, new Vector4(BlurSpread.value, 0.0f, 0.0f, 0.0f));
                Draw(cmd, mTempRT[i * 2], mTempRT[i * 2 + 1], (int)PassEnum.BloomBlur);
            }

            // UpSample
            SetKeyword(mBloomAddtiveKeyword, BloomMode.value == BloomModeEnum.Addtive);
            SetKeyword(mBloomScatterKeyword, BloomMode.value == BloomModeEnum.Scatter);

            float finalIntensity = 0.0f;
            PassEnum finalPass = PassEnum.BloomCombine;

            if (Iteration.value > 1) {
                if (BloomMode.value == BloomModeEnum.Addtive) {
                    // 叠加升采样时，混合强度固定为1
                    cmd.SetGlobalFloat(mBloomIntensityId, 1.0f);
                    finalIntensity = Intensity.value;
                    finalPass = PassEnum.BloomCombine;
                }
                else {
                    cmd.SetGlobalFloat(mBloomIntensityId, Intensity.value);
                    // 散射升采样时，将最终的bloom强度限制到0.95 防止增加光
                    finalIntensity = Mathf.Min(Intensity.value, 0.95f);
                    finalPass = PassEnum.BloomScatterFinalPass;
                }

                for (int i = Iteration.value - 1; i > 0; --i) {
                    cmd.SetGlobalTexture(mBloomSourceTextureId, mTempRT[i * 2 - 1]);
                    Draw(cmd, mTempRT[i == Iteration.value - 1 ? i * 2 + 1 : i * 2], mTempRT[i * 2 - 2], 1);
                }
            }

            // Combine DownScaling Source Texture 
            cmd.SetGlobalTexture(mBloomSourceTextureId, source);
            cmd.SetGlobalFloat(mBloomIntensityId, finalIntensity);
            Draw(cmd, mTempRT[0], destination, (int)finalPass);
        }

        // 释放
        public override void Dispose(bool disposing) {
            base.Dispose(disposing);
            CoreUtils.Destroy(mMaterial);

            // 释放RTHandle
            for (int i = 0; i <= Iteration.value * 2; ++i)
                mTempRT[i]?.Release();

            mPrefilteredRT?.Release();
        }
    }

    [Serializable]
    public sealed class BloomModeParameter : VolumeParameter<Bloom.BloomModeEnum>{
        public BloomModeParameter(Bloom.BloomModeEnum value, bool overrideState = false) : base(value, overrideState) {
        }
    }
}