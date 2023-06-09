﻿using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


// [System.Serializable]
public class DitheringEffectPass : EffectPass<DitheringBlitMaterialComponentComponent>
{
    private readonly int m_PatternID = Shader.PropertyToID("_Pattern");

    private readonly int m_PatternTexSizeID = Shader.PropertyToID("_PatternTexSize");

    private readonly int m_PrimaryID = Shader.PropertyToID("_Primary");

    private readonly int m_SecondaryID = Shader.PropertyToID("_Secondary");

    private readonly int m_RemapID = Shader.PropertyToID("_Remap");

    public DitheringEffectPass()
    {
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    protected override string GetPassName() => "DitheringEffectPass";


    protected override void ExecutePass(DitheringBlitMaterialComponentComponent blitMaterial, CommandBuffer commandBuffer, ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var sourceRT = m_CamTexRT;

        var material = blitMaterial.m_DitherMaterial.value;
        if (material != null)
        {
            SetMaterialMainTex(material);

            // Set other shader properties 
            var patternTexture = blitMaterial.m_Pattern.value;
            material.SetTexture(m_PatternID, patternTexture);
            var patternTextureSize = new Vector2(blitMaterial.m_Pattern.value.width, blitMaterial.m_Pattern.value.height);
            material.SetVector(m_PatternTexSizeID, patternTextureSize);

            material.SetTexture(m_PrimaryID, blitMaterial.m_Primary.value);
            material.SetTexture(m_SecondaryID, blitMaterial.m_Secondary.value);
            material.SetVector(m_RemapID, blitMaterial.m_Remap.value);

            // Blit the camera texture to the temporary RT
            Blitter.BlitCameraTexture(commandBuffer, sourceRT, m_TmpTexRT, RenderBufferLoadAction.DontCare,
                                      RenderBufferStoreAction.Store, material, 0);

            sourceRT = m_TmpTexRT;

            Blitter.BlitCameraTexture(commandBuffer, sourceRT, m_CamTexRT);

            // execute the command buffer
            context.ExecuteCommandBuffer(commandBuffer);
        }
    }
}
