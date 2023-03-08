namespace Server.Configs.BattleSimulation
{
    public interface ISimCombat
    {
        string Name { get; }
        int Power { get; }
        int Offend { get; }
        int Defend { get; }
        int Strength { get; }
        int Agility { get; }
        int Hp { get; }
        int Mp { get; }
        int Weapon { get; }
        int Armor { get; }
    }
}