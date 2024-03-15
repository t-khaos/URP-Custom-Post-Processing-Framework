using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CPP{
    public class CustomProcessRendererFeature : ScriptableRendererFeature{
        [SerializeField] public bool NormalTexture = false; // 当关闭SSAO或SSAO使用Depth Only时，开启此选项渲染法线图

        // 不同插入点的render pass
        private CustomPostProcessingPass mAfterOpaquePass;
        private CustomPostProcessingPass mAfterSkyboxPass;
        private CustomPostProcessingPass mBeforePostProcessPass;
        private CustomPostProcessingPass mAfterPostProcessPass;

        private DepthNormalsPass mDepthNormalsPass;

        // 所有后处理基类列表
        private List<CustomPostProcessing> mCustomPostProcessings;

        // 获取所有的CustomPostProcessing实例，并且根据插入点排序，放入到对应Render Pass中，并且指定Pass Event
        public override void Create() {
            // 获取VolumeStack
            var stack = VolumeManager.instance.stack;

            // 获取所有的CustomPostProcessing实例
            mCustomPostProcessings = VolumeManager.instance.baseComponentTypeArray
                .Where(t => t.IsSubclassOf(typeof(CustomPostProcessing))) // 筛选出VolumeComponent派生类类型中所有的CustomPostProcessing类型元素 不论是否在Volume中 不论是否激活
                .Select(t => stack.GetComponent(t) as CustomPostProcessing) // 将类型元素转换为实例
                .ToList(); // 转换为List

            // 初始化不同插入点的render pass
            var afterOpaqueCPPs = mCustomPostProcessings
                .Where(c => c.InjectionPoint == CustomPostProcessInjectionPoint.AfterOpauqe)
                .OrderBy(c => c.OrderInInjectionPoint)
                .ToList();
            mAfterOpaquePass = new CustomPostProcessingPass("Custom PostProcess after Opaque", afterOpaqueCPPs);
            mAfterOpaquePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

            // 找到在透明物和天空后渲染的CustomPostProcessing
            var afterSkyboxCPPs = mCustomPostProcessings
                .Where(c => c.InjectionPoint == CustomPostProcessInjectionPoint.AfterSkybox) // 筛选出所有CustomPostProcessing类中注入点为透明物体和天空后的实例
                .OrderBy(c => c.OrderInInjectionPoint) // 按照顺序排序
                .ToList(); // 转换为List
            // 创建CustomPostProcessingPass类
            mAfterSkyboxPass = new CustomPostProcessingPass("Custom PostProcess after Skybox", afterSkyboxCPPs);
            // 设置Pass执行时间
            mAfterSkyboxPass.renderPassEvent = RenderPassEvent.AfterRenderingSkybox;

            var beforePostProcessingCPPs = mCustomPostProcessings
                .Where(c => c.InjectionPoint == CustomPostProcessInjectionPoint.BeforePostProcess)
                .OrderBy(c => c.OrderInInjectionPoint)
                .ToList();
            mBeforePostProcessPass = new CustomPostProcessingPass("Custom PostProcess before PostProcess", beforePostProcessingCPPs);
            mBeforePostProcessPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

            var afterPostProcessCPPs = mCustomPostProcessings
                .Where(c => c.InjectionPoint == CustomPostProcessInjectionPoint.AfterPostProcess)
                .OrderBy(c => c.OrderInInjectionPoint)
                .ToList();
            mAfterPostProcessPass = new CustomPostProcessingPass("Custom PostProcess after PostProcessing", afterPostProcessCPPs);
            mAfterPostProcessPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

            mDepthNormalsPass = new DepthNormalsPass();
        }

        // 当为每个摄像机设置一个渲染器时，调用此方法
        // 将不同注入点的RenderPass注入到renderer中
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
            // 当前渲染的相机支持后处理
            if (renderingData.cameraData.postProcessEnabled) {
                // 为每个render pass设置RT
                // 并且将pass列表加到renderer中
                if (mAfterOpaquePass.SetupCustomPostProcessing()) {
                    mAfterOpaquePass.ConfigureInput(ScriptableRenderPassInput.Color);
                    renderer.EnqueuePass(mAfterOpaquePass);
                }

                if (mAfterSkyboxPass.SetupCustomPostProcessing()) {
                    mAfterSkyboxPass.ConfigureInput(ScriptableRenderPassInput.Color);
                    renderer.EnqueuePass(mAfterSkyboxPass);
                }

                if (mBeforePostProcessPass.SetupCustomPostProcessing()) {
                    mBeforePostProcessPass.ConfigureInput(ScriptableRenderPassInput.Color);
                    renderer.EnqueuePass(mBeforePostProcessPass);
                }

                if (mAfterPostProcessPass.SetupCustomPostProcessing()) {
                    mAfterPostProcessPass.ConfigureInput(ScriptableRenderPassInput.Color);
                    renderer.EnqueuePass(mAfterPostProcessPass);
                }

                if (NormalTexture) {
                    renderer.EnqueuePass(mDepthNormalsPass);
                }
            }

            // 如果需要渲染法线，则入队
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            mAfterSkyboxPass.Dispose();
            mBeforePostProcessPass.Dispose();
            mAfterPostProcessPass.Dispose();

            if (mCustomPostProcessings != null) {
                foreach (var item in mCustomPostProcessings) {
                    item.Dispose();
                }
            }
        }

        // 渲染法线Pass
        private class DepthNormalsPass : ScriptableRenderPass{
            // 相机初始化
            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
                // 设置输入为Normal，让Unity RP添加DepthNormalPrepass Pass
                ConfigureInput(ScriptableRenderPassInput.Normal);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
                // 什么都不做，我们只需要在相机初始化时配置DepthNormals即可
            }
        }
    }
}