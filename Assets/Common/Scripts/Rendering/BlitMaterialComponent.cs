using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable] [VolumeComponentMenuForRenderPipeline("Custom/BlitMaterial", typeof(UniversalRenderPipeline))]
public class BlitMaterialComponent : VolumeComponent, IPostProcessComponent
{
    public MaterialParameter m_Material = new MaterialParameter(value: null);

    public IntParameter m_MaterialPassIndex = new IntParameter(value: -1); // -1: all passes
    
    public bool IsActive()
    {
        return m_Material.value != null;
    }

    public bool IsTileCompatible()
    {
        return true;
    }
}
