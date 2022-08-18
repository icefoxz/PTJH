using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace BattleM
{
    public interface IRound
    {
        public int Current { get; }
    }
    /// <summary>
    /// 战斗单位管理器
    /// </summary>
    public class CombatManager
    {
        public enum Judgment
        {
            [InspectorName("决斗")] Duel,
            [InspectorName("切磋")] Test,
        }
        private static readonly Random Random = new Random();
        /// <summary>
        /// 战斗对手映像，key = 单位， Value = 目标
        /// </summary>
        private Dictionary<ICombatInfo, CombatUnit> UnitMap { get; } =
            new Dictionary<ICombatInfo, CombatUnit>();

        public IReadOnlyList<CombatUnit> AllUnits { get; }
        public bool IsFightEnd { get; private set; }
        public Judgment Judge { get; }
        public int WinningStance => AliveStances.Count() == 1 ? AliveStances.Single() : -1;
        public IEnumerable<int> AliveStances =>
            UnitMap.Where(u => !u.Value.IsExhausted).Select(u => u.Key.StandingPoint).Distinct();
        //战斗单位的辨识Id
        private int CombatId { get; set; }
        public CombatManager(IEnumerable<CombatUnit> combats,Judgment judgment)
        {
            AllUnits = combats.ToList();
            Judge = judgment;
            foreach (var unit in AllUnits)
            {
                unit.SetCombatId(CombatId++);
                if (Judge == Judgment.Duel)
                {
                    unit.SetStrategy(CombatUnit.Strategies.Hazard);
                }
            }

        }

        public IEnumerable<CombatUnit> GetAliveCombatUnits() => UnitMap.Values.Where(c => !c.IsExhausted);
        public CombatUnit GetAliveCombatUnit(ICombatInfo unit) => UnitMap.TryGetValue(unit, out var tg) ? tg : null;
        public CombatUnit GetFromAllUnits(int combatId) => AllUnits.Single(u => u.CombatId == combatId);
        public void RemoveUnit(ICombatInfo unit)
        {
            UnitMap.Remove(unit);
            UpdateFightEnd();
        }

        public void CheckExhausted()
        {
            foreach (var unit in UnitMap.Values.ToArray())
            {
                if (!unit.IsExhausted) continue;
                unit.ExhaustedAction();
            }
            UpdateFightEnd();
        }

        private void UpdateFightEnd()
        {
            if (UnitMap.Where(c => !c.Value.IsExhausted)
                    .GroupBy(u => u.Key.StandingPoint).Count() <= 1)
                IsFightEnd = true;
        }

        public void AddUnit(CombatUnit combat) => UnitMap.Add(combat, combat);

        public void SetTargetFor(CombatUnit op)
        {
            if (UnitMap.TryGetValue(op.Target, out _))
                return;
            var target = UnitMap.Values.OrderBy(_ => Random.Next(10))
                .FirstOrDefault(u => u.StandingPoint != op.StandingPoint && !u.IsExhausted);
            if (target == null)
            {
                IsFightEnd = true;
                return;
            }
            op.ChangeTarget(target);
        }
    }
    /// <summary>
    /// 单个战斗回合处理器
    /// </summary>
    public class CombatRound : IRound
    {
        private static Random Random { get; } = new(DateTime.Now.Millisecond);
        public int Current { get; set; }
        
        public int MinEscapeRounds { get; private set; }
        private event Action<(ICombatUnit unit,IParryForm parryForm, ParryFormula parryFormula)> OnParryAction;
        private event Action<(ICombatUnit unit,IDodgeForm dodge, DodgeFormula dodgeFormula)> OnDodgeAction;
        private event Action<(ICombatUnit op, IConsumeRecord tar, ICombatForm combat, DamageFormula damageFormula, bool isFling)> OnAttackAction;
        public CombatManager Mgr { get; }
        public CombatRound(CombatManager manager,int minEscapeRounds,bool preInit)
        {
            MinEscapeRounds = minEscapeRounds;
            var pos = 0;
            Mgr = manager;
            AddRecords();
            foreach (var combat in manager.AllUnits)
            {
                pos++;
                if (!preInit)
                {
                    var target = manager.AllUnits.First(c => c.StandingPoint != combat.StandingPoint);
                    combat.Init(manager, this, target, combat.StandingPoint, combat.Strategy);
                }

                combat.SetPosition(pos);
                manager.AddUnit(combat);
            };
            void AddRecords()
            {
                OnAttackAction += AttackActionRecord;
                OnParryAction += ParryActionRecord;
                OnDodgeAction += DodgeActionRecord;
            }
        }

        public void AdjustCombatDistance(CombatUnit obj ,ICombatInfo target, bool isEscape)
        {
            int AdjustDistance(ICombatInfo combat,ICombatInfo tar,bool closer)
            {
                var dif = combat.Position - tar.Position;
                var pos = dif > 0 ? -1 : 1;
                if (!closer) pos *= -1;
                return combat.Position + pos;
            }

            var newPos = 0;
            if (isEscape) newPos = AdjustDistance(obj, target, false);
            switch (obj.Equipment.Armed)
            {
                case Way.Armed.Unarmed when !obj.IsCombatRange():
                case Way.Armed.Short when !obj.IsCombatRange():
                    newPos = obj.Distance(target) < 1 ? AdjustDistance(obj,target, false) : AdjustDistance(obj,target, true);
                    break;
                case Way.Armed.Sword when !obj.IsCombatRange():
                case Way.Armed.Blade when !obj.IsCombatRange():
                    newPos = obj.Distance(target) < 2 ? AdjustDistance(obj, target, false) : AdjustDistance(obj, target, true);
                    break;
                case Way.Armed.Stick when !obj.IsCombatRange():
                case Way.Armed.Whip when !obj.IsCombatRange():
                    newPos = obj.Distance(target) < 3 ? AdjustDistance(obj, target, false) : AdjustDistance(obj, target, true);
                    break;
                case Way.Armed.Fling:
                {
                    if (obj.IsCombatRange() && obj.Distance(target) < 3) 
                        newPos = AdjustDistance(obj, target, false);
                    break;
                }
                //default:
                //    throw new ArgumentOutOfRangeException(
                //        $"{nameof(AdjustCombatDistance)}:{obj.Name}.{obj.Equipment.Armed},距离[{obj.Distance(target)}]，逻辑超出预期范围！");
            }

            CurrentRoundRecord.Add(new PositionRecord(newPos, obj, Mgr.GetAliveCombatUnit(target)));
            obj.SetPosition(newPos);
        }

        /// <summary>
        /// true = isAvoidEscape, false = isDodge
        /// </summary>
        /// <param name="op"></param>
        /// <param name="target"></param>
        /// <param name="combat"></param>
        /// <param name="dodge"></param>
        /// <returns></returns>
        public bool FlingOnTargetEscape(CombatUnit op, ICombatInfo target, ICombatForm combat,
            IDodgeForm dodge)
        {
            var escapee = Mgr.GetAliveCombatUnit(target);
            var dodgeFormula = InstanceDodgeFormula(op, escapee, dodge);
            var damageFormula = InstanceDamageFormula(op, combat);
            var isAvoidEscape = false;
            var consume = ConsumeRecord.Instance();
            consume.Set(escapee, () =>
            {
                if (dodgeFormula.IsSuccess)
                {
                    escapee.DodgeFromAttack(dodge);
                    OnDodgeAction?.Invoke((escapee, dodge, dodgeFormula));
                    isAvoidEscape = false;
                }
                else
                {

                    var damage = damageFormula.Finalize;
                    var armor = GetArmor(escapee);
                    var finalDamage = damage - armor;
                    var sufferDmg = finalDamage = finalDamage < 1 ? 1 : finalDamage;
                    var parryForm = escapee.PickParry();
                    var parryFormula = InstanceParryFormula(op, escapee, parryForm);

                    OnParryAction?.Invoke((escapee, parryForm, parryFormula));

                    if (parryFormula.IsSuccess)
                    {
                        sufferDmg = (int)(finalDamage * 0.2f); //防守修正
                        escapee.SufferDamage(sufferDmg, op.WeaponInjuryType);
                        escapee.SetBusy(combat.TarBusy);
                        op.SetBusy(combat.OffBusy);
                        op.SetBusy(1); //招架导致硬直
                        isAvoidEscape = true;
                    }

                    escapee.SufferDamage(sufferDmg, op.WeaponInjuryType);
                }
            });
            OnAttackAction?.Invoke((op, consume, combat, damageFormula, true));
            return isAvoidEscape;
        }


        public void OnAttack(CombatUnit offender, ICombatForm combat, ICombatInfo target)
        {
            var tg = Mgr.GetAliveCombatUnit(target);
            var tgDodge = tg.PickDodge();
            var dodgeFormula = InstanceDodgeFormula(offender, tg, tgDodge);
            var damageFormula = InstanceDamageFormula(offender, combat);
            var consume = ConsumeRecord.Instance();
            consume.Set(tg, () =>
            {
                OnDodgeAction?.Invoke((tg, tgDodge, dodgeFormula));
                if (dodgeFormula.IsSuccess)
                {
                    tg.DodgeFromAttack(tgDodge);
                    AdjustCombatDistance(tg, offender, tg.IsEscapeCondition);

                    return;
                }

                var damage = damageFormula.Finalize;
                var armor = GetArmor(tg);
                var finalDamage = damage - armor;
                var sufferDmg = finalDamage = finalDamage < 1 ? 1 : finalDamage;
                var parryForm = tg.PickParry();
                var parryFormula = InstanceParryFormula(offender, tg, parryForm);

                OnParryAction?.Invoke((tg, parryForm, parryFormula));

                if (parryFormula.IsSuccess)
                {
                    sufferDmg = (int)(finalDamage * 0.2f); //防守修正
                    offender.SetBusy(parryForm.OffBusy); //招架打入硬直
                }

                tg.SufferDamage(sufferDmg, offender.WeaponInjuryType); //伤害
                tg.SetBusy(combat.TarBusy); //攻击打入硬直
                offender.SetBusy(combat.OffBusy); //攻击方招式硬直
            });
            OnAttackAction?.Invoke((offender, consume, combat, damageFormula, false));
        }

        private int GetArmor(CombatUnit tg)
        {
            var formula = ArmorFormula.Instance(tg.Armor, tg.Status.Mp.Squeeze(tg.ForceSkill.MpArmor),
                tg.ForceSkill.MpRate);
            return formula.Finalize;
        }

        /// <summary>
        /// 每个回合的战斗执行
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public FightRoundRecord NextRound()
        {
            CurrentRoundRecord = new FightRoundRecord(Current);
            var list = Mgr.GetAliveCombatUnits().ToList();
            var fighters = list.ToList();
            fighters.ForEach(SubscribeRecords);

            fighters.ForEach(f => f.CombatPlan());
            fighters.Sort();
            var combat = fighters.First();
            Mgr.SetTargetFor(combat);
            fighters.Remove(combat);
            var breathes = combat.BreathBar.TotalBreath;
            combat.Action(breathes);

            Mgr.CheckExhausted();
            foreach (var unit in fighters) unit.BreathCharge(breathes);

            list.ToList().ForEach(UnsubscribeRecords);
            Current++;
            return CurrentRoundRecord;
        }

        #region ConsumeRecord
        private void SubscribeRecords(CombatUnit unit)
        {
            unit.OnCombatConsume += CombatConsume;
            unit.OnDodgeConsume += DodgeConsume;
            unit.OnRecoverConsume += RecoveryConsume;
            unit.OnConsume += OnConsume;
            unit.OnFightEvent += OnFightEvent;
        }

        private void DodgeActionRecord((ICombatUnit unit, IDodgeForm dodge, DodgeFormula dodgeFormula) obj)
        {
            (ICombatUnit unit, IDodgeForm dodge, DodgeFormula dodgeFormula) = obj;
            CurrentRoundRecord.Add(new DodgeRecord(unit, dodge, dodgeFormula));
        }
        private void ParryActionRecord((ICombatUnit unit, IParryForm parryForm, ParryFormula parryFormula) obj)
        {
            (ICombatUnit unit, IParryForm parryForm, ParryFormula parryFormula) = obj;
            CurrentRoundRecord.Add(new ParryRecord(unit, parryForm, parryFormula));
        }

        private void AttackActionRecord((ICombatUnit op,IConsumeRecord tar,ICombatForm combat, DamageFormula damageFormula,bool isFling) obj)
        {
            (ICombatUnit op,IConsumeRecord tar ,ICombatForm combat, DamageFormula damageFormula,bool isFling) = obj;
            CurrentRoundRecord.Add(new AttackRecord(op, tar, combat, damageFormula, isFling));
        }

        private void UnsubscribeRecords(CombatUnit unit)
        {
            unit.OnCombatConsume -= CombatConsume;
            unit.OnDodgeConsume -= DodgeConsume;
            unit.OnRecoverConsume -= RecoveryConsume;
            unit.OnConsume -= OnConsume;
            unit.OnFightEvent -= OnFightEvent;
        }

        void OnFightEvent(EventRecord @event) => CurrentRoundRecord.Add(@event);
        void OnConsume(ConsumeRecord consume) => CurrentRoundRecord.Add(consume);
        void CombatConsume(ConsumeRecord<ICombatForm> consume) => CurrentRoundRecord.Add(consume);
        void DodgeConsume(ConsumeRecord<IDodgeForm> consume) => CurrentRoundRecord.Add(consume);
        void RecoveryConsume(ConsumeRecord<IForceForm> consume) => CurrentRoundRecord.Add(consume);
        #endregion

        private FightRoundRecord CurrentRoundRecord { get; set; }

        //伤害公式
        private static DamageFormula InstanceDamageFormula(CombatUnit op, ICombatForm combat) =>
            DamageFormula.Instance(op.Strength, op.WeaponDamage, op.Status.Mp.Squeeze(combat.Mp),
                op.ForceSkill.MpRate);

        //招架公式
        private static ParryFormula InstanceParryFormula(CombatUnit op, CombatUnit tg, IParryForm form) =>
            ParryFormula.Instance(form.Parry, tg.Agility, tg.Strength, op.Distance(tg), tg.IsBusy, Randomize());

        //闪避公式
        private static DodgeFormula InstanceDodgeFormula(CombatUnit op, CombatUnit tg, IDodgeForm form) =>
            DodgeFormula.Instance(form.Dodge, tg.Agility, op.Distance(tg), tg.IsBusy, Randomize());

        private static int Randomize() => Random.Next(1, 101);
    }
}