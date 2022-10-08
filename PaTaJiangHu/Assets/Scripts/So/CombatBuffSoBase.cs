using System;
using BattleM;
using UnityEngine;

namespace So
{
    public abstract class CombatBuffSoBase : ScriptableObject
    {
        public enum Consumptions
        {
            [InspectorName("回合")]Round,
            [InspectorName("次数")]Consume
        }
        [SerializeField] private int _id;
        [SerializeField] private int 叠加上限;
        [SerializeField] private Consumptions 消耗方式;
        [SerializeField] private int 消耗值;
        public int SpriteId => _id;
        public int Stacks => 叠加上限;
        public ICombatBuff.Consumptions Consumption => 消耗方式 switch
        {
            Consumptions.Round => ICombatBuff.Consumptions.Round,
            Consumptions.Consume => ICombatBuff.Consumptions.Consume,
            _ => throw new ArgumentOutOfRangeException()
        };
        public int Lasting => 消耗值;
        public virtual Func<CombatUnit, float> AddStrength { get; }
        public virtual Func<CombatUnit, float> AddAgility { get; }
        public virtual Func<CombatUnit, float> AddParry { get; }
        public virtual Func<CombatUnit, float> AddDodge { get; }
        public virtual Func<CombatUnit, float> AddWeaponDamage { get; }
        public virtual Func<CombatUnit, float> ExtraMpValue { get; }
        public virtual Action<ICombatRound> RoundEnd { get; }
        public virtual Action<ICombatRound> RoundStart { get; }

        public virtual IBuffInstance InstanceBuff(CombatUnit unit) =>
            new CombatBuff(combatId: unit.CombatId, 
                lasting: Lasting, spriteId: SpriteId, 
                stacks: Stacks, consumption: Consumption,
                addStrength: AddStrength, addAgility: AddAgility, 
                addParry: AddParry, addDodge: AddDodge, 
                addWeaponDamage: AddWeaponDamage, extraMpValue: ExtraMpValue, 
                onRoundStart: RoundStart, onRoundEnd: RoundEnd);

        protected record CombatBuff : CombatBuffBase, IBuffInstance
        {

            public override int SpriteId { get; }
            public override int Stacks { get; }
            public override ICombatBuff.Consumptions Consumption { get; }
            public override Func<CombatUnit, float> AddStrength { get; }
            public override Func<CombatUnit, float> AddAgility { get; }
            public override Func<CombatUnit, float> AddParry { get; }
            public override Func<CombatUnit, float> AddDodge { get; }
            public override Func<CombatUnit, float> AddWeaponDamage { get; }
            public override Func<CombatUnit, float> ExtraMpValue { get; }
            private event Action<ICombatRound> OnRoundStart;
            private event Action<ICombatRound> OnRoundEnd;

            public CombatBuff(int combatId, int lasting, int spriteId, int stacks, 
                ICombatBuff.Consumptions consumption, 
                Func<CombatUnit, float> addStrength, 
                Func<CombatUnit, float> addAgility, 
                Func<CombatUnit, float> addParry, 
                Func<CombatUnit, float> addDodge, 
                Func<CombatUnit, float> addWeaponDamage, 
                Func<CombatUnit, float> extraMpValue,
                Action<ICombatRound> onRoundStart,
                Action<ICombatRound> onRoundEnd) : base(combatId, lasting)
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
                OnRoundStart = onRoundStart;
                OnRoundEnd = onRoundEnd;
            }

            public override void RoundEnd(ICombatRound round) => OnRoundStart?.Invoke(round);
            public override void RoundStart(ICombatRound round) => OnRoundEnd?.Invoke(round);
        }
    }
}