using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RendererPixelart.Effect
{
    [System.Serializable, VolumeComponentMenuForRenderPipeline("Custom/PixelartEffectComponent", typeof(UniversalRenderPipeline))]
    public class PixelartEffectComponent : VolumeComponent, IPostProcessComponent
    {
        public MaterialParameter m_Material = new MaterialParameter(value: null);
        
        public ClampedIntParameter m_PixelRate = new ClampedIntParameter(value: 80, min: 1, max: 300);
        
        public bool IsActive()
        {
            return true;
        }
        
        public bool IsTileCompatible()
        {
            return true;
        }
    }
}
