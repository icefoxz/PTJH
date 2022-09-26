namespace BattleM
{
    public interface IBuffInstance : ICombatBuff
    {
        int InstanceId { get; }
        int CombatId { get; }
        int Lasting { get; }
        void SetSeed(int seed);
        void LastingDepletion(int deplete = 1);
    }
}