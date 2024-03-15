using CPP;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Vector3 = System.Numerics.Vector3;

namespace CPP.EFFECTS{
    [VolumeComponentMenu(("Custom Post Processing/AO/SSAO"))]
    public class SSAO : CustomPostProcessing{
        #region Parameters Defination

        public MinFloatParameter Intensity = new MinFloatParameter(0.0f, 0.0f);
        public MinFloatParameter Radius = new MinFloatParameter(0.25f, 0.0f);
        public MinFloatParameter FalloffDistance = new MinFloatParameter(100.0f, 0.0f);

        #endregion

        private const string mShaderName = "Hidden/PostProcessing/SSAO";

        public override bool IsActive() => mMaterial != null && Intensity.value != 0.0f;

        public override CustomPostProcessInjectionPoint InjectionPoint => CustomPostProcessInjectionPoint.AfterOpauqe;

        public override int OrderInInjectionPoint => 100;

        private static readonly int mProjectionParams2ID = Shader.PropertyToID("_ProjectionParams2"),
            mCameraViewTopLeftCornerID = Shader.PropertyToID("_CameraViewTopLeftCorner"),
            mCameraViewXExtentID = Shader.PropertyToID("_CameraViewXExtent"),
            mCameraViewYExtentID = Shader.PropertyToID("_CameraViewYExtent"),
            mSSAOParamsID = Shader.PropertyToID("_SSAOParams"),
            mSSAOBlurRadiusID = Shader.PropertyToID("_SSAOBlurRadius");

        private RTHandle mSSAOTexture0RT, mSSAOTexture1RT;

        private const string mSSAOTexture0Name = "_SSAO_OcclusionTexture0",
            mSSAOTexture1Name = "_SSAO_OcclusionTexture1";

        public override void Setup() {
            if (mMaterial == null)
                mMaterial = CoreUtils.CreateEngineMaterial(mShaderName);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
            Matrix4x4 view = renderingData.cameraData.GetViewMatrix();
            Matrix4x4 proj = renderingData.cameraData.GetProjectionMatrix();
            Matrix4x4 vp = proj * view;

            // 将camera view space 的平移置为0，用来计算world space下相对于相机的vector
            Matrix4x4 cview = view;
            cview.SetColumn(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
            Matrix4x4 cviewProj = proj * cview;

            // 计算viewProj逆矩阵，即从裁剪空间变换到世界空间
            Matrix4x4 cviewProjInv = cviewProj.inverse;

            // 计算世界空间下，近平面四个角的坐标
            var near = renderingData.cameraData.camera.nearClipPlane;
            // Vector4 topLeftCorner = cviewProjInv * new Vector4(-near, near, -near, near);
            // Vector4 topRightCorner = cviewProjInv * new Vector4(near, near, -near, near);
            // Vector4 bottomLeftCorner = cviewProjInv * new Vector4(-near, -near, -near, near);
            Vector4 topLeftCorner = cviewProjInv.MultiplyPoint(new Vector4(-1.0f, 1.0f, -1.0f, 1.0f));
            Vector4 topRightCorner = cviewProjInv.MultiplyPoint(new Vector4(1.0f, 1.0f, -1.0f, 1.0f));
            Vector4 bottomLeftCorner = cviewProjInv.MultiplyPoint(new Vector4(-1.0f, -1.0f, -1.0f, 1.0f));

            // 计算相机近平面上方向向量
            Vector4 cameraXExtent = topRightCorner - topLeftCorner;
            Vector4 cameraYExtent = bottomLeftCorner - topLeftCorner;

            near = renderingData.cameraData.camera.nearClipPlane;

            // 发送ReconstructViewPos参数
            mMaterial.SetVector(mCameraViewTopLeftCornerID, topLeftCorner);
            mMaterial.SetVector(mCameraViewXExtentID, cameraXExtent);
            mMaterial.SetVector(mCameraViewYExtentID, cameraYExtent);
            mMaterial.SetVector(mProjectionParams2ID, new Vector4(1.0f / near, renderingData.cameraData.worldSpaceCameraPos.x, renderingData.cameraData.worldSpaceCameraPos.y, renderingData.cameraData.worldSpaceCameraPos.z));

            // 发送SSAO参数
            mMaterial.SetVector(mSSAOParamsID, new Vector4(Intensity.value, Radius.value * 1.5f, FalloffDistance.value));

            // 分配SSAO Texture
            var desc = GetCameraRenderTextureDescriptor(renderingData);
            desc.colorFormat = RenderTextureFormat.ARGB32;
            RenderingUtils.ReAllocateIfNeeded(ref mSSAOTexture0RT, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: mSSAOTexture0Name);
            RenderingUtils.ReAllocateIfNeeded(ref mSSAOTexture1RT, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: mSSAOTexture1Name);
        }


        public override void Render(CommandBuffer cmd, ref RenderingData renderingData, in RTHandle source, in RTHandle destination) {
            if (mMaterial == null) return;

            // SSAO
            Draw(cmd, source, mSSAOTexture0RT, 0);

            // Horizontal Blur
            cmd.SetGlobalVector(mSSAOBlurRadiusID, new Vector4(1.0f, 0.0f, 0.0f, 0.0f));
            Draw(cmd, mSSAOTexture0RT, mSSAOTexture1RT, 1);

            // Vertical Blur
            cmd.SetGlobalVector(mSSAOBlurRadiusID, new Vector4(0.0f, 1.0f, 0.0f, 0.0f));
            Draw(cmd, mSSAOTexture1RT, mSSAOTexture0RT, 1);

            // Final Pass
            cmd.SetGlobalTexture("_SSAOSourceTexture", source);
            Draw(cmd, mSSAOTexture0RT, destination, 2);
        }

        public override void Dispose(bool disposing) {
            base.Dispose(disposing);
            CoreUtils.Destroy(mMaterial);

            mSSAOTexture0RT?.Release();
            mSSAOTexture1RT?.Release();
        }
    }
}