namespace AOT.Core.Dizi
{
    /// <summary>
    /// 游戏状态值基本接口，包括执行方法
    /// </summary>
    public interface IGameCondition : IConditionValue
    {
        bool IsExhausted { get; }
        void Add(int value);
        /// <summary>
        /// 榨取并扣除值，(有可能获取不足预设)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        int Squeeze(int value);
        void Set(int value);
        void SetMax(int max, bool alignValue = true);
        void SetBase(int fix);
        void AddMax(int value, bool alignValue = true);
        void Clone(IConditionValue con);
    }
}