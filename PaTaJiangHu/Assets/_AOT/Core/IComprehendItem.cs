using AOT.Core.Dizi;

namespace AOT.Core
{
    // 领悟道具, 用于领悟技能加成, 非书籍类
    public interface IComprehendItem : IFunctionItem
    {
        ColorGrade Grade { get; }
        float Rate { get; }
        bool IsLevelAvailable(int level);
    }
}