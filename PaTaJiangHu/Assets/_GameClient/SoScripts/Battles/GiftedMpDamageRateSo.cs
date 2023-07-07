using UnityEngine;

namespace GameClient.SoScripts.Battles
{
    [CreateAssetMenu(fileName = "内力伤害转化率上限加成", menuName = "弟子/天赋/内力伤害转化率上限加成")]
    public class GiftedMpDamageRateSo : GiftedCfgSoBase
    {
        [SerializeField] private float 内力伤害转化加成;
        private float MpDamageRateMax => 内力伤害转化加成;
        public override float GetValue() => MpDamageRateMax;
    }
}