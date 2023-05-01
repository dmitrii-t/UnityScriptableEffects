using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Common.Scripts
{
    public class Effect : VolumeComponent, IPostProcessComponent
    {
        public MaterialParameter m_Material = new MaterialParameter(value: null);
        
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
