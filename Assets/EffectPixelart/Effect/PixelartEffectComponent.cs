using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace EffectPixelart.Effect
{
    [System.Serializable, VolumeComponentMenuForRenderPipeline("Custom/PixelartEffectComponent", typeof(UniversalRenderPipeline))]
    public class PixelartEffectComponent : Common.Scripts.Effect
    {
        public Texture2DParameter m_Pattern = new Texture2DParameter(null);
        
        public Texture3DParameter m_Primary = new Texture3DParameter(null);
        
        public Texture3DParameter m_Secondary = new Texture3DParameter(null);

        public Vector2Parameter m_Remap = new Vector2Parameter(Vector2.up);
        
    }
}
