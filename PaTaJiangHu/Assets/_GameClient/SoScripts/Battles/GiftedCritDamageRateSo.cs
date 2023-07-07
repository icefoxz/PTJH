using UnityEngine;

namespace GameClient.SoScripts.Battles
{
    [CreateAssetMenu(fileName = "会心伤害比率上限加成", menuName = "弟子/天赋/会心伤害比率上限加成")]
    public class GiftedCritDamageRateSo : GiftedCfgSoBase
    {
        [SerializeField] private float 会心伤害比率上限加成;
        private float CritDamageRatioMax => 会心伤害比率上限加成;
        public override float GetValue() => CritDamageRatioMax /100f;
    }
}