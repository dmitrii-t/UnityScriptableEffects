using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ARenderPassFeature : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        private readonly string m_ProfilingName;
        
        private readonly Material m_Material;
        
        private const string m_TmpRTName = "_TempTexture";
        
        private RTHandle m_TmpRT;
        
        private RTHandle m_CamRT;
        public CustomRenderPass(string profilingName, Material material)
        {
            m_ProfilingName = profilingName;
            m_Material = material;
        }

        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var commandBuffer = CommandBufferPool.Get(name: this.m_ProfilingName);
            commandBuffer.Clear();

            using (new ProfilingScope(commandBuffer, new ProfilingSampler(m_ProfilingName)))
            {
                // Setting up camera color RT
                var descriptor = renderingData.cameraData.cameraTargetDescriptor;
                // descriptor.msaaSamples = 1;
                descriptor.depthBufferBits = 0;
                
                // Setting up tmp color RT 
                RenderingUtils.ReAllocateIfNeeded(ref m_TmpRT, descriptor, FilterMode.Point, TextureWrapMode.Clamp, name: m_TmpRTName);
                
                var sourceRT = m_CamRT;
                var targetRT = m_TmpRT;
                
                // Blitting
                Blitter.BlitCameraTexture(commandBuffer, sourceRT, targetRT, RenderBufferLoadAction.DontCare,
                                          RenderBufferStoreAction.Store, m_Material, 0);
                
                sourceRT = m_TmpRT;
                targetRT = m_CamRT;
                
                Blitter.BlitCameraTexture(commandBuffer, sourceRT, targetRT);
                
                // Execute Blit
                context.ExecuteCommandBuffer(commandBuffer);
                commandBuffer.Clear();
            }

            // release the command buffer
            CommandBufferPool.Release(commandBuffer);
            
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            m_TmpRT?.Release();
        }

        public void SetRenderTargets(RTHandle camRT)
        {
            m_CamRT = camRT;
        }

    }

    CustomRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        var material = new Material(Shader.Find("Shader Graphs/RedColorShader"));
        
        m_ScriptablePass = new CustomRenderPass(profilingName: "ASampleRenderFeature", material);
        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }
    
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType != CameraType.Game)
        {
            return;
        }
        
        m_ScriptablePass.SetRenderTargets(renderer.cameraColorTargetHandle);
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType != CameraType.Game)
        {
            return;
        }
        
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


