using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable] [VolumeComponentMenuForRenderPipeline("Custom/Downsampling", typeof(UniversalRenderPipeline))]
public class DownsamplingComponent : VolumeComponent, IPostProcessComponent
{
    [Header("Downsampling")]
    public IntParameter m_Downsample = new IntParameter(0);
    
    public bool IsActive()
    {
        return active;
    }

    public bool IsTileCompatible()
    {
        return true;
    }
}
