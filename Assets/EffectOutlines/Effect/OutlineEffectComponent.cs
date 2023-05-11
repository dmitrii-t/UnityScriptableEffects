using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace EffectOutlines.Effect
{
    [Serializable, VolumeComponentMenuForRenderPipeline("Custom/OutlineEffectComponent", typeof(UniversalRenderPipeline))]
    public partial class OutlineEffectComponent : Common.Scripts.Effect
    {
        
    }
}
