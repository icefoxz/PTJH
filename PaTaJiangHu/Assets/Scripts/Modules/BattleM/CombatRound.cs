using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace BattleM
{
    public interface IRound
    {
        public int RoundIndex { get; }
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

        public CombatBuffManager BuffMgr { get; set; }

        //战斗单位的辨识Id
        private int CombatIdIndex { get; set; }
        public CombatManager(IEnumerable<CombatUnit> combats,Judgment judgment)
        {
            BuffMgr = new CombatBuffManager();
            AllUnits = combats.ToList();
            Judge = judgment;
            foreach (var unit in AllUnits)
            {
                unit.SetCombatId(++CombatIdIndex);
                if (Judge == Judgment.Duel)
                {
                    unit.SetStrategy(CombatUnit.Strategies.Hazard);
                }
            }
        }

        public IEnumerable<CombatUnit> GetAliveCombatUnits() => AliveMap.Values;
        public CombatUnit TryGetAliveCombatUnit(ICombatInfo unit) => AliveMap.TryGetValue(unit, out var tg) ? tg : null;
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

        public void CheckExhausted(Action<CombatUnit> onUnitExhausted)
        {
            foreach (var unit in AliveMap.Values.ToArray())
            {
                if (!unit.IsExhausted) continue;
                
                onUnitExhausted(unit);
                BuffMgr.RemoveAll(unit.CombatId);
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

    public interface ICombatRound : IRound
    {
        int MinEscapeRounds { get; }

        void OnEscape(CombatUnit escapee, IDodgeForm dodge);

        void OnAttack(CombatUnit offender, IBreathBar breathBar, ICombatInfo target);
        IList<CombatUnit> CombatPlan(IEnumerable<int> skipPlanIds);

        /// <summary>
        /// 每个回合的战斗执行
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        CombatRoundRecord NextRound(bool autoPlan);
        void RecRecharge(RechargeRecord rec);
        void RecForceRecovery(RecoveryRecord rec);
        void OnTargetSwitch(CombatUnit combatUnit, ICombatInfo target);
    }

    /// <summary>
    /// 单个战斗回合处理器,处理战斗的事件逻辑
    /// </summary>
    public class CombatRound : ICombatRound
    {
        private bool _isPlan;
        private static Random Random { get; } = new(DateTime.Now.Millisecond);
        public int RoundIndex { get; set; }
        
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

        private PositionRecord AdjustCombatDistance(CombatUnit obj ,ICombatInfo target, bool isEscape)
        {
            int AdjustDistance(ICombatInfo combat,ICombatInfo tar,bool closer)
            {
                var dif = combat.Position - tar.Position;
                var pos = dif > 0 ? -1 : 1;
                if (!closer) pos *= -1;
                return combat.Position + pos;
            }

            var newPos = 0;
            if (isEscape)
            {
                newPos = AdjustDistance(obj, target, false);
            }
            else
            {
                //如果不在允许攻击范围内，赋予新位置(根据最近位置原则)
                //否则返回原来的位置
                switch (obj.Equipment.Armed)
                {
                    case Way.Armed.Unarmed :
                    case Way.Armed.Short:
                        newPos = !obj.IsTargetRange()
                            ? obj.Distance(target) < 1
                                ? AdjustDistance(obj, target, false)
                                : AdjustDistance(obj, target, true)
                            : obj.Position;
                        break;
                    case Way.Armed.Sword:
                    case Way.Armed.Blade:
                        newPos = !obj.IsTargetRange()
                            ? obj.Distance(target) < 2
                                ? AdjustDistance(obj, target, false)
                                : AdjustDistance(obj, target, true)
                            : obj.Position;
                        break;
                    case Way.Armed.Stick:
                    case Way.Armed.Whip:
                        newPos = !obj.IsTargetRange() 
                            ? obj.Distance(target) < 3 
                                ? AdjustDistance(obj, target, false) 
                                : AdjustDistance(obj, target, true) 
                            : obj.Position;
                        break;
                    //case Way.Armed.Fling:
                    //{
                    //    if (obj.IsTargetRange() && obj.Distance(target) < 3) 
                    //        newPos = AdjustDistance(obj, target, false);
                    //    break;
                    //}
                    //default:
                    //    throw new ArgumentOutOfRangeException(
                    //        $"{nameof(AdjustCombatDistance)}:{obj.Name}.{obj.Equipment.Armed},距离[{obj.Distance(target)}]，逻辑超出预期范围！");
                }
            }

            obj.SetPosition(newPos);
            return new PositionRecord(newPos, obj, Mgr.TryGetAliveCombatUnit(target));
        }

        public void OnEscape(CombatUnit escapee, IDodgeForm dodge)
        {
            escapee.SetBusy(1); //尝试逃走+1硬直
            var op = Mgr.GetTargetedUnit(escapee);
            if (op != null)
            {
                if (op.Distance(escapee) > 4)
                {
                    var combat = op.PickCombat();
                    op.Equipment.FlingConsume();
                    {
                        var dodgeFormula = InstanceDodgeFormula(op, escapee, dodge);
                        var damageFormula = InstanceDamageFormula(op, combat);
                        var armor = GetArmor(escapee);
                        var parryForm = escapee.PickParry();
                        var parryFormula = InstanceParryFormula(op, escapee, parryForm);

                        var attConsume = ConsumeRecord<ICombatForm>.Instance(combat);
                        attConsume.Set(op, () => op.ConsumeForm(combat));

                        var tarParryConsume = ConsumeRecord<IParryForm>.Instance(parryForm);
                        var tarDodgeConsume = ConsumeRecord<IDodgeForm>.Instance(dodge);
                        tarDodgeConsume.Set(escapee, () => escapee.ConsumeForm(dodge));
                        if (parryFormula.IsSuccess)
                            tarParryConsume.Set(escapee, () => escapee.ConsumeForm(parryForm));
                        var tarSuffer = ConsumeRecord.Instance();
                        tarSuffer.Set(escapee, () =>
                        {
                            if (dodgeFormula.IsSuccess) //成功逃走
                                return;
                            var damage = (int)(0.01f * damageFormula.Finalize);
                            var finalDamage = damage - (int)(1f - armor * 0.01f); //最终伤害(扣除免伤)
                            var sufferDmg = finalDamage = finalDamage < 1 ? 1 : finalDamage; //最低伤害为1
                            if (parryFormula.IsSuccess)
                            {
                                sufferDmg = ParryFormula.Damage(finalDamage); //防守修正
                                op.SetBusy(parryForm.OffBusy); //招架打入硬直
                            }

                            escapee.SufferDamage(sufferDmg, op.WeaponInjuryType); //伤害
                            escapee.SetBusy(combat.TarBusy); //攻击打入硬直
                            op.SetBusy(combat.OffBusy); //攻击方招式硬直
                        });

                        RoundRec.SetEscape(new EscapeRecord(escapee, op, attConsume, tarDodgeConsume, tarParryConsume,
                            tarSuffer, damageFormula, dodgeFormula, parryFormula, dodgeFormula.IsSuccess));

                        if (!dodgeFormula.IsSuccess) return;
                    }
                }
            }

            RoundRec.SetEscape(new EscapeRecord(escapee, true));
            Mgr.RemoveAlive(escapee);
        }

        public void OnAttack(CombatUnit offender, IBreathBar breathBar, ICombatInfo target)
        {
            var attackForm = breathBar.Combat;
            PositionRecord placingRecord = null;
            if (breathBar.Dodge != null) placingRecord = AdjustCombatDistance(offender, offender.Target, false);
            var tar = Mgr.TryGetAliveCombatUnit(target);
            var dodgeForm = tar.PickDodge();
            var dodgeFormula = InstanceDodgeFormula(offender, tar, dodgeForm);
            var damageFormula = InstanceDamageFormula(offender, attackForm);
            var combo = attackForm.Combo?.Rates ?? new[] { 100 };
            for (var i = 0; i < combo.Length; i++)
            {
                var damageRate = combo[i];
                var armor = GetArmor(tar);
                var parryForm = tar.PickParry();
                var parryFormula = InstanceParryFormula(offender, tar, parryForm);
                RecAttackAction(offender, tar, attackForm, parryForm, dodgeForm, placingRecord, damageFormula,
                    dodgeFormula, parryFormula, armor, damageRate);
            }
        }

        private void OnUnitExhausted(CombatUnit tar)
        {
            tar.DeathAction();
            RoundRec.SetSubEvent(SubEventRecord.Instance(tar, SubEventRecord.EventTypes.Death));
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
        public CombatRoundRecord NextRound(bool autoPlan)
        {
            if (!autoPlan && !_isPlan)
                throw new InvalidOperationException($"{nameof(NextRound)}:手动回合必须先执行一次{nameof(CombatPlan)}才可以实现回合。");
            RoundRec = new CombatRoundRecord(++RoundIndex);
            var allUnits = autoPlan ? CombatPlan(Array.Empty<int>()) : Mgr.GetAliveCombatUnits().ToList();
            return RoundProcess(allUnits);
        }

        public void OnTargetSwitch(CombatUnit combatUnit, ICombatInfo target)
        {
            var tar = Mgr.TryGetAliveCombatUnit(target);
            if (tar == null)
                throw new NullReferenceException("Target is null!");
            RoundRec.SetSwitchTarget(SwitchTargetRecord.Instance(combatUnit, tar));
        }

        /*****************************Round Process**********************************/
        private CombatRoundRecord RoundProcess(IList<CombatUnit> allUnits)
        {
            Mgr.BuffMgr.OnRoundStart(this);
            foreach (var unit in allUnits) 
                RoundRec.AddBreathBar(unit);//breath记录
            var fighters = allUnits.ToList();
            var actionUnits = fighters.Where(c => c.Plan != CombatPlans.Wait).ToList();//剔除等待单位
            actionUnits.Sort();
            var combat = actionUnits.FirstOrDefault();
            var breathes = 10;//默认息-恢复值(如果双方都等待，各自恢复10点)
            if(combat != null)
            {
                RoundRec.SetExecutor(combat);
                fighters.Remove(combat);
                breathes = combat.BreathBar.TotalBreath;//覆盖默认息
                combat.Action(breathes);
                Mgr.CheckExhausted(OnUnitExhausted);
            }

            foreach (var unit in fighters.Where(c => !c.IsExhausted)) //去掉死亡单位(因为列表可能包涵被攻击死亡的单位)
            {
                if (unit.BreathBar.BusyCharged > 0)
                    unit.BreathCharge(unit.BreathBar.BusyCharged);
            }//息恢复
            Mgr.BuffMgr.OnRoundEnd(this);

            if(Mgr.IsFightEnd) RoundRec.SetFightEnd();
            _isPlan = false;
            return RoundRec;
        }

        #region ConsumeRecord

        private void RecAttackAction(CombatUnit op, CombatUnit tar,
            ICombatForm attackForm, IParryForm parryForm, IDodgeForm dodgeForm, PositionRecord attackPlacing,
            DamageFormula damageFormula, DodgeFormula dodgeFormula, ParryFormula parryFormula,
            int armor, int damageRate)
        {
            var attConsume = ConsumeRecord<ICombatForm>.Instance(attackForm);
            attConsume.Set(op, () => op.ConsumeForm(attackForm));

            var tarParryConsume = ConsumeRecord<IParryForm>.Instance(parryForm);
            var tarDodgeConsume = ConsumeRecord<IDodgeForm>.Instance(dodgeForm);

            var tarSuffer = ConsumeRecord.Instance();
            if (dodgeFormula.IsSuccess)
            {
                //闪避优先更新状态一次(1)
                tarDodgeConsume.Set(tar, () => tar.ConsumeForm(dodgeForm));
            }
            else
            {
                if (parryFormula.IsSuccess)
                {
                    //招架第二次更新状态(2)
                    tarParryConsume.Set(tar, () => tar.ConsumeForm(parryForm));
                }

                //承受伤害第三次更新状态(3)
                tarSuffer.Set(tar, () =>
                {
                    var damage = (int)(0.01f * damageFormula.Finalize * damageRate);
                    var finalDamage = damage - (int)(1f - armor * 0.01f); //最终伤害(扣除免伤)
                    var sufferDmg = finalDamage = finalDamage < 1 ? 1 : finalDamage; //最低伤害为1
                    if (parryFormula.IsSuccess)
                    {
                        sufferDmg = ParryFormula.Damage(finalDamage); //防守修正
                        op.SetBusy(parryForm.OffBusy); //招架打入硬直
                    }

                    tar.SufferDamage(sufferDmg, op.WeaponInjuryType); //伤害
                    tar.SetBusy(attackForm.TarBusy); //攻击打入硬直
                    op.SetBusy(attackForm.OffBusy); //攻击方招式硬直
                });
            }

            RoundRec.SetAttack(
                new AttackRecord(op, tar, attackPlacing, 
                    attConsume, tarDodgeConsume, tarParryConsume, tarSuffer,
                    damageFormula, dodgeFormula, parryFormula));
        }

        public void RecRecharge(RechargeRecord rec) => RoundRec.AddRechargeRec(rec);
        public void RecForceRecovery(RecoveryRecord rec) => RoundRec.SetForceRecovery(rec);
        #endregion

        private CombatRoundRecord RoundRec { get; set; }

        //伤害公式
        private DamageFormula InstanceDamageFormula(CombatUnit op, ICombatForm combat)
        {
            var opStrength = Mgr.BuffMgr.GetStrength(op, op.Strength);
            var opWeaponDamage = Mgr.BuffMgr.GetWeaponDamage(op, op.WeaponDamage);
            var mp = op.Status.Mp.Squeeze(combat.Mp);
            var mpValue = Mgr.BuffMgr.GetExtraMpValue(op, mp);
            return DamageFormula.Instance(opStrength, opWeaponDamage, mpValue, op.ForceSkill.MpRate);
        }

        //招架公式
        private ParryFormula InstanceParryFormula(CombatUnit op, CombatUnit tg, IParryForm form)
        {
            var tarStrength = Mgr.BuffMgr.GetStrength(tg, tg.Strength);
            var tarAgility = Mgr.BuffMgr.GetAgility(tg, tg.Agility);
            var tarParry = Mgr.BuffMgr.GetParry(tg, form.Parry);
            return ParryFormula.Instance(tarParry, tarAgility, tarStrength, op.Distance(tg), tg.IsBusy, Randomize());
        }

        //闪避公式
        private DodgeFormula InstanceDodgeFormula(CombatUnit op, CombatUnit tg, IDodgeForm form)
        {
            var tarDodge = Mgr.BuffMgr.GetDodge(tg, form.Dodge);
            var tarAgility = Mgr.BuffMgr.GetAgility(tg, tg.Agility);
            return DodgeFormula.Instance(tarDodge, tarAgility, op.Distance(tg), tg.IsBusy, Randomize());
        }

        private static int Randomize() => Random.Next(1, 101);
    }
}