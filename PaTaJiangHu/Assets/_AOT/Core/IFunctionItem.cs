namespace AOT.Core
{
    public enum FunctionItemType
    {
        Comprehend,
        Medicine,
        AdvItem,
        StoryProps,
    }
    public interface IFunctionItem : IGameItem
    {
        FunctionItemType FunctionType { get; }
    }
}