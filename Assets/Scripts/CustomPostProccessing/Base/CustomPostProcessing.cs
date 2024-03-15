using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CPP{
    // 声明注入点的插入Event
    public enum CustomPostProcessInjectionPoint{
        AfterOpauqe,
        AfterSkybox,
        BeforePostProcess,
        AfterPostProcess
    }

    public abstract class CustomPostProcessing : VolumeComponent, IPostProcessComponent, IDisposable{
        // 材质声明
        protected Material mMaterial = null;
        static public Material copyMaterial = null;

        private const string mCopyShaderName = "Hidden/PostProcess/PostProcessCopy";

        // 注入点
        public virtual CustomPostProcessInjectionPoint InjectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

        //  在注入点的顺序
        public virtual int OrderInInjectionPoint => 0;

        protected override void OnEnable() {
            base.OnEnable();
            if (copyMaterial == null) {
                copyMaterial = CoreUtils.CreateEngineMaterial(mCopyShaderName);
            }
        }

        #region Setup

        // 后处理是否激活
        public abstract bool IsActive();

        // 当前相机
        public abstract void Setup();

        // 当相机初始化时执行
        public virtual void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
        }

        #endregion


        #region Render

        // 执行渲染
        public abstract void Render(CommandBuffer cmd, ref RenderingData renderingData, in RTHandle source, in RTHandle destination);

        // 绘制全屏三角形
        private int mSourceTextureId = Shader.PropertyToID("_SourceTexture");

        public virtual void Draw(CommandBuffer cmd, in RTHandle source, in RTHandle destination, int pass = -1) {
            // 将GPU端_SourceTexture设置为source
            cmd.SetGlobalTexture(mSourceTextureId, source);
            // 将RT设置为destination 不关心初始状态(直接填充) 需要存储
            CoreUtils.SetRenderTarget(cmd, destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            // 绘制程序化三角形
            if (pass == -1 || mMaterial == null)
                cmd.DrawProcedural(Matrix4x4.identity, copyMaterial, 0, MeshTopology.Triangles, 3);
            else
                cmd.DrawProcedural(Matrix4x4.identity, mMaterial, pass, MeshTopology.Triangles, 3);
        }

        // 获取相机描述符
        protected RenderTextureDescriptor GetCameraRenderTextureDescriptor(RenderingData renderingData) {
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.msaaSamples = 1;
            descriptor.depthBufferBits = 0;
            descriptor.useMipMap = false;
            return descriptor;
        }

        // 设置keyword
        // 在OnCameraSetUp函数中使用，渲染时用CoreUtils.SetKeyword
        protected void SetKeyword(string keyword, bool enabled = true) {
            if (enabled) mMaterial.EnableKeyword(keyword);
            else mMaterial.DisableKeyword(keyword);
        }

        #endregion

        public virtual bool IsTileCompatible() => false;

        #region IDisposable

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing) {
        }

        #endregion
    }
}