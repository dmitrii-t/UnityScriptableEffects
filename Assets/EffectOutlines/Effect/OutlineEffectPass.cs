using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineEffectPass : EffectPass<OutlineBlitMaterialComponentComponent>
{
    public OutlineEffectPass()
    {
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    protected override string GetPassName() => "OutlineEffectPass";

    protected override void ExecutePass(OutlineBlitMaterialComponentComponent blitMaterial, CommandBuffer commandBuffer,
        ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var sourceRT = m_CamTexRT;

        // Adding effect passes
        var outlinesMaterial = blitMaterial.m_OutlineMaterial.value;
        AddOutlinesEffect(ref sourceRT, blitMaterial, outlinesMaterial, renderingData, commandBuffer,
                          condition: outlinesMaterial != null);

        // render back to teh camera RT
        if (sourceRT != m_CamTexRT)
        {
            Blitter.BlitCameraTexture(commandBuffer, sourceRT, m_CamTexRT);

            // execute the command buffer
            context.ExecuteCommandBuffer(commandBuffer);
        }
    }

    private bool AddOutlinesEffect(ref RTHandle sourceRT, OutlineBlitMaterialComponentComponent blitMaterial,
        Material outlinesMaterial, RenderingData renderingData, CommandBuffer commandBuffer, bool condition)
    {
        if (condition)
        {
            SetMaterialMainTex(outlinesMaterial);

            // Blit the camera texture to the temporary RT
            Blitter.BlitCameraTexture(commandBuffer, sourceRT, m_TmpTexRT, RenderBufferLoadAction.DontCare,
                                      RenderBufferStoreAction.Store, outlinesMaterial, 0);

            sourceRT = m_TmpTexRT;

            return true;
        }

        return false;
    }
}
