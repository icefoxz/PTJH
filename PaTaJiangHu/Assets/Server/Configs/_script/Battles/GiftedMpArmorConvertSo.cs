using UnityEngine;

namespace Server.Configs.Battles
{
    [CreateAssetMenu(fileName = "内力抵消转化率上限加成", menuName = "弟子/天赋/内力抵消转化率上限加成")]
    public class GiftedMpArmorConvertSo : GiftedCfgSoBase
    {
        [SerializeField] private float 内力抵消转化率加成;
        private float MpArmorConvertMax => 内力抵消转化率加成;
        public override float GetValue() => MpArmorConvertMax;
    }
}