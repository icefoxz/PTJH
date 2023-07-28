namespace AOT.Core
{
    /// <summary>
    /// 堆叠
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IStacking<out T> 
    {
        T Item { get; }
        int Amount { get; }
    }
}