using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

internal class ColorBlitRendererFeature : ScriptableRendererFeature{
    public Shader mShader;
    public float mIntensity;

    private Material mMaterial;
    private ColorBlitPass mRenderPass = null;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        if(renderingData.cameraData.cameraType == CameraType.Game)
            renderer.EnqueuePass(mRenderPass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData) {
        if (renderingData.cameraData.cameraType == CameraType.Game) {
            mRenderPass.ConfigureInput(ScriptableRenderPassInput.Color);
            mRenderPass.SetTarget(renderer.cameraColorTargetHandle, mIntensity);
        }
    }

    public override void Create() {
        mMaterial = CoreUtils.CreateEngineMaterial(mShader);
        mRenderPass = new ColorBlitPass(mMaterial);
    }

    protected override void Dispose(bool disposing) {
        CoreUtils.Destroy(mMaterial);
    }
}