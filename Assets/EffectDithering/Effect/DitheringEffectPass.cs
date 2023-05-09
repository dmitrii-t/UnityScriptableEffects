using Common.Scripts;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace EffectDithering.Effect
{
    // [System.Serializable]
    public class DitheringEffectPass : EffectPass<DitheringEffectComponent>
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

        protected override string GetPassName() => "Dithering Effect Pass";


        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var stack = VolumeManager.instance.stack;
            var effect = stack.GetComponent<DitheringEffectComponent>();

            if (effect.IsActive())
            {
                var material = effect.m_DitherMaterial.value;
                if (material != null)
                {
                    SetMainTextureProperties(material);
                
                    var patternTexture = effect.m_Pattern.value;
                    material.SetTexture(m_PatternID,patternTexture);
                    var patternTextureSize = new Vector2(effect.m_Pattern.value.width, effect.m_Pattern.value.height);
                    material.SetVector(m_PatternTexSizeID, patternTextureSize);

                    material.SetTexture(m_PrimaryID, effect.m_Primary.value);
                    material.SetTexture(m_SecondaryID, effect.m_Secondary.value);
                    material.SetVector(m_RemapID, effect.m_Remap.value);
                }
                
                ExecuteWithMaterial(context, effect.m_DitherMaterial.value, ref renderingData);
            }
        }
    }
}
