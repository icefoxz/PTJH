namespace AOT.Core.Dizi
{
    /// <summary>
    /// 战斗状态接口
    /// </summary>
    public interface ICombatCondition : IGameCondition
    {
        string Name { get; }
    }
}