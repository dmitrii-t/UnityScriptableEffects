using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Effect : VolumeComponent, IPostProcessComponent
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
