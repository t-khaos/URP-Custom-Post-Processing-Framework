using System;
using CPP;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CPP.EFFECTS{
    [VolumeComponentMenu("Custom Post Processing/Glitch/Glitch RGB Split")]
    public class GlitchRGBSplit : CustomPostProcessing{
        #region Properties Defination

        public enum FrequencyTypeEnum{
            Constant = 0,
            Random = 1,
            Infinite = 2,
        }

        public enum SplitDirectionEnum{
            Horizontal = 0,
            Vertical = 1,
            Mixed = 2,
        }

        public SplitDirectionParameter SplitDirection = new SplitDirectionParameter(SplitDirectionEnum.Horizontal);
        public FrequencyTypeParameter FrequencyType = new FrequencyTypeParameter(FrequencyTypeEnum.Constant);
        public ClampedFloatParameter Frequency = new ClampedFloatParameter(3.0f, 0.1f, 25.0f);
        public ClampedFloatParameter Amount = new ClampedFloatParameter(0.0f, 0.0f, 200.0f);
        public ClampedFloatParameter Speed = new ClampedFloatParameter(20.0f, 0.0f, 30.0f);

        #endregion

        private const string mShaderName = "Hidden/PostProcessing/GlitchRGBSplit";

        private const string mInfiniteFrequencyKeyword = "_INFINITEFREQUENCY",
            mGlitchRGBSplitParamsKeyword = "_GlitchRGBSplitParams";

        public override bool IsActive() => mMaterial != null && Amount.value != 0.0f;

        public override CustomPostProcessInjectionPoint InjectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

        public override int OrderInInjectionPoint => 3;

        private float mRandomFrequency = 0.0f;
        private int mFrameCount = 0;

        public override void Setup() {
            if (mMaterial == null)
                mMaterial = CoreUtils.CreateEngineMaterial(mShaderName);
        }

        public override void Render(CommandBuffer cmd, ref RenderingData renderingData, in RTHandle source, in RTHandle destination) {
            if (mMaterial == null) return;

            UpdateFrequency();

            cmd.SetGlobalVector(mGlitchRGBSplitParamsKeyword, new Vector4(FrequencyType.value == FrequencyTypeEnum.Random ? mRandomFrequency : Frequency.value, Amount.value, Speed.value, 0.0f));

            Draw(cmd, source, destination, (int)SplitDirection.value);
        }

        private void UpdateFrequency() {

            if (FrequencyType.value == FrequencyTypeEnum.Random) {
                if (mFrameCount > Frequency.value) {
                    mFrameCount = 0;
                    mRandomFrequency = UnityEngine.Random.Range(0, Frequency.value);
                }

                mFrameCount++;
            }

            SetKeyword(mInfiniteFrequencyKeyword, FrequencyType == FrequencyTypeEnum.Infinite);
        }


        public override void Dispose(bool disposing) {
            base.Dispose(disposing);
            CoreUtils.Destroy(mMaterial);
        }
    }

    [Serializable]
    public sealed class SplitDirectionParameter : VolumeParameter<GlitchRGBSplit.SplitDirectionEnum>{
        public SplitDirectionParameter(GlitchRGBSplit.SplitDirectionEnum value, bool overrideState = false) : base(value, overrideState) {
        }
    }


    [Serializable]
    public sealed class FrequencyTypeParameter : VolumeParameter<GlitchRGBSplit.FrequencyTypeEnum>{
        public FrequencyTypeParameter(GlitchRGBSplit.FrequencyTypeEnum value, bool overrideState = false) : base(value, overrideState) {
        }
    }
}