using UnityEngine;

namespace Server.Configs.Battles
{
    [CreateAssetMenu(fileName = "重击伤害比率上限加成", menuName = "弟子/天赋/重击伤害比率上限加成")]
    public class GiftedHardDamageRatioSo : GiftedCfgSoBase
    {
        [SerializeField] private float 重击伤害倍率上限加成;
        private float HardDamageRatioMax => 重击伤害倍率上限加成;
        public override float GetValue() => HardDamageRatioMax / 100f;
    }
}