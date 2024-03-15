using CPP;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CPP.EFFECTS{
    [VolumeComponentMenu("Custom Post Processing/Color Adjustment/Color Blit")]
    public class ColorBlit : CustomPostProcessing{
        public ClampedFloatParameter intensity = new(0.0f, 0.0f, 2.0f);

        private const string mShaderName = "Hidden/PostProcess/ColorBlit";

        public override bool IsActive() => mMaterial != null && intensity.value > 0;

        public override CustomPostProcessInjectionPoint InjectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;
        public override int OrderInInjectionPoint => 2;

        public override void Setup() {
            if (mMaterial == null)
                mMaterial = CoreUtils.CreateEngineMaterial(mShaderName);
        }

        public override void Render(CommandBuffer cmd, ref RenderingData renderingData, in RTHandle source, in RTHandle destination) {
            if (mMaterial == null) return;
            mMaterial.SetFloat("_Intensity", intensity.value);
            Draw(cmd, source, destination, 0);
        }

        public override void Dispose(bool disposing) {
            base.Dispose(disposing);
            CoreUtils.Destroy(mMaterial);
        }
    }
}