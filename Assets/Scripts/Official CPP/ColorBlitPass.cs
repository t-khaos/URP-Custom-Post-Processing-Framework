using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

internal class ColorBlitPass : ScriptableRenderPass{
    private ProfilingSampler mProfilingSampler = new ProfilingSampler("Color Blit");
    private Material mMaterial;
    private RTHandle mCameraColorTarget;
    private float mIntensity;

    public ColorBlitPass(Material material) {
        mMaterial = material;
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public void SetTarget(RTHandle colorHandle, float intensity) {
        mCameraColorTarget = colorHandle;
        mIntensity = intensity;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
        ConfigureTarget(mCameraColorTarget);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
        var cameraData = renderingData.cameraData;
        if (cameraData.cameraType != CameraType.Game) return;
        if (mMaterial == null) return;

        CommandBuffer cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, mProfilingSampler)) {
            mMaterial.SetFloat("_Intensity", mIntensity);
            Blitter.BlitCameraTexture(cmd, mCameraColorTarget, mCameraColorTarget, mMaterial, 0);
        }
        
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        
        CommandBufferPool.Release(cmd);
    }
}