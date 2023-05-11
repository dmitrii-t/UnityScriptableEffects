using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Common.Scripts
{
    public abstract class EffectPass<T> : ScriptableRenderPass where T : Effect
    {
        private readonly int m_MainTexID = Shader.PropertyToID("_MainTex");
        
        private readonly int m_MainTexSizeID = Shader.PropertyToID("_MainTexSize");
        
        protected RTHandle m_CamTexRT;

        protected RTHandle m_TmpTexRT;

        protected abstract string GetPassName();
        
        protected abstract void ExecutePass(T effect, CommandBuffer commandBuffer, ScriptableRenderContext context, ref RenderingData renderingData);
        
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
                // to perform operations on the textures, we need to use a command buffer. CB allows queuing up commands
                // to modify textures
                var commandBuffer = CommandBufferPool.Get(name: GetPassName());
                commandBuffer.Clear();

                using (new ProfilingScope(commandBuffer, new ProfilingSampler(GetPassName())))
                {
                    ExecutePass(effect, commandBuffer, context, ref renderingData);
                    
                    // never execute the command buffer by default
                    
                    commandBuffer.Clear();
                }
                
                // release the command buffer
                CommandBufferPool.Release(commandBuffer);
            }
        }
        
        protected void SetMaterialMainTex(Material material)
        {
            // main texture
            material.SetTexture(m_MainTexID, m_CamTexRT);
            var mainTextureSize = new Vector2(Screen.width, Screen.height);
            material.SetVector(m_MainTexSizeID, mainTextureSize);
        }

        public void SetRenderTargets(RTHandle camTexRT, RTHandle tmpTexRT)
        {
            m_CamTexRT = camTexRT;
            m_TmpTexRT = tmpTexRT;
        }
    }
}
