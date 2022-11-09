using System;
using System.Collections.Generic;
using System.Linq;
using BattleM;

namespace BattleM
{
    /// <summary>
    /// 气息条，用于计算战斗角色总
    /// </summary>
    public interface IBreathBar : IComparable<IBreathBar>
    {
        /// <summary>
        /// 总气息
        /// </summary>
        int TotalBreath { get; }
        /// <summary>
        /// 总硬直
        /// </summary>
        int TotalBusies { get; }
        /// <summary>
        /// 总蓄力
        /// </summary>
        int TotalCharged { get; }
        /// <summary>
        /// 行动履行(演示)，自动或介入的战斗执行的统一接口
        /// </summary>
        IPerform Perform { get; }
        /// <summary>
        /// 总硬直
        /// </summary>
        IList<int> Busies { get; }
        /// <summary>
        /// 自动策略(包括认输，与等待这类玩家不能主动操作的策略)
        /// </summary>
        CombatEvents AutoPlan { get; }
        /// <summary>
        /// 是否发生移位
        /// </summary>
        bool IsReposition { get; }
    }
    /// <summary>
    /// 气息节点，所有战斗动作都有必须继承这个
    /// </summary>
    public interface IBreathNode
    {
        int Breath { get; }
    }

    public class BreathBar : IBreathBar
    {
        private readonly List<int> _busies;
        public CombatEvents AutoPlan { get; private set; }

        /// <summary>
        /// 主要行动的出口
        /// </summary>
        public IPerform Perform { get; private set; }
        public IExert Exert { get; private set; }
        public CombatUnitSummary Strategy { get; private set; }
        public CombatUnitManager.Judgment Judge { get; private set; }
        public IList<int> Busies => _busies;
        
        public int TotalBreath
        {
            get
            {
                return AutoPlan switch
                {
                    CombatEvents.Offend => CombatBreath + BusyCharged,
                    CombatEvents.Recover => ForceBreath + BusyCharged,
                    CombatEvents.Wait => IdleBreath,
                    CombatEvents.Surrender => IdleBreath,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
        public int IdleBreath => BusyCharged;
        public int ForceBreath => Perform.ForceSkill?.Breath ?? 0;
        /// <summary>
        /// 战斗息(如果移位+身法息)
        /// </summary>
        public int CombatBreath
        {
            get
            {
                var combatBreath = Perform.CombatForm?.Breath ?? 0;
                var dodgeBreath = Perform.DodgeSkill?.Breath ?? 0;
                return IsReposition ? combatBreath + dodgeBreath : combatBreath;
            }
        }
        /// <summary>
        /// 硬直与蓄力抵消
        /// </summary>
        public int BusyCharged => TotalBusies - TotalCharged;
        /// <summary>
        /// 总蓄力
        /// </summary>
        public int TotalCharged { get; private set; }
        /// <summary>
        /// 总硬直
        /// </summary>
        public int TotalBusies => Busies.Sum(b => b);
        /// <summary>
        /// 是否移位
        /// </summary>
        public bool IsReposition { get; private set; }

        public BreathBar() => _busies = new List<int>();

        public void AddBusy(int busy)
        {
            if (busy > 0) _busies.Add(busy);
        }

        public void Charge(int charge) => TotalCharged += charge;

        public void SetAutoPlan(CombatUnitManager.Judgment judge,CombatUnitSummary strategy)
        {
            Judge = judge;
            Strategy = strategy;
            IsReposition = strategy.IsReposition;
            SetAutoStrategy();
        }
        /// <summary>
        /// 玩家介入技能策略
        /// </summary>
        public void SetExert(IExert exert)
        {
            Exert = exert;
            Perform = exert.GetPerform(this);
        }

        /// <summary>
        /// 自动策略，一般用于玩家介入取消运功的重置策略
        /// </summary>
        public void SetAutoStrategy()
        {
            AutoPlan = Strategy.GetPlan(Judge);
            Perform = Strategy.GetAutoPerform();
        }

        public void ClearPlan()
        {
            AutoPlan = CombatEvents.Wait;
            Perform = null;
            Strategy = default;
        }

        /// <summary>
        /// 消费气息条
        /// </summary>
        /// <param name="breathes"></param>
        public void BreathConsume(int breathes)
        {
            Charge(breathes);
            if (_busies.Any()) //先去掉硬直
            {
                var busies = _busies.Count;
                for (var i = 0; i < busies; i++)
                {
                    var value = _busies[0];
                    if (value > TotalCharged) break;
                    _busies.RemoveAt(0);
                    Charge(-value);
                }
            }
            var consume = 0;
            switch (Perform.Activity)
            {
                case IPerform.Activities.Attack:
                    consume = CombatBreath;
                    break;
                case IPerform.Activities.Recover:
                    consume = ForceBreath;
                    break;
                case IPerform.Activities.Auto:
                    switch (AutoPlan)
                    {
                        case CombatEvents.Offend:
                            //执行攻击之类
                            consume = CombatBreath;
                            break;
                        case CombatEvents.Recover:
                            consume = ForceBreath;
                            break;
                        case CombatEvents.Wait:
                        case CombatEvents.Surrender:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Charge(-consume);
        }

        public int CompareTo(IBreathBar other) => TotalBreath.CompareTo(other.TotalBreath);

        public override string ToString() =>
            $"气息条({TotalBreath})【硬：{TotalBusies}|蓄{TotalCharged}|" +
            $"{Perform.CombatForm?.Name}({Perform.CombatForm?.Breath})|" +
            $"{Perform.DodgeSkill?.Name}({Perform.DodgeSkill?.Breath})|" +
            $"{Perform.ForceSkill?.Name}({Perform.ForceSkill?.Breath})】";
    }
}