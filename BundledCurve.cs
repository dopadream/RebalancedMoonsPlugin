using UnityEngine;

namespace RebalancedMoons
{
    [CreateAssetMenu(menuName = "ScriptableObjects/BundledCurve", order = 2)]
    public class BundledCurve : ScriptableObject
    {
        public AnimationCurve curve;
    }
}

