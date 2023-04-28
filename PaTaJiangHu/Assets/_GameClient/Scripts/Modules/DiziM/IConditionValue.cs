namespace DiziM
{
    /// <summary>
    /// 游戏状态值，作为非常基础的游戏属性，分为固定，上限，当前值
    /// </summary>
    public interface IConditionValue
    {
        int Max { get; }
        int Value { get; }
        int Fix { get; }
        /// <summary>
        /// 上限与固定值的比率
        /// </summary>
        double MaxRatio { get; }
        /// <summary>
        /// 当前与固定值的比率
        /// </summary>
        double ValueFixRatio { get; }
        /// <summary>
        /// 当前与最大值的比率
        /// </summary>
        double ValueMaxRatio { get; }
    }

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
        void SetFix(int fix);
        void AddMax(int value, bool alignValue = true);
        void Clone(IConditionValue con);
    }

}