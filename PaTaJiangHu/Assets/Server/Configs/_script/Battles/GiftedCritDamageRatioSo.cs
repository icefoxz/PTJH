using UnityEngine;

namespace Server.Configs.Battles
{
    [CreateAssetMenu(fileName = "会心伤害比率上限加成", menuName = "弟子/天赋/会心伤害比率上限加成")]
    public class GiftedCritDamageRatioSo : GiftedCfgSoBase
    {
        [SerializeField] private float 会心伤害比率上限加成;
        private float CritDamageRatioMax => 会心伤害比率上限加成;
        public override float GetValue() => CritDamageRatioMax /100f;
    }
}