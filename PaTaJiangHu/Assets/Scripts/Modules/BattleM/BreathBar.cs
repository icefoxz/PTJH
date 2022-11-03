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
        /// 恢复息
        /// </summary>
        int RecoverBreath { get; }
        /// <summary>
        /// 战斗息
        /// </summary>
        int CombatBreath { get; }
        /// <summary>
        /// 蓄与硬抵消
        /// </summary>
        int BusyCharged { get; }
        /// <summary>
        /// 总硬直
        /// </summary>
        int TotalBusies { get; }
        /// <summary>
        /// 总蓄力
        /// </summary>
        int TotalCharged { get; }

        ICombatForm Combat { get; }
        IDodge Dodge { get; }
        IForce Force { get; }
        IRound Round { get; }
        IList<int> Busies { get; }
        CombatPlans Plan { get; }
        int LastRound { get; }
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

        public CombatPlans Plan { get; private set; }
        public ICombatForm Combat { get; private set; }
        public IDodge Dodge { get; private set; }
        public IForce Force { get; private set; }
        public IRound Round { get; }
        public IList<int> Busies => _busies;
        
        public int TotalBreath
        {
            get
            {
                return Plan switch
                {
                    CombatPlans.Attack => CombatBreath + BusyCharged,
                    CombatPlans.RecoverTp => RecoverBreath + BusyCharged,
                    CombatPlans.RecoverHp => RecoverBreath + BusyCharged,
                    CombatPlans.Wait => IdleBreath,
                    CombatPlans.Surrender => IdleBreath,
                    CombatPlans.Exert => throw new ArgumentOutOfRangeException(),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
        public int IdleBreath => BusyCharged;
        public int RecoverBreath => Force?.Breath ?? 0;
        public int CombatBreath
        {
            get
            {
                var combatBreath = Combat?.Breath ?? 0;
                var dodgeBreath = Dodge?.Breath ?? 0;
                return IsReposition ? combatBreath + dodgeBreath : combatBreath;
            }
        }

        public int BusyCharged => TotalBusies - TotalCharged;
        /// <summary>
        /// 总蓄力
        /// </summary>
        public int TotalCharged { get; private set; }
        public int TotalBusies => Busies.Sum(b => b);
        public int LastRound { get; private set; }
        public bool IsReposition { get; private set; }

        /// <summary>
        /// 上次蓄力
        /// </summary>
        public int LastCharged { get; private set; }

        public BreathBar(IRound round)
        {
            Round = round;
            _busies = new List<int>();
        }

        public void AddBusy(int busy)
        {
            if (busy == 0) _busies.Add(busy);
        }

        public void Charge(int charge)
        {
            LastCharged = charge;
            TotalCharged += charge;
        }
        public void SetPlan(CombatManager.Judgment judge,CombatUnitSummary strategy)
        {
            Plan = strategy.GetPlan(judge);
            Combat = strategy.CombatForm;
            Dodge = strategy.Dodge;
            Force = strategy.Force;
            IsReposition = strategy.IsReposition;
        }

        public void ClearPlan()
        {
            Plan = CombatPlans.Wait;
            Combat = null;
            Dodge = null;
            Force = null;
        }

        /// <summary>
        /// 消费气息条
        /// </summary>
        /// <param name="breathes"></param>
        public void BreathConsume(int breathes)
        {
            LastRound = Round.RoundIndex;
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
            switch (Plan)
            {
                case CombatPlans.Attack:
                    //执行攻击之类
                    consume = CombatBreath;
                    break;
                case CombatPlans.RecoverHp:
                case CombatPlans.RecoverTp:
                    consume = RecoverBreath;
                    break;
                case CombatPlans.Wait:
                case CombatPlans.Surrender:
                    break;
                case CombatPlans.Exert:
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Charge(-consume);
        }

        public int CompareTo(IBreathBar other) => TotalBreath.CompareTo(other.TotalBreath);

        public override string ToString() =>
            $"气息条({TotalBreath})【硬：{TotalBusies}|蓄{TotalCharged}|{Combat?.Name}({Combat?.Breath})|{Dodge?.Name}({Dodge?.Breath})|{Force?.Name}({Force?.Breath})】";
    }
}