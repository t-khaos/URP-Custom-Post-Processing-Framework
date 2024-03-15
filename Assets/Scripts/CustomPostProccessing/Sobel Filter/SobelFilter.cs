using CPP;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CPP.EFFECTS{
    [VolumeComponentMenu("Custom Post Processing/Stylized/Sobel Filter")]
    public class SobelFilter : CustomPostProcessing{
        public ClampedFloatParameter lineThickness = new(0f, .0005f, .0025f);
        public BoolParameter outLineOnly = new(false);
        public BoolParameter posterize = new(false);
        public IntParameter count = new(6);

        private const string ShaderName = "Hidden/PostProcess/SobleFilter";

        public override CustomPostProcessInjectionPoint InjectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

        public override void Setup() {
            if (mMaterial == null)
                mMaterial = CoreUtils.CreateEngineMaterial(ShaderName);
        }

        public override bool IsActive() => mMaterial != null && lineThickness.value > 0f;

        public override void Render(CommandBuffer cmd, ref RenderingData renderingData, in RTHandle source, in RTHandle destination) {
            if (mMaterial == null)
                return;

            mMaterial.SetFloat("_Delta", lineThickness.value);
            mMaterial.SetInt("_PosterizationCount", count.value);
            SetKeyword("RAW_OUTLINE", outLineOnly.value);
            SetKeyword("POSTERIZEE", posterize.value);

            Draw(cmd, source, destination, 0);
        }

        public override void Dispose(bool disposing) {
            base.Dispose(disposing);
            CoreUtils.Destroy(mMaterial);
        }
    }
}