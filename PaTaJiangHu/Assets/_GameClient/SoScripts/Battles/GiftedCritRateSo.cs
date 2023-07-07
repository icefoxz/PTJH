using UnityEngine;

namespace GameClient.SoScripts.Battles
{
    [CreateAssetMenu(fileName = "会心触发率上限加成", menuName = "弟子/天赋/会心触发率上限加成")]
    public class GiftedCritRateSo : GiftedCfgSoBase
    {
        [SerializeField] private float 会心触发率上限加成;
        private float CritRateMax => 会心触发率上限加成;
        public override float GetValue() => CritRateMax;
    }
}