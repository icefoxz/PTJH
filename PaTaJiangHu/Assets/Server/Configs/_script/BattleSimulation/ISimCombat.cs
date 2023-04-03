namespace Server.Configs.BattleSimulation
{
    public interface ISimCombat : ICombatUnit
    {
        string Name { get; }
        int Power { get; }
        int Damage { get; }
        int MaxHp { get; }
        int Strength { get; }
        int Agility { get; }
        int Hp { get; }
        int Mp { get; }
        int Weapon { get; }
        int Armor { get; }
    }
}