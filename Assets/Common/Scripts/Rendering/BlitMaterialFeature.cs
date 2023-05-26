using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlitMaterialFeature<T> : ScriptableRendererFeature where T : Effect
{
    private static readonly int m_MainTexID = Shader.PropertyToID("_MainTex");
    
    private const string m_TmpRTName = "_TempTexture";
    
    private RTHandle m_TmpRT; 
    
    class BlitMaterialRenderPass : ScriptableRenderPass
    {
        private readonly string m_ProfilingName;

        private RTHandle m_CamRT;

        private RTHandle m_TmpRT;

        public BlitMaterialRenderPass(string profilingName) : base()
        {
            m_ProfilingName = profilingName;
        }

        public void SetRenderTargets(RTHandle camRT, RTHandle tmpRT)
        {
            m_CamRT = camRT;
            m_TmpRT = tmpRT;
        }

        private void Blit(ScriptableRenderContext context, CommandBuffer commandBuffer, T effect)
        {
            var material = effect.m_Material.value;
            var materialPassIndex = effect.m_MaterialPassIndex.value;
            
            var sourceRT = m_CamRT;
            var targetRT = m_TmpRT;

            material.SetTexture(m_MainTexID, m_CamRT);
            
            // Blit the camera texture to the temporary RT
            Blitter.BlitCameraTexture(commandBuffer, sourceRT, targetRT, RenderBufferLoadAction.DontCare,
                                      RenderBufferStoreAction.Store, material, materialPassIndex);

            sourceRT = m_TmpRT;
            targetRT = m_CamRT;

            // Blit the temporary RT to the camera RT
            Blitter.BlitCameraTexture(commandBuffer, sourceRT, targetRT);
            // Execute Blit
            context.ExecuteCommandBuffer(commandBuffer);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var stack = VolumeManager.instance.stack;
            
            var effect = stack.GetComponent<T>();
            if (effect.IsActive())
            {
                var commandBuffer = CommandBufferPool.Get(name: this.m_ProfilingName);
                commandBuffer.Clear();

                using (new ProfilingScope(commandBuffer, new ProfilingSampler(m_ProfilingName)))
                {
                    Blit(context, commandBuffer, effect);
                    commandBuffer.Clear();
                }

                // release the command buffer
                CommandBufferPool.Release(commandBuffer);
            }
        }
    }

    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    [SerializeField]
    private Settings settings = new Settings();

    private BlitMaterialRenderPass m_BlitMaterialRenderPass;

    public override void Create()
    {
        m_BlitMaterialRenderPass = new BlitMaterialRenderPass(name);
        m_BlitMaterialRenderPass.renderPassEvent = settings.renderEvent;
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        // only run this pass on the game camera
        // if (renderingData.cameraData.cameraType != CameraType.Game)
        // {
        //     return;
        // }

        // Setting up camera color RT
        var descriptor = renderingData.cameraData.cameraTargetDescriptor;
        // descriptor.msaaSamples = 1;
        descriptor.depthBufferBits = 0;

        // Setting up tmp color RT 
        RenderingUtils.ReAllocateIfNeeded(ref m_TmpRT, descriptor, FilterMode.Point, TextureWrapMode.Clamp, name: m_TmpRTName);

        m_BlitMaterialRenderPass.SetRenderTargets(renderer.cameraColorTargetHandle, m_TmpRT);
    }
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // only run this pass on the game camera
        // if (renderingData.cameraData.cameraType != CameraType.Game)
        // {
        //     Debug.Log($"Disabling ${name} feature");
        //     return;
        // }
        
        renderer.EnqueuePass(m_BlitMaterialRenderPass);
    }
    
    protected override void Dispose(bool disposing)
    {
        m_TmpRT?.Release();
    }
}
