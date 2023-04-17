using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RendererPixelart.Effect
{
    // [System.Serializable]
    public class PixelartRenderFeature : ScriptableRendererFeature
    {
        private PixelartRenderPass m_PixelartPass;

        private RTHandle m_tmpColorRT;

        private const string m_tmpColorTexName = "_TempTexture";

        /// <inheritdoc/>
        public override void Create()
        {
            // we configure where the render pass should be injected.
            m_PixelartPass = new PixelartRenderPass();
            m_PixelartPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType != CameraType.Game)
            {
                return;
            }

            // Setting up camera color RT
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.msaaSamples = 1;
            descriptor.depthBufferBits = 0;

            // Setting up tmp color RT 
            RenderingUtils.ReAllocateIfNeeded(ref m_tmpColorRT, descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: m_tmpColorTexName);

            m_PixelartPass.SetTarget(renderer.cameraColorTargetHandle, m_tmpColorRT);
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // we only want to run this pass on the main camera
            if (renderingData.cameraData.cameraType != CameraType.Game)
            {
                return;
            }

            renderer.EnqueuePass(m_PixelartPass);
        }

        protected override void Dispose(bool disposing)
        {
            m_tmpColorRT?.Release();
        }
    }
}
