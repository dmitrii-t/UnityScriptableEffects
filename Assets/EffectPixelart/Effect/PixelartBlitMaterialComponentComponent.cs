using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("Custom/PixelartEffectComponent", typeof(UniversalRenderPipeline))]
public class PixelartBlitMaterialComponentComponent : BlitMaterialComponent
{
    [Header("Posterizing")]
    public MaterialParameter m_PosterizeMaterial = new MaterialParameter(value: null);

    public IntParameter m_Posterize = new IntParameter(0);

    [Header("Downsampling")]
    public IntParameter m_Downsample = new IntParameter(0);

    [Header("Dithering")]
    public MaterialParameter m_DitherMaterial = new MaterialParameter(value: null);

    public Texture2DParameter m_Pattern = new Texture2DParameter(null);

    public Texture3DParameter m_Primary = new Texture3DParameter(null);

    public Texture3DParameter m_Secondary = new Texture3DParameter(null);

    public Vector2Parameter m_Remap = new Vector2Parameter(Vector2.up);
}
