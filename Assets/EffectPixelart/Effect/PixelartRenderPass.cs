using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace EffectPixelart.Effect
{
    // [System.Serializable]
    public class PixelartRenderPass : ScriptableRenderPass
    {
        private readonly int m_MainTexID = Shader.PropertyToID("_MainTex");

        private readonly int m_PixelRateID = Shader.PropertyToID("_PixelRate");

        private RTHandle m_CamTexRT;
        
        private RTHandle m_TmpTexRT;

        
        public void SetRenderTargets(RTHandle camTexRT, RTHandle tmpTexRT)
        {
            m_CamTexRT = camTexRT;
            m_TmpTexRT = tmpTexRT;
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // we only want to run this pass on the main camera
            if (renderingData.cameraData.cameraType != CameraType.Game)
            {
                return;
            }

            var stack = VolumeManager.instance.stack;
            var pixelartEffect = stack.GetComponent<PixelartEffectComponent>();

            if (pixelartEffect.IsActive())
            {
                // to perform operations on the textures, we need to use a command buffer. CB allows queuing up commands to modify textures
                var commandBuffer = CommandBufferPool.Get(name: "PixelartPass");
                commandBuffer.Clear();

                using (new ProfilingScope(commandBuffer, new ProfilingSampler("Pixelart Render Pass")))
                {
                    var material = pixelartEffect.m_Material.value;

                    if (material != null)
                    {
                        // setting the shader properties
                        material.SetFloat(m_PixelRateID, pixelartEffect.m_PixelRate.value);
                        material.SetTexture(m_MainTexID, m_CamTexRT);

                        Blitter.BlitCameraTexture(commandBuffer, m_CamTexRT, m_TmpTexRT, RenderBufferLoadAction.DontCare,
                                                  RenderBufferStoreAction.Store, material, 0);

                        Blitter.BlitCameraTexture(commandBuffer, m_TmpTexRT, m_CamTexRT);

                        // execute the command buffer
                        context.ExecuteCommandBuffer(commandBuffer);
                        commandBuffer.Clear();
                    }

                    // release the command buffer
                    CommandBufferPool.Release(commandBuffer);
                }
            }
        }
    }
}
