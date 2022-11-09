using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleM
{
    /// <summary>
    /// 战斗精灵管理器，处理战斗精灵生成销毁方法
    /// </summary>
    public interface ICombatBuffManager
    {
    }

    public class CombatBuffManager : ICombatBuffManager
    {
        private int IdSeed { get; set; }
        private List<IBuffInstance> Buffs { get; } = new List<IBuffInstance>();

        #region CUD

        private int GetSeed() => ++IdSeed;

        public void AddBuff(IBuffInstance sp)
        {
            if (sp.Stacks == 0 || 
                Buffs.Count(s => s.CombatId == sp.CombatId && s.SpriteId == sp.SpriteId) < sp.Stacks) //同类型的buff不超过最大叠加
            {
                sp.SetSeed(GetSeed());
                Buffs.Add(sp);
            }
        }

        public void RemoveBuff(IBuffInstance sp) => Buffs.Remove(sp);

        public void OnRoundStart(ICombatRound round)
        {
            foreach (var sprite in Buffs) sprite.RoundStart(round);
        }

        public void OnRoundEnd(ICombatRound round)
        {
            foreach (var buff in Buffs)
                buff.RoundEnd(round);
            foreach (var buff in Buffs.Where(b =>
                         b.Consumption == ICombatBuff.Consumptions.Round).ToArray())
                DepleteOnConsumption(buff, ICombatBuff.Consumptions.Round);
        }
        private void DepleteOnConsumption(IBuffInstance buff, ICombatBuff.Consumptions con)
        {
            if (buff.Consumption == con)
            {
                buff.LastingDepletion();
            }
            if (buff.Lasting <= 0)
            {
                RemoveBuff(buff);
            }
        }

        public void RemoveAll(int combatId)
        {
            foreach (var sprite in Buffs.Where(s => s.CombatId == combatId)
                         .ToArray())
                Buffs.Remove(sprite);
        }

        #endregion

        public IEnumerable<IBuffInstance> GetBuffs(int combatId) =>
            Buffs.Where(s => s.CombatId == combatId);

        private int ConsumeBuff(CombatUnit unit, ICombatBuff.Kinds kind, Func<IBuffInstance, float> funcMethod,bool depletion)
        {
            var buffs = GetBuffs(unit.CombatId).Where(b => IsBuffOf(kind, b)).ToArray();
            var additional = buffs.Sum(funcMethod.Invoke);
            if(depletion)//消耗buff
                foreach (var buff in buffs)
                    DepleteOnConsumption(buff, ICombatBuff.Consumptions.Consume);
            return (int)additional;

            static bool IsBuffOf(ICombatBuff.Kinds k, ICombatBuff cb)
            {
                return k switch
                {
                    ICombatBuff.Kinds.Strength => cb.AddStrength != null,
                    ICombatBuff.Kinds.Agility => cb.AddAgility != null,
                    ICombatBuff.Kinds.Parry => cb.AddParry != null,
                    ICombatBuff.Kinds.Dodge => cb.AddDodge != null,
                    ICombatBuff.Kinds.Weapon => cb.AddWeaponDamage != null,
                    ICombatBuff.Kinds.ExtraMp => cb.ExtraMpValue != null,
                    _ => throw new ArgumentOutOfRangeException(nameof(k), k, null)
                };
            }
        }


        public int GetStrength(CombatUnit unit, int unitStrength, bool depletion)
        {
            var additional = ConsumeBuff(unit, ICombatBuff.Kinds.Strength, b => b.AddStrength?.Invoke(unit) ?? 0,
                depletion);
            return unitStrength + additional;
        }

        public int GetAgility(CombatUnit unit, int unitAgility, bool depletion)
        {
            var additional = ConsumeBuff(unit, ICombatBuff.Kinds.Agility, b => b.AddAgility?.Invoke(unit) ?? 0,
                depletion);
            return unitAgility + additional;
        }

        public int GetWeaponDamage(CombatUnit unit, int unitWeaponDamage, bool depletion)
        {
            var additional = ConsumeBuff(unit, ICombatBuff.Kinds.Weapon, b => b.AddWeaponDamage?.Invoke(unit) ?? 0, depletion);
            return unitWeaponDamage + additional;
        }

        public int GetParry(CombatUnit unit, int unitParry, bool depletion)
        {
            var additional = ConsumeBuff(unit, ICombatBuff.Kinds.Parry, b => b.AddParry?.Invoke(unit) ?? 0, depletion);
            return unitParry + additional;
        }

        public int GetExtraMpValue(CombatUnit unit, int formMp, bool depletion)
        {
            var additional = ConsumeBuff(unit, ICombatBuff.Kinds.Parry, b => b.ExtraMpValue?.Invoke(unit) ?? 0, depletion);
            return formMp + additional;
        }

        public int GetDodge(CombatUnit unit, int unitDodge, bool depletion)
        {
            var additional = ConsumeBuff(unit, ICombatBuff.Kinds.Dodge, b => b.AddDodge?.Invoke(unit) ?? 0, depletion);
            return unitDodge + additional;
        }
    }
}