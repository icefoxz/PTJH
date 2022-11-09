using System;
using UnityEngine;

namespace BattleM
{
    public interface ICombatBuff
    {
        /** 持久
         * 1.回合制
         * 2.消耗制(攻,受击,架,闪,移)
         */
        public enum Consumptions
        {
            Round,
            Consume
        }
        public enum Kinds
        {
            Strength,
            Agility,
            Parry,
            Dodge,
            Weapon,
            ExtraMp,
        }

        public enum Appends
        {
            /// <summary>
            /// 自身buff
            /// </summary>
            [InspectorName("自身")] Self,
            /// <summary>
            /// 对方-强制赋buff
            /// </summary>
            [InspectorName("敌强制")] TargetForce,
            /// <summary>
            /// 对方-当击中
            /// </summary>
            [InspectorName("敌击中")]TargetIfHit
        }

        /// <summary>
        /// 精灵Id(唯一类型标识)
        /// </summary>
        int SpriteId { get; }
        /// <summary>
        /// 同类型最大重叠数
        /// </summary>
        int Stacks { get; }
        Appends Append { get; }
        Consumptions Consumption { get; }
        /// <summary>
        /// 力量增加值<see cref="Kinds.Strength"/>
        /// </summary>
        Func<ICombatUnit, float> AddStrength { get; }
        /// <summary>
        /// 敏捷增加值<see cref="Kinds.Agility"/>
        /// </summary>
        Func<ICombatUnit, float> AddAgility { get; }
        /// <summary>
        /// 招架增加值<see cref="Kinds.Parry"/>
        /// </summary>
        Func<ICombatUnit, float> AddParry { get; }
        /// <summary>
        /// 身法增加值<see cref="Kinds.Dodge"/>
        /// </summary>
        Func<ICombatUnit, float> AddDodge { get; }
        /// <summary>
        /// 武器伤害增加值<see cref="Kinds.Weapon"/>
        /// </summary>
        Func<ICombatUnit, float> AddWeaponDamage { get; }
        /// <summary>
        /// Mp增加值<see cref="Kinds.ExtraMp"/>
        /// </summary>
        Func<ICombatUnit, float> ExtraMpValue { get; }
        /// <summary>
        /// 当回合结束，回合精灵消失前。
        /// </summary>
        void RoundEnd(ICombatRound round);
        /// <summary>
        /// 所有消耗类型的buff都会触发回合开始
        /// </summary>
        /// <param name="round"></param>
        void RoundStart(ICombatRound round);
    }

    public abstract record CombatBuffBase : IBuffInstance
    {
        public int InstanceId { get; private set; }
        public int CombatId { get; }

        public void SetSeed(int seed)
        {
            if (InstanceId > 0)
                throw new InvalidOperationException($"InstanceId ={InstanceId}, Duplicate SetSeed = {seed}");
            InstanceId = seed;
        }

        protected CombatBuffBase(int combatId,int lasting, ICombatBuff.Appends append)
        {
            CombatId = combatId;
            Lasting = lasting;
            Append = append;
        }

        public abstract int SpriteId { get; }
        public abstract int Stacks { get; }
        public ICombatBuff.Appends Append { get; }
        public abstract ICombatBuff.Consumptions Consumption { get; }
        public int Lasting { get; protected set; }
        public abstract Func<ICombatUnit, float> AddStrength { get; }
        public abstract Func<ICombatUnit, float> AddAgility { get; }
        public abstract Func<ICombatUnit, float> AddParry { get; }
        public abstract Func<ICombatUnit, float> AddDodge { get; }
        public abstract Func<ICombatUnit, float> AddWeaponDamage { get; }
        public abstract Func<ICombatUnit, float> ExtraMpValue { get; }
        public abstract void RoundEnd(ICombatRound round);
        public abstract void RoundStart(ICombatRound round);
        public void LastingDepletion(int deplete = 1)
        {
            Lasting -= deplete;
        }
    }
}