using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CPP.EFFECTS
{
    [VolumeComponentMenu(("Custom Post Processing/Blur/Mipmap Gaussian Blur"))]
    public class MipGaussianBlur : CustomPostProcessing
    {
        private const int MAXITERATION = 10;

        #region Properties Defination
        public BoolParameter EnableAdvanced = new BoolParameter(false);
        public ClampedFloatParameter Sigma = new ClampedFloatParameter(2.0f, 0.0f, 20.0f);
        public ClampedIntParameter Iteration = new ClampedIntParameter(0, 0, MAXITERATION);

        #endregion

        private const string mShaderName = "Hidden/PostProcessing/MipGaussianBlur";
        
        private int mSigmaKeyword = Shader.PropertyToID("_Sigma");
        private int mMaxLevelKeyword = Shader.PropertyToID("_MaxLevel");
        private int mCurrentLevelKeyword = Shader.PropertyToID("_CurrentLevel");
        private int mDownSampleTextureKeyword = Shader.PropertyToID("_DownSampleTexture");
        public override bool IsActive() => mMaterial != null && Iteration.value != 0;

        public override CustomPostProcessInjectionPoint InjectionPoint =>
            CustomPostProcessInjectionPoint.AfterPostProcess;

        public override int OrderInInjectionPoint => 5;

        private string mRTName => "_TemporaryRenderTexture";

        private RTHandle[] mDownSampleRTs = new RTHandle[MAXITERATION + 1];
        private RTHandle[] mUpSampleRTs = new RTHandle[MAXITERATION + 1];

        public override void Setup()
        {
            if (mMaterial == null)
            {
                mMaterial = CoreUtils.CreateEngineMaterial(mShaderName);
            }
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var descriptor = GetCameraRenderTextureDescriptor(renderingData);
            //取小于等于当前分辨率的最大2的幂次方
            descriptor.width = 1 << (int)Mathf.Log(descriptor.width, 2);
            descriptor.height = 1 << (int)Mathf.Log(descriptor.height, 2);
            
            //计算图像金字塔的最大层级
            var maxLevel = (int)Mathf.Log(Mathf.Max(descriptor.width, descriptor.height), 2);
            mMaterial.SetInt(mMaxLevelKeyword, maxLevel);
            
            mMaterial.SetFloat(mSigmaKeyword, Sigma.value);
            
            for (var i = 0; i <= Iteration.value; i++)
            {
                RenderingUtils.ReAllocateIfNeeded(ref mDownSampleRTs[i], descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp,
                    name: mRTName + i + "Down");
                RenderingUtils.ReAllocateIfNeeded(ref mUpSampleRTs[i], descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp,
                    name: mRTName + i + "Up");

                //下采样，若分辨率为1024，则下采样后为512，256，128，64，32，16，8，4，2，1
                //若分辨率大于1024，则只迭代10次，若分辨率小于1024，则只迭代到分辨率为1
                descriptor.width = Math.Max(descriptor.width / 2, 1);
                descriptor.height = Math.Max(descriptor.height / 2, 1);
            }
            
            if (EnableAdvanced.value)
            {
                Shader.EnableKeyword("ADVANCED_MIP_GAUSSIAN");
            }
            else
            {
                Shader.DisableKeyword("ADVANCED_MIP_GAUSSIAN");
            }
        }

        public override void Render(CommandBuffer cmd, ref RenderingData renderingData, in RTHandle source, in RTHandle destination)
        {
            if (mMaterial == null) return;
            
            //拷贝原始图像到下采样图像金字塔的第0层，即最大分辨率
            using (new ProfilingScope(cmd, new ProfilingSampler("Copy Raw Image to DownSample Level 0")))
            {
                Draw(cmd, source, mDownSampleRTs[0]);
            }

            //下采样生成图像金字塔
            using (new ProfilingScope(cmd, new ProfilingSampler("Generate DownSample Mipmap")))
            {
                for (var i = 0; i < Iteration.value; i++)
                {
                    Draw(cmd, mDownSampleRTs[i], mDownSampleRTs[i + 1], 0);
                }
            }

            //拷贝原始图像到下采样图像金字塔的第0层，即最大分辨率
            using (new ProfilingScope(cmd, new ProfilingSampler("Copy Max Level Image to UpSample Level 0")))
            {
                Draw(cmd, mDownSampleRTs[Iteration.value], mUpSampleRTs[Iteration.value]);
            }

            //上采样生成图像金字塔
            using (new ProfilingScope(cmd, new ProfilingSampler("Generate UpSample Mipmap")))
            {
                for (var i = Iteration.value - 1; i >= 0; i--)
                {
                    cmd.SetGlobalTexture(mDownSampleTextureKeyword, mDownSampleRTs[i]);
                    cmd.SetGlobalInt(mCurrentLevelKeyword, i);
                    Draw(cmd, mUpSampleRTs[i + 1], mUpSampleRTs[i], 1);
                }
            }


            Draw(cmd, mUpSampleRTs[0], destination, 1);
        }

        public override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            CoreUtils.Destroy(mMaterial);

            for (int i = 0; i < MAXITERATION; i++)
            {
                mDownSampleRTs[i]?.Release();
            }
        }
    }
}