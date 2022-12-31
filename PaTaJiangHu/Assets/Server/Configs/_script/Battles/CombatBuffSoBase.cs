using System;
using System.Collections.Generic;
using System.Linq;
using BattleM;
using UnityEngine;

namespace Server.Configs._script.Battles
{
    public abstract class CombatBuffSoBase : ScriptableObject,ICombatBuff
    {
        public enum Consumptions
        {
            [InspectorName("回合")]Round,
            [InspectorName("次数")]Consume
        }
        [SerializeField] private int _id;
        [SerializeField] private int 叠加上限;
        [SerializeField] private Consumptions 消耗方式;
        [SerializeField] private int 持久值;
        [SerializeField] private ICombatBuff.Appends 赋buff;
        public int SpriteId => _id;
        public int Stacks => 叠加上限;
        public ICombatBuff.Consumptions Consumption => 消耗方式 switch
        {
            Consumptions.Round => ICombatBuff.Consumptions.Round,
            Consumptions.Consume => ICombatBuff.Consumptions.Consume,
            _ => throw new ArgumentOutOfRangeException()
        };
        public int Lasting => 持久值;
        public virtual Func<ICombatUnit, float> AddStrength { get; }
        public virtual Func<ICombatUnit, float> AddAgility { get; }
        public virtual Func<ICombatUnit, float> AddParry { get; }
        public virtual Func<ICombatUnit, float> AddDodge { get; }
        public virtual Func<ICombatUnit, float> AddWeaponDamage { get; }
        public virtual Func<ICombatUnit, float> ExtraMpValue { get; }

        public virtual Action<ICombatRound> RoundEnd { get; }
        public virtual Action<ICombatRound> RoundStart { get; }
        public ICombatBuff.Appends Append => 赋buff;

        public virtual IBuffInstance InstanceBuff(ICombatUnit unit) =>
            new CombatBuff(combatId: unit.CombatId,
                lasting: Lasting, spriteId: SpriteId,
                stacks: Stacks, consumption: Consumption,
                append: Append,
                addStrength: AddStrength, addAgility: AddAgility,
                addParry: AddParry, addDodge: AddDodge,
                addWeaponDamage: AddWeaponDamage, extraMpValue: ExtraMpValue,
                onRoundStart: RoundStart, onRoundEnd: RoundEnd);

        protected record CombatBuff : CombatBuffBase, IBuffInstance
        {
            public override int SpriteId { get; }
            public override int Stacks { get; }
            public override ICombatBuff.Consumptions Consumption { get; }
            public override Func<ICombatUnit, float> AddStrength { get; }
            public override Func<ICombatUnit, float> AddAgility { get; }
            public override Func<ICombatUnit, float> AddParry { get; }
            public override Func<ICombatUnit, float> AddDodge { get; }
            public override Func<ICombatUnit, float> AddWeaponDamage { get; }
            public override Func<ICombatUnit, float> ExtraMpValue { get; }
            public override Action<ICombatRound> RoundEnd { get; }
            public override Action<ICombatRound> RoundStart { get; }

            public CombatBuff(int combatId, int lasting, int spriteId, int stacks,
                ICombatBuff.Consumptions consumption,
                ICombatBuff.Appends append,
                Func<ICombatUnit, float> addStrength,
                Func<ICombatUnit, float> addAgility,
                Func<ICombatUnit, float> addParry,
                Func<ICombatUnit, float> addDodge,
                Func<ICombatUnit, float> addWeaponDamage,
                Func<ICombatUnit, float> extraMpValue,
                Action<ICombatRound> onRoundStart,
                Action<ICombatRound> onRoundEnd) : base(combatId, lasting, append)
            {
                SpriteId = spriteId;
                Stacks = stacks;
                Consumption = consumption;
                AddStrength = addStrength;
                AddAgility = addAgility;
                AddParry = addParry;
                AddDodge = addDodge;
                AddWeaponDamage = addWeaponDamage;
                ExtraMpValue = extraMpValue;
                RoundStart = onRoundStart;
                RoundEnd = onRoundEnd;
            }
        }
    }

    public static class CombatBuffInstanceExtension
    {
        public static IEnumerable<IBuffInstance> GetSortedInstance(this CombatBuffSoBase[] buffs,
            ICombatBuff.Appends append, ICombatUnit unit) =>
            buffs.Where(b => b.Append == append).Select(b => b.InstanceBuff(unit));
    }
}