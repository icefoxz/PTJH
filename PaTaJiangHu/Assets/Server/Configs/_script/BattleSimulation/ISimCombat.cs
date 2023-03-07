namespace Server.Configs.BattleSimulation
{
    public interface ISimCombat
    {
        string Name { get; }
        float Offend { get; }
        float Defend { get; }
        float Strength { get; }
        float Agility { get; }
        float Weapon { get; }
        float Armor { get; }
    }
}