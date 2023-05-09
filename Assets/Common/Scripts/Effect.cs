using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Common.Scripts
{
    public class Effect : VolumeComponent, IPostProcessComponent
    {
        
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
