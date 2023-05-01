using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Common.Scripts
{
    public abstract class EffectPass<T> : ScriptableRenderPass where T : Effect
    {
        protected RTHandle m_CamTexRT;

        protected RTHandle m_TmpTexRT;


        public abstract string GetPassName();
        
        public abstract void SetMaterialProperties(T effect, Material material);

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
            Checks.CheckNotNull(m_CamTexRT, "m_CamTexRT is null");
            Checks.CheckNotNull(m_TmpTexRT, "m_TmpTexRT is null");

            // we only want to run this pass on the main camera
            if (renderingData.cameraData.cameraType != CameraType.Game)
            {
                return;
            }

            var stack = VolumeManager.instance.stack;
            var effect = stack.GetComponent<T>();

            if (effect.IsActive())
            {
                // to perform operations on the textures, we need to use a command buffer. CB allows queuing up commands to modify textures
                var commandBuffer = CommandBufferPool.Get(name: "PixelartPass");
                commandBuffer.Clear();

                using (new ProfilingScope(commandBuffer, new ProfilingSampler(GetPassName())))
                {
                    var material = effect.m_Material.value;

                    if (material != null)
                    {
                        // setting the shader properties
                        SetMaterialProperties(effect, material);

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
