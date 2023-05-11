using UnityEngine;
using UnityEngine.Rendering;

namespace EffectOutlines.Effect
{
    public partial class OutlineEffectComponent : Common.Scripts.Effect
    {
        [Header("Difference of Gaussians")]
        public MaterialParameter m_DifferenceOfGaussiansMaterial = new MaterialParameter(value: null);
        
    }
}
