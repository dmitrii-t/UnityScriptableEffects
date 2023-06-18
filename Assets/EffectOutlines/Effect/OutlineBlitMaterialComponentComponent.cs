using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("Custom/OutlineEffectComponent", typeof(UniversalRenderPipeline))]
public partial class OutlineBlitMaterialComponentComponent : BlitMaterialComponent
{
    [Header("Outlines")]
    public MaterialParameter m_OutlineMaterial = new MaterialParameter(value: null);
}
