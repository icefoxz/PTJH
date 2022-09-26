using System;

namespace BattleM
{
    public interface ICombatBuff
    {
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

        /// <summary>
        /// 精灵Id(唯一类型标识)
        /// </summary>
        int SpriteId { get; }

        /// <summary>
        /// 同类型最大重叠数
        /// </summary>
        int Stacks { get; }

        Consumptions Consumption { get; }

        /// <summary>
        /// 力量增加值<see cref="Kinds.Strength"/>
        /// </summary>
        Func<CombatUnit, float> AddStrength { get; }

        /// <summary>
        /// 敏捷增加值<see cref="Kinds.Agility"/>
        /// </summary>
        Func<CombatUnit, float> AddAgility { get; }

        /// <summary>
        /// 招架增加值<see cref="Kinds.Parry"/>
        /// </summary>
        Func<CombatUnit, float> AddParry { get; }

        /// <summary>
        /// 身法增加值<see cref="Kinds.Dodge"/>
        /// </summary>
        Func<CombatUnit, float> AddDodge { get; }

        /// <summary>
        /// 武器伤害增加值<see cref="Kinds.Weapon"/>
        /// </summary>
        Func<CombatUnit, float> AddWeaponDamage { get; }

        /// <summary>
        /// Mp增加值<see cref="Kinds.ExtraMp"/>
        /// </summary>
        Func<CombatUnit, float> ExtraMpValue { get; }
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

        protected CombatBuffBase(int combatId,int lasting)
        {
            CombatId = combatId;
            Lasting = lasting;
        }

        public abstract int SpriteId { get; }
        public abstract int Stacks { get; }
        public abstract ICombatBuff.Consumptions Consumption { get; }
        public int Lasting { get; protected set; }
        public abstract Func<CombatUnit, float> AddStrength { get; }
        public abstract Func<CombatUnit, float> AddAgility { get; }
        public abstract Func<CombatUnit, float> AddParry { get; }
        public abstract Func<CombatUnit, float> AddDodge { get; }
        public abstract Func<CombatUnit, float> AddWeaponDamage { get; }
        public abstract Func<CombatUnit, float> ExtraMpValue { get; }
        public abstract void RoundEnd(ICombatRound round);
        public abstract void RoundStart(ICombatRound round);
        public void LastingDepletion(int deplete = 1)
        {
            Lasting -= deplete;
        }
    }
}