namespace _GameClient.Models
{
    /// <summary>
    /// 弟子状态
    /// </summary>
    public interface IDiziState
    {
        string Title { get; }
    }

    public class DiziIdleState : IDiziState
    {
        public string Title { get; } = "闲置";
    }
    
    public class DiziAdventureState : IDiziState
    {
        public string Title { get; } = "历练";
    }
}