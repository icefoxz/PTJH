using UnityEngine;

namespace GameClient.Modules.BattleM
{
    public class Combat
    {
        public enum Calculate
        {
            [InspectorName("除")] Divide,
            [InspectorName("乘")] Multiply,
        }

        public enum Compares
        {
            [InspectorName("力")] Strength,
            [InspectorName("敏")] Agility,
            [InspectorName("血")] Hp,
            [InspectorName("血上限")] HpMax,
            [InspectorName("内")] Mp,
            [InspectorName("内上限")] MpMax,
        }

        //战斗属性, DiziCombatUnit的主要属性
        public const string Hp = "Hp";
        public const string Mp = "Mp";
        public const string Str = "Str";
        public const string Agi = "Agi";

        //事件处理器,基于战斗流程中固定的事件(例:回合开始,回合结束,攻击开始)触发
        public const string Func_HpRecover = "Func_HpRecover";
        public const string Func_HpSubtract = "Func_HpSubtract";
        public const string Func_MpRecover = "Func_MpRecover";
        public const string Func_MpSubtract = "Func_MpSubtract";

        //触发器,在战斗中流程中触发
        public const string Trigger_HardAttack = "Trigger_HardAttack";
        public const string Trigger_CriticalAttack = "Trigger_CriticalAttack";
        public const string Trigger_Dodge = "Trigger_Dodge";

        public enum RoundTriggers
        {
            [InspectorName("回合开始")] RoundStart,
            [InspectorName("回合结束")] RoundEnd,
            [InspectorName("攻击开始")] CombatStart,
            //[InspectorName("攻击结束")] AttackEnd,
            //[InspectorName("受击开始")] BeAttackStart,
            //[InspectorName("受击结束")] BeAttackEnd,
        }
    }
}