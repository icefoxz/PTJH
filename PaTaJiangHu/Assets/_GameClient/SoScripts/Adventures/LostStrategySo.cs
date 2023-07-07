using GameClient.SoScripts.Items;
using UnityEngine;

namespace GameClient.SoScripts.Adventures
{
    [CreateAssetMenu(fileName = "LostCfg", menuName = "状态玩法/失踪配置")]
    internal class LostStrategySo : AutoUnderscoreNamingObject, IIClause
    {
        [SerializeField][Range(0,100)] private int 战斗失败失踪百分比;
        [SerializeField][Range(0,100)] private int 状态触发后失踪的百分比;
        [SerializeField] private StatusClauseField 状态条件;

        private int LostOnBattleFailed => 战斗失败失踪百分比;
        private int LostOnConditionTriggered => 状态触发后失踪的百分比;

        private StatusClauseField Clause => 状态条件;
        /// <summary>
        /// 是否触发失踪, 一般传入随机值, 这里将判断是否值低于触发百分比
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsTriggerBattleLost(int value) => LostOnBattleFailed > value;
        /// <summary>
        /// 是否触发失踪,传入随机值,判断是否值低于触发百分比
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsTriggerConditionLost(int value) => LostOnConditionTriggered > value;
        //是否状态可触发失踪判断
        public bool IsInTerm(ITerm term) => Clause.IsInTerm(term);
    }
}