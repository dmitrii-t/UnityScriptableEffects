using Common.Scripts;
using UnityEngine;

namespace EffectPixelart.Effect
{
    // [System.Serializable]
    public class PixelartPass : EffectPass<PixelartEffectComponent>
    {
        private readonly int m_MainTexID = Shader.PropertyToID("_MainTex");

        private readonly int m_PixelRateID = Shader.PropertyToID("_PixelRate");

        public override string GetPassName() => "Pixelart Render Pass";
        
        public override void SetMaterialProperties(PixelartEffectComponent effect, Material material)
        {
            // setting the shader properties
            material.SetFloat(m_PixelRateID, effect.m_PixelRate.value);
            material.SetTexture(m_MainTexID, m_CamTexRT);
        }
    }
}
