using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CPP.EFFECTS{
    [VolumeComponentMenu(("Custom Post Processing/Blur/Radial Blur"))]
    public class RadialBlur : CustomPostProcessing{
        public ClampedFloatParameter BlurSpread = new ClampedFloatParameter(0.6f, 0.0f, 1.0f);
        public ClampedIntParameter Iteration = new ClampedIntParameter(0, 0, 30);
        public ClampedFloatParameter RadialCenterX = new ClampedFloatParameter(0.5f, 0.0f, 1.0f);
        public ClampedFloatParameter RadialCenterY = new ClampedFloatParameter(0.5f, 0.0f, 1.0f);

        private const string mShaderName = "Hidden/PostProcessing/RadialBlur";

        private int mRidialParamsKeyword = Shader.PropertyToID("_RadialParams");

        public override bool IsActive() => mMaterial != null && Iteration.value != 0;

        public override CustomPostProcessInjectionPoint InjectionPoint => CustomPostProcessInjectionPoint.BeforePostProcess;

        public override int OrderInInjectionPoint => 5;

        public override void Setup() {
            if (mMaterial == null)
                mMaterial = CoreUtils.CreateEngineMaterial(mShaderName);
        }


        public override void Render(CommandBuffer cmd, ref RenderingData renderingData, in RTHandle source, in RTHandle destination) {
            if (mMaterial == null) return;

            cmd.SetGlobalVector(mRidialParamsKeyword, new Vector4(Iteration.value, BlurSpread.value * 0.02f, RadialCenterX.value, RadialCenterY.value));
            Draw(cmd, source, destination, 0);
        }

        public override void Dispose(bool disposing) {
            base.Dispose(disposing);
            CoreUtils.Destroy(mMaterial);
        }
    }
}