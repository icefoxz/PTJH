using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleM
{
    /// <summary>
    /// 气息条，用于计算战斗角色总
    /// </summary>
    public interface IBreathBar : IComparable<IBreathBar>
    {
        int TotalBreath { get; }
        ICombatForm Combat { get; }
        IDodgeForm Dodge { get; }
        IForceForm Recover { get; }
        IRound Round { get; }
        IList<int> Busies { get; }
        public int LastRound { get; }
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
        public IDodgeForm Dodge { get; private set; }
        public IForceForm Recover { get; private set; }
        public IRound Round { get; }
        public IList<int> Busies => _busies;
        public int Charged { get; private set; }
        public int TotalBreath
        {
            get
            {
                if (Recover != null)//优先执行调息
                    return Busies.Sum(b => b) + Recover.Breath - Charged;
                return Busies.Sum(b => b) + (Combat?.Breath ?? 0) + (Dodge?.Breath ?? 0) - Charged;
            }
        }
        public int LastRound { get; private set; }

        public BreathBar(IRound round)
        {
            Round = round;
            _busies = new List<int>();
        }

        public void SetPlan(CombatPlans plan) => Plan = plan;
        public void SetBusy(int busy) => _busies.Add(busy);
        public void Charge(int charge) => Charged += charge;
        public void SetCombat(ICombatForm form) => Combat = form;
        public void ClearCombat() => Combat = null;
        public void SetDodge(IDodgeForm form) => Dodge = form;
        public void ClearDodge() => Dodge = null;
        public void SetRecover(IForceForm form) => Recover = form;
        /// <summary>
        /// 消费气息条
        /// </summary>
        /// <param name="breathes"></param>
        public void BreathConsume(int breathes)
        {
            LastRound = Round.Current;
            Charge(breathes);
            if (_busies.Any()) //先去掉硬直
            {
                var busies = _busies.Count;
                for (var i = 0; i < busies; i++)
                {
                    var value = _busies[0];
                    if (value > Charged) break;
                    _busies.RemoveAt(0);
                    Charge(-value);
                }
            }

            if (Charged >= Recover?.Breath) //先执行调息
            {
                Charge(-Recover.Breath);
                //recoverCallback?.Invoke(Recover);
                Recover = null;
            }

            //执行攻击之类
            var combatBreath = Combat?.Breath ?? 0;
            var dodgeBreath = Dodge?.Breath ?? 0;
            var breath = combatBreath + dodgeBreath;
            Charge(-breath);
            //combatCallback?.Invoke(Dodge, Combat);
            Dodge = null;
            Combat = null;
        }
        public int CompareTo(IBreathBar other) => TotalBreath.CompareTo(other.TotalBreath);
        public override string ToString() => $"气息条({TotalBreath - Charged})【硬直：{Busies.Sum():D}|{Combat?.Name}({Combat?.Breath})|{Dodge?.Name}({Dodge?.Breath})|{Recover?.Name}({Recover?.Breath})】";
    }
}