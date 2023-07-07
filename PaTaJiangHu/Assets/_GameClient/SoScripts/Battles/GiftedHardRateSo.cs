using UnityEngine;

namespace GameClient.SoScripts.Battles
{
    [CreateAssetMenu(fileName = "重击触发率上限加成", menuName = "弟子/天赋/重击触发率上限加成")]
    public class GiftedHardRateSo : GiftedCfgSoBase
    {
        [SerializeField] private float 重击触发率上限加成;
        private float HardRateMax => 重击触发率上限加成;
        public override float GetValue() => HardRateMax;
    }
}