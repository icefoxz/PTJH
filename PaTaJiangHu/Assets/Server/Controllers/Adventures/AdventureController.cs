using System.Collections.Generic;
using BattleM;
using Newtonsoft.Json.Linq;

namespace Server.Controllers.Adventures
{
    public class AdventureController 
    {
        public Adventure InstanceAdventure(int id, AdvUnit[] units)
        {
            return new Adventure(id, units);
        }

        public void NextEvent(Adventure adv)
        {
            adv.Progress++;
            if (adv.Progress > 5) return;
            var id = adv.Progress % 2;
            var op = id == 0 ? new[] { 0 } : new[] { 0, 1 };
            var e = new AdvEvent
            {
                Id = id,
                Options = op
            };
            adv.Events.Add(e);
        }
    }

    public class AdvEvent 
    {
        public int Id { get; set; }//<--需要定义可以指定战斗(百位数为战斗,千位为固定战斗关卡)
        public int[] Options { get; set; }
    }

    public class Adventure
    {
        public int Id { get; set; }
        public AdvUnit[] Units { get; set; }
        public int Progress { get; set; }
        public List<AdvEvent> Events { get; set; } = new List<AdvEvent>();

        public Adventure(int id, AdvUnit[] units)
        {
            Id = id;
            Units = units;
        }
        public Adventure() { }
    }
    public class UnitStatus 
    {
        public ConValue Hp { get; set; }
        public ConValue Mp { get; set; }
        public ConValue Tp { get; set; }

        public UnitStatus()
        {
            
        }
        public UnitStatus(int hp, int tp,int mp)
        {
            Hp = new ConValue(hp);
            Mp = new ConValue(mp);
            Tp = new ConValue(tp);
        }

        public ICombatStatus GetCombatStatus() => CombatStatus.Instance(
            Hp.Value, Hp.Max, Hp.Fix,
            Tp.Value, Tp.Max, Tp.Fix,
            Mp.Value, Mp.Max, Mp.Fix);

        public void Clone(ICombatStatus c)
        {
            Hp.Clone(c.Hp);
            Mp.Clone(c.Mp);
            Tp.Clone(c.Tp);
        }
    }

    public class AdvUnit
    {
        public string Name { get; set; }
        public UnitStatus Status { get; set; }
        public int Strength { get; set; }
        public int Agility { get; set; }
        public Equip Equip { get; set; }
        public IForce Force { get; set; }
        public IMartial Martial { get; set; }
        public IDodge Dodge { get; set; }

        public ICombatUnit GetCombatUnit() => CombatUnit.Instance(Name, Strength, Agility,
            Status.GetCombatStatus(), Force, Martial, Dodge, Equip);
    }

    public class Equip : IEquip
    {
        private Weapon _weapon;
        private Weapon _fling;
        private Armor _armor;

        public IWeapon Weapon => _weapon;
        public IWeapon Fling => _fling;
        public IArmor Armor => _armor;
    }
}
