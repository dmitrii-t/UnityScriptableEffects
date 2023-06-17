using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// [System.Serializable]
public class PixelartEffectPass : EffectPass<PixelartBlitMaterialComponentComponent>
{
    private readonly int m_PatternID = Shader.PropertyToID("_Pattern");
    private readonly int m_PatternTexSizeID = Shader.PropertyToID("_PatternTexSize");
    private readonly int m_Posterize = Shader.PropertyToID("_Posterize");
    private readonly int m_PrimaryID = Shader.PropertyToID("_Primary");
    private readonly int m_RemapID = Shader.PropertyToID("_Remap");
    private readonly int m_SecondaryID = Shader.PropertyToID("_Secondary");

    public PixelartEffectPass()
    {
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    protected override string GetPassName() => "PixelartEffectPass";

    // Here you can implement the rendering logic.
    // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
    // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
    // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
    protected override void ExecutePass(PixelartBlitMaterialComponentComponent blitMaterial, CommandBuffer commandBuffer,
        ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var sourceRT = m_CamTexRT;

        // Downsample the texture
        AppDownsamplingWhenTrue(ref sourceRT, blitMaterial, renderingData, commandBuffer, condition: blitMaterial.m_Downsample.value > 0);

        // Add Posterizing Effect
        var posterizeMaterial = blitMaterial.m_PosterizeMaterial.value;
        AddPosterizationWhenTrue(ref sourceRT, blitMaterial, commandBuffer, posterizeMaterial, condition: posterizeMaterial != null);

        // Add Dithering Effect
        var ditherMaterial = blitMaterial.m_DitherMaterial.value;
        AddDitheringWhenTrue(ref sourceRT, blitMaterial, commandBuffer, ditherMaterial, condition: ditherMaterial != null);

        if (sourceRT != m_CamTexRT)
        {
            Blitter.BlitCameraTexture(commandBuffer, sourceRT, m_CamTexRT);

            // execute the command buffer
            context.ExecuteCommandBuffer(commandBuffer);
        }
    }

    private bool AddDitheringWhenTrue(ref RTHandle sourceRT, PixelartBlitMaterialComponentComponent blitMaterial, CommandBuffer commandBuffer,
        Material ditherMaterial, bool condition)
    {
        if (condition)
        {
            // setting the shader properties
            SetMaterialMainTex(ditherMaterial);

            // pattern texture
            var patternTexture = blitMaterial.m_Pattern.value;
            ditherMaterial.SetTexture(m_PatternID, patternTexture);
            var patternTextureSize = new Vector2(blitMaterial.m_Pattern.value.width, blitMaterial.m_Pattern.value.height);
            ditherMaterial.SetVector(m_PatternTexSizeID, patternTextureSize);

            ditherMaterial.SetTexture(m_PrimaryID, blitMaterial.m_Primary.value);
            ditherMaterial.SetTexture(m_SecondaryID, blitMaterial.m_Secondary.value);
            ditherMaterial.SetVector(m_RemapID, blitMaterial.m_Remap.value);

            // Dithering
            Blitter.BlitCameraTexture(commandBuffer, sourceRT, m_TmpTexRT, RenderBufferLoadAction.DontCare,
                                      RenderBufferStoreAction.Store, ditherMaterial, 0);

            sourceRT = m_TmpTexRT;

            return true;
        }

        return false;
    }

    private bool AddPosterizationWhenTrue(ref RTHandle sourceRT, PixelartBlitMaterialComponentComponent blitMaterial, CommandBuffer commandBuffer,
        Material posterizeMaterial, bool condition)
    {
        if (condition)
        {
            // setting the shader properties
            SetMaterialMainTex(posterizeMaterial);

            // setting other shader properties
            posterizeMaterial.SetInt(m_Posterize, blitMaterial.m_Posterize.value);

            // Posterize
            Blitter.BlitCameraTexture(commandBuffer, sourceRT, m_TmpTexRT, RenderBufferLoadAction.DontCare,
                                      RenderBufferStoreAction.Store, posterizeMaterial, 0);

            sourceRT = m_TmpTexRT;

            return true;
        }

        return false;
    }

    private bool AppDownsamplingWhenTrue(ref RTHandle sourceRT, PixelartBlitMaterialComponentComponent blitMaterial,
        RenderingData renderingData, CommandBuffer commandBuffer, bool condition)
    {
        if (condition)
        {
            var camera = renderingData.cameraData.camera;

            var w = camera.scaledPixelWidth;
            var h = camera.scaledPixelHeight;

            var temporaryRts = new RTHandle[blitMaterial.m_Downsample.value];

            for (var i = 0; i < blitMaterial.m_Downsample.value; i++)
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
}
