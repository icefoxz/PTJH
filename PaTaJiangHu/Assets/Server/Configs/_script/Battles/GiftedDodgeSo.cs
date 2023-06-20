using UnityEngine;

namespace Server.Configs.Battles
{
    [CreateAssetMenu(fileName = "闪避率上限加成", menuName = "弟子/天赋/闪避率上限加成")]
    public class GiftedDodgeSo : GiftedCfgSoBase
    {
        [SerializeField]private float 闪避率上限加成;
        private float DodgeRateMax => 闪避率上限加成;
        public override float GetValue()=> DodgeRateMax;
    }
}