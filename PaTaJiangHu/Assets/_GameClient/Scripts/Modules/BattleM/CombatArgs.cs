using System;

/// <summary>
/// 战斗参数
/// </summary>
public class CombatArgs : EventArgs
{
    public DiziCombatUnit Caster { get; }
    public DiziCombatUnit Target { get; }

    public CombatArgs(DiziCombatUnit caster, DiziCombatUnit target)
    {
        Caster = caster;
        Target = target;
    }

    public static DiziCombatUnit InstanceCombatUnit(string guid, string name, int hp, int mp, int strength, int agility,
        int teamId, ICombatSet combat) => new(unit: new CombatUnit(guid: guid, name: name, hp: hp, mp: mp,
        strength: strength, agility: agility, teamId: teamId, combat: combat));

    private class CombatUnit : IDiziCombatUnit
    {
        public int InstanceId { get; private set; }
        public string Name { get; }
        public int Hp { get; }
        public int MaxHp { get; }
        public int Damage { get; }
        public int Speed { get; }
        public int TeamId { get; }
        public string Guid { get; }
        public int Mp { get; }
        public int MaxMp { get; }
        public int Strength { get; }
        public int Agility { get; }
        public ICombatSet Combat { get; }
        public void SetInstanceId(int instanceId) => InstanceId=instanceId;

        public CombatUnit(string guid, string name, int hp, int mp, int strength, int agility, int teamId, ICombatSet combat)
        {
            Name = name;
            Hp = hp;
            MaxHp = hp;
            Damage = strength;
            Speed = agility;
            TeamId = teamId;
            Guid = guid;
            Mp = mp;
            MaxMp = mp;
            Strength = strength;
            Agility = agility;
            Combat = combat;
        }
    }
}