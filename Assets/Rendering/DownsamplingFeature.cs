using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DownsamplingFeature : ScriptableRendererFeature
{
    private static readonly int m_MainTexID = Shader.PropertyToID("_MainTex");

    private const string m_TmpRTName = "_TempTexture";

    private RTHandle m_TmpRT;

    class DownsamplingRenderPass : ScriptableRenderPass
    {
        private readonly string m_ProfilingName;

        private RTHandle m_CamRT;

        private RTHandle m_TmpRT;
        
        public DownsamplingRenderPass(string profilingName) : base()
        {
            m_ProfilingName = profilingName;
        }

        public void SetRenderTargets(RTHandle camRT)
        {
            m_CamRT = camRT;
        }
        
        private bool BlitWithDownsampling(ScriptableRenderContext context, RenderingData renderingData, CommandBuffer commandBuffer,
            int downsample, RTHandle sourceRT, bool condition)
        {
            if (condition)
            {
                var camera = renderingData.cameraData.camera;

                var w = camera.scaledPixelWidth;
                var h = camera.scaledPixelHeight;

                var temporaryRts = new RTHandle[downsample];

                for (var i = 0; i < downsample; i++)
                {
                    w >>= 1;
                    h >>= 1;

                    if (h < 2)
                    {
                        break;
                    }

                    // Setting up camera color RT
                    var descriptor = renderingData.cameraData.cameraTargetDescriptor;
                    descriptor.depthBufferBits = 0;
                    descriptor.width = w;
                    descriptor.height = h;

                    // Allocating tmp RT 
                    RenderingUtils.ReAllocateIfNeeded(ref temporaryRts[i], descriptor, FilterMode.Point, TextureWrapMode.Clamp, name: $"_TempRT{i}");

                    // Blit the source texture to the temp RT
                    Blitter.BlitCameraTexture(commandBuffer, sourceRT, temporaryRts[i]);

                    sourceRT = temporaryRts[i];
                }
                
                var targetRT = m_CamRT;
                
                // Blit the temporary RT to the camera RT
                Blitter.BlitCameraTexture(commandBuffer, sourceRT, targetRT);
                // Execute Blit
                context.ExecuteCommandBuffer(commandBuffer);
                
                return true;
            }
            return false;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var stack = VolumeManager.instance.stack;

            var effect = stack.GetComponent<DownsamplingComponent>();
            if (effect.IsActive())
            {
                var commandBuffer = CommandBufferPool.Get(name: this.m_ProfilingName);
                commandBuffer.Clear();

                using (new ProfilingScope(commandBuffer, new ProfilingSampler(m_ProfilingName)))
                {
                    var downsample = effect.m_Downsample.value;
                    BlitWithDownsampling(context, renderingData, commandBuffer, downsample, m_CamRT, downsample > 0);

                    commandBuffer.Clear();
                }

                // release the command buffer
                CommandBufferPool.Release(commandBuffer);
            }
        }
    }

    [Serializable]
    public class Settings
    {
        public RenderPassEvent renderEvent = RenderPassEvent.AfterRenderingOpaques;
        public bool isGameCameraOnly = false;
    }

    [SerializeField]
    private Settings settings = new Settings();

    private DownsamplingRenderPass m_DownsamplingRenderPass;

    public override void Create()
    {
        m_DownsamplingRenderPass = new DownsamplingRenderPass(name);
        m_DownsamplingRenderPass.renderPassEvent = settings.renderEvent;
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        // only run this pass on the game camera
        if (settings.isGameCameraOnly && renderingData.cameraData.cameraType != CameraType.Game)
        {
            Debug.Log($"${name} feature disabled");
            return;
        }
        
        m_DownsamplingRenderPass.SetRenderTargets(renderer.cameraColorTargetHandle);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // only run this pass on the game camera
        if (settings.isGameCameraOnly && renderingData.cameraData.cameraType != CameraType.Game)
        {
            Debug.Log($"${name} feature disabled");
            return;
        }

        renderer.EnqueuePass(m_DownsamplingRenderPass);
    }

    protected override void Dispose(bool disposing)
    {
        m_TmpRT?.Release();
    }
}
