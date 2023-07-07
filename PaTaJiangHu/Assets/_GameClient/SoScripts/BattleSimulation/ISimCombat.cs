namespace GameClient.SoScripts.BattleSimulation
{
    public interface ISimCombat 
    {
        string Name { get; }
        int Power { get; }
        int Damage { get; }
        int MaxHp { get; }
        float Strength { get; }
        float Agility { get; }
        float Hp { get; }
        float Mp { get; }
    }
}