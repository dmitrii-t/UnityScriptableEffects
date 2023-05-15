using Common.Scripts;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace EffectOutlines.Effect
{
    public class OutlineEffectPass : EffectPass<OutlineEffectComponent>
    {
        public OutlineEffectPass()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        protected override string GetPassName() => "OutlineEffectPass";

        protected override void ExecutePass(OutlineEffectComponent effect, CommandBuffer commandBuffer,
            ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var sourceRT = m_CamTexRT;

            var outlinesMaterial = effect.m_OutlinesMaterial.value;
            AddOutlinesEffect(ref sourceRT, effect, outlinesMaterial, renderingData, commandBuffer,
                                     condition: outlinesMaterial != null);

            // render back to teh camera RT
            if (sourceRT != m_CamTexRT)
            {
                Blitter.BlitCameraTexture(commandBuffer, sourceRT, m_CamTexRT);

                // execute the command buffer
                context.ExecuteCommandBuffer(commandBuffer);
            }
        }

        private bool AddOutlinesEffect(ref RTHandle sourceRT, OutlineEffectComponent effect,
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
}
