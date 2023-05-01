using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace EffectPixelart.Effect
{
    [System.Serializable, VolumeComponentMenuForRenderPipeline("Custom/PixelartEffectComponent", typeof(UniversalRenderPipeline))]
    public class PixelartEffectComponent : Common.Scripts.Effect
    {
        public ClampedIntParameter m_PixelRate = new ClampedIntParameter(value: 80, min: 1, max: 300);
    }
}
