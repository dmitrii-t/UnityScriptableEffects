using Common.Scripts;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace EffectPixelart.Effect
{
    // [System.Serializable]
    public class PixelartEffectPass : EffectPass<PixelartEffectComponent>
    {
        private readonly int m_PatternID = Shader.PropertyToID("_Pattern");
        
        private readonly int m_PatternTexSizeID = Shader.PropertyToID("_PatternTexSize");

        private readonly int m_PrimaryID = Shader.PropertyToID("_Primary");

        private readonly int m_SecondaryID = Shader.PropertyToID("_Secondary");

        private readonly int m_RemapID = Shader.PropertyToID("_Remap");
        
        private readonly int m_Posterize = Shader.PropertyToID("_Posterize");

        public PixelartEffectPass()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        protected override string GetPassName() => "Pixelart Effect Pass";

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
            var effect = stack.GetComponent<PixelartEffectComponent>();

            if (effect.IsActive())
            {
                // to perform operations on the textures, we need to use a command buffer. CB allows queuing up commands to modify textures
                var commandBuffer = CommandBufferPool.Get(GetPassName());
                commandBuffer.Clear();

                using (new ProfilingScope(commandBuffer, new ProfilingSampler(GetPassName())))
                {
                    var sourceRT = m_CamTexRT;

                    // Downsample the texture
                    DownsampleWhenTrue(ref sourceRT, effect, renderingData, commandBuffer, condition: effect.m_Downsample.value > 0);

                    // Add Posterizing Effect
                    var posterizeMaterial = effect.m_PosterizeMaterial.value;
                    PosteriseWhenTrue(ref sourceRT, effect, commandBuffer, posterizeMaterial,  condition: posterizeMaterial != null);
                    
                    // Add Dithering Effect
                    var ditherMaterial = effect.m_DitherMaterial.value;
                    DitheringWhenTrue(ref sourceRT, effect, commandBuffer, ditherMaterial, condition: ditherMaterial != null);

                    Blitter.BlitCameraTexture(commandBuffer, sourceRT, m_CamTexRT);

                    // execute the command buffer
                    context.ExecuteCommandBuffer(commandBuffer);

                    // release the command buffer
                    commandBuffer.Clear();
                    CommandBufferPool.Release(commandBuffer);

                    // release the temp RTs when downsampling enabled
                    // ReleaseRTHandlesWhenTrue(temporaryRts, condition: effect.m_DownSample.value > 0);
                }
            } // eof Execute
        }

        private bool DitheringWhenTrue(ref RTHandle sourceRT, PixelartEffectComponent effect, CommandBuffer commandBuffer,
            Material ditherMaterial,  bool condition)
        {
            if (condition)
            {
                // setting the shader properties
                SetMainTextureProperties(ditherMaterial);

                // pattern texture
                var patternTexture = effect.m_Pattern.value;
                ditherMaterial.SetTexture(m_PatternID, patternTexture);
                var patternTextureSize = new Vector2(effect.m_Pattern.value.width, effect.m_Pattern.value.height);
                ditherMaterial.SetVector(m_PatternTexSizeID, patternTextureSize);

                ditherMaterial.SetTexture(m_PrimaryID, effect.m_Primary.value);
                ditherMaterial.SetTexture(m_SecondaryID, effect.m_Secondary.value);
                ditherMaterial.SetVector(m_RemapID, effect.m_Remap.value);

                // Dithering
                Blitter.BlitCameraTexture(commandBuffer, sourceRT, m_TmpTexRT, RenderBufferLoadAction.DontCare,
                                          RenderBufferStoreAction.Store, ditherMaterial, 0);

                sourceRT = m_TmpTexRT;

                return true;
            }

            return false;
        }

        private bool PosteriseWhenTrue(ref RTHandle sourceRT, PixelartEffectComponent effect, CommandBuffer commandBuffer,
            Material posterizeMaterial,  bool condition)
        {
            if (condition)
            {
                // setting the shader properties
                SetMainTextureProperties(posterizeMaterial);
                
                posterizeMaterial.SetInt(m_Posterize, effect.m_Posterize.value);
                
                // Posterize
                Blitter.BlitCameraTexture(commandBuffer, sourceRT, m_TmpTexRT, RenderBufferLoadAction.DontCare,
                                          RenderBufferStoreAction.Store, posterizeMaterial, 0);

                sourceRT = m_TmpTexRT;

                return true;
            }

            return false;
        }
        
        private bool DownsampleWhenTrue(ref RTHandle sourceRT, PixelartEffectComponent effect,
            RenderingData renderingData, CommandBuffer commandBuffer,  bool condition)
        {
            if (condition)
            {
                var camera = renderingData.cameraData.camera;

                var w = camera.scaledPixelWidth;
                var h = camera.scaledPixelHeight;

                var temporaryRts = new RTHandle[effect.m_Downsample.value];

                for (var i = 0; i < effect.m_Downsample.value; i++)
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
                return true;
                
            }
            return false;
        }

        static void ReleaseRTHandlesWhenTrue(RTHandle[] tempRTs, bool condition)
        {
            if (condition)
            {
                foreach (var t in tempRTs)
                {
                    t?.Release();
                }
            }
        }
    }
}
