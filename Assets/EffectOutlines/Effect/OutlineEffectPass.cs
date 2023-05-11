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
            
            var differenceOfGaussiansMaterial = effect.m_DifferenceOfGaussiansMaterial.value;
            AddDifferenceOfGaussians(ref sourceRT, effect, differenceOfGaussiansMaterial, renderingData, commandBuffer, 
                                     condition: differenceOfGaussiansMaterial != null);

            // render back to teh camera RT
            if (sourceRT != m_CamTexRT)
            {
                Blitter.BlitCameraTexture(commandBuffer, sourceRT, m_CamTexRT);

                // execute the command buffer
                context.ExecuteCommandBuffer(commandBuffer);
            }
        }

        private bool AddDifferenceOfGaussians(ref RTHandle sourceRT, OutlineEffectComponent effect,
            Material differenceOfGaussiansMaterial, RenderingData renderingData, CommandBuffer commandBuffer, bool condition)
        {
            if (condition)
            {
                SetMaterialMainTex(differenceOfGaussiansMaterial);

                // Blit the camera texture to the temporary RT
                Blitter.BlitCameraTexture(commandBuffer, sourceRT, m_TmpTexRT, RenderBufferLoadAction.DontCare,
                                          RenderBufferStoreAction.Store, differenceOfGaussiansMaterial, 0);

                sourceRT = m_TmpTexRT;

                return true;
            }

            return false;
        }
    }
}
