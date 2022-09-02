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
        private Dictionary<ICombatInfo, CombatUnit> AliveMap { get; } =
            new Dictionary<ICombatInfo, CombatUnit>();

        public IReadOnlyList<CombatUnit> AllUnits { get; }
        public bool IsFightEnd { get; private set; }
        public Judgment Judge { get; }
        public int WinningStance => AliveStances.Count() == 1 ? AliveStances.Single() : -1;
        public IEnumerable<int> AliveStances =>
            AliveMap.Where(u => !u.Value.IsExhausted).Select(u => u.Key.StandingPoint).Distinct();
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

        public IEnumerable<CombatUnit> GetAliveCombatUnits() => AliveMap.Values;
        public CombatUnit GetAliveCombatUnit(ICombatInfo unit) => AliveMap.TryGetValue(unit, out var tg) ? tg : null;
        public CombatUnit GetFromAllUnits(int combatId) => AllUnits.Single(u => u.CombatId == combatId);
        public CombatUnit GetTargetedUnit(CombatUnit escapee) =>
            AliveMap.Values
                .Where(c => c.Target == escapee && c.Equipment.Fling?.FlingTimes > 0)
                .OrderBy(c => c.Distance(escapee)).FirstOrDefault();
        public void RemoveAlive(ICombatInfo unit)
        {
            AliveMap.Remove(unit);
            UpdateFightEnd();
        }

        public void CheckExhausted()
        {
            foreach (var unit in AliveMap.Values.ToArray())
            {
                if (!unit.IsExhausted) continue;
                unit.ExhaustedAction();
                AliveMap.Remove(unit);
            }
            UpdateFightEnd();
        }

        private void UpdateFightEnd()
        {
            if (AliveMap.Where(c => !c.Value.IsExhausted)
                    .GroupBy(u => u.Key.StandingPoint).Count() <= 1)
                IsFightEnd = true;
        }

        public void AddUnit(CombatUnit combat) => AliveMap.Add(combat, combat);

        public void SetTargetFor(CombatUnit op)
        {
            if (AliveMap.TryGetValue(op.Target, out _))
                return;
            var target = AliveMap.Values.OrderBy(_ => Random.Next(10))
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
    /// 单个战斗回合处理器,处理战斗的事件逻辑
    /// </summary>
    public class CombatRound : IRound
    {
        private bool _isPlan;
        private static Random Random { get; } = new(DateTime.Now.Millisecond);
        public int Current { get; set; }
        
        public int MinEscapeRounds { get; private set; }
        private CombatManager Mgr { get; }
        public CombatRound(CombatManager manager,int minEscapeRounds,bool preInit)
        {
            MinEscapeRounds = minEscapeRounds;
            var pos = 0;
            Mgr = manager;
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
                case Way.Armed.Unarmed when !obj.IsTargetRange():
                case Way.Armed.Short when !obj.IsTargetRange():
                    newPos = obj.Distance(target) < 1 ? AdjustDistance(obj,target, false) : AdjustDistance(obj,target, true);
                    break;
                case Way.Armed.Sword when !obj.IsTargetRange():
                case Way.Armed.Blade when !obj.IsTargetRange():
                    newPos = obj.Distance(target) < 2 ? AdjustDistance(obj, target, false) : AdjustDistance(obj, target, true);
                    break;
                case Way.Armed.Stick when !obj.IsTargetRange():
                case Way.Armed.Whip when !obj.IsTargetRange():
                    newPos = obj.Distance(target) < 3 ? AdjustDistance(obj, target, false) : AdjustDistance(obj, target, true);
                    break;
                case Way.Armed.Fling:
                {
                    if (obj.IsTargetRange() && obj.Distance(target) < 3) 
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
        private bool FlingOnTargetEscape(CombatUnit op, ICombatInfo target, ICombatForm combat,
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
                    RecDodgeAction(escapee, dodge, dodgeFormula);
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

                    RecParryAction(escapee, parryForm, parryFormula);

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
            RecAttackAction(op, consume, combat, damageFormula, true);
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
                RecDodgeAction(tg, tgDodge, dodgeFormula);
                if (dodgeFormula.IsSuccess)
                {
                    tg.DodgeFromAttack(tgDodge);
                    AdjustCombatDistance(tg, offender, tg.IsSurrenderCondition);

                    return;
                }

                var damage = damageFormula.Finalize;
                var armor = GetArmor(tg);
                var finalDamage = damage - armor;
                var sufferDmg = finalDamage = finalDamage < 1 ? 1 : finalDamage;
                var parryForm = tg.PickParry();
                var parryFormula = InstanceParryFormula(offender, tg, parryForm);

                RecParryAction(tg, parryForm, parryFormula);

                if (parryFormula.IsSuccess)
                {
                    sufferDmg = (int)(finalDamage * 0.2f); //防守修正
                    offender.SetBusy(parryForm.OffBusy); //招架打入硬直
                }

                tg.SufferDamage(sufferDmg, offender.WeaponInjuryType); //伤害
                tg.SetBusy(combat.TarBusy); //攻击打入硬直
                offender.SetBusy(combat.OffBusy); //攻击方招式硬直
            });
            RecAttackAction(offender, consume, combat, damageFormula, false);
        }

        private int GetArmor(CombatUnit tg)
        {
            var formula = ArmorFormula.Instance(tg.Armor, tg.Status.Mp.Squeeze(tg.ForceSkill.MpArmor),
                tg.ForceSkill.MpRate);
            return formula.Finalize;
        }

        public IList<CombatUnit> CombatPlan(IEnumerable<int> skipPlanIds)
        {
            _isPlan = true;
            var fighters = Mgr.GetAliveCombatUnits().ToList();
            foreach (var unit in fighters.Where(c => !skipPlanIds.Contains(c.CombatId))) unit.AutoCombatPlan();
            fighters.Sort();
            return fighters;
        }

        /// <summary>
        /// 每个回合的战斗执行
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public FightRoundRecord NextRound(bool autoPlan)
        {
            if (!autoPlan && !_isPlan)
                throw new InvalidOperationException($"{nameof(NextRound)}:手动回合必须先执行一次{nameof(CombatPlan)}才可以实现回合。");
            CurrentRoundRecord = new FightRoundRecord(Current);
            var allUnits = autoPlan ? CombatPlan(Array.Empty<int>()) : Mgr.GetAliveCombatUnits().ToList();
            return ProcessRound(allUnits);
        }

        private FightRoundRecord ProcessRound(IList<CombatUnit> allUnits)
        {
            foreach (var unit in allUnits) SubscribeRecords(unit);
            var fighters = allUnits.ToList();
            var actionUnits = fighters.Where(c => c.Plan != CombatPlans.Wait).ToList();
            actionUnits.Sort();
            var combat = actionUnits.FirstOrDefault();
            var breathes = 10;
            if(combat != null)
            {
                fighters.Remove(combat);
                breathes = combat.BreathBar.TotalBreath;
                combat.Action(breathes);
                Mgr.CheckExhausted();
            }
            foreach (var unit in fighters) unit.BreathCharge(breathes);
            allUnits.ToList().ForEach(UnsubscribeRecords);
            Current++;
            _isPlan = false;
            return CurrentRoundRecord;
        }

        #region ConsumeRecord
        private void SubscribeRecords(CombatUnit unit)
        {
            unit.OnCombatConsume += RecCombatConsume;
            unit.OnDodgeConsume += RecDodgeConsume;
            unit.OnRecoverConsume += RecRecoveryConsume;
            unit.OnConsume += RecConsume;
            unit.OnFightEvent += RecFightEvent;
            unit.OnSwitchTargetEvent += RecSwitchTargetEvent;
        }

        private void RecDodgeAction(ICombatUnit unit, IDodgeForm dodge, DodgeFormula dodgeFormula) =>
            CurrentRoundRecord.Add(new DodgeRecord(unit, dodge, dodgeFormula));
        private void RecParryAction(ICombatUnit unit, IParryForm parryForm, ParryFormula parryFormula) =>
            CurrentRoundRecord.Add(new ParryRecord(unit, parryForm, parryFormula));
        private void RecAttackAction(ICombatUnit op, IConsumeRecord tar, ICombatForm combat,
            DamageFormula damageFormula, bool isFling) =>
            CurrentRoundRecord.Add(new AttackRecord(op, tar, combat, damageFormula, isFling));

        private void UnsubscribeRecords(CombatUnit unit)
        {
            unit.OnCombatConsume -= RecCombatConsume;
            unit.OnDodgeConsume -= RecDodgeConsume;
            unit.OnRecoverConsume -= RecRecoveryConsume;
            unit.OnConsume -= RecConsume;
            unit.OnFightEvent -= RecFightEvent;
            unit.OnSwitchTargetEvent -= RecSwitchTargetEvent;
        }

        void RecSwitchTargetEvent(SwitchTargetRecord obj) => CurrentRoundRecord.Add(obj);
        void RecFightEvent(EventRecord @event) => CurrentRoundRecord.Add(@event);
        void RecConsume(ConsumeRecord consume) => CurrentRoundRecord.Add(consume);
        void RecCombatConsume(ConsumeRecord<ICombatForm> consume) => CurrentRoundRecord.Add(consume);
        void RecDodgeConsume(ConsumeRecord<IDodgeForm> consume) => CurrentRoundRecord.Add(consume);
        void RecRecoveryConsume(ConsumeRecord<IForceForm> consume) => CurrentRoundRecord.Add(consume);
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

        /// <summary>
        /// true = success escape
        /// </summary>
        /// <param name="escapee"></param>
        /// <param name="dodge"></param>
        /// <returns></returns>
        public bool OnTryEscape(CombatUnit escapee, IDodgeForm dodge)
        {
            escapee.SetBusy(1);//尝试逃走+1硬直
            RecFightEvent(EventRecord.Instance(escapee, FightFragment.Types.TryEscape));
            var tar = Mgr.GetTargetedUnit(escapee);
            if (tar != null)
            {
                if (tar.Distance(escapee) <= 4)
                {
                    var combat = tar.PickCombat();
                    tar.Equipment.FlingConsume();
                    var isSuccessAttack = FlingOnTargetEscape(escapee, tar, combat, dodge);
                    return !isSuccessAttack;
                }
            }
            return true;
        }

    }
}