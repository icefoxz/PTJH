
namespace BattleM
{
    /// <summary>
    /// 游戏状态值，作为非常基础的游戏属性，分为固定，上限，当前值
    /// </summary>
    public interface IConditionValue
    {
        int Max { get; }
        int Value { get; }
        int Fix { get; }
    }
    /// <summary>
    /// 游戏状态值基本接口，包括执行方法
    /// </summary>
    public interface IGameCondition : IConditionValue
    {
        float MaxRatio { get; }
        float ValueFixRatio { get; }
        float ValueMaxRatio { get; }
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

    /// <summary>
    /// 战斗状态属性
    /// </summary>
    public interface ICombatStatus
    {
        /// <summary>
        /// 血量,Health Point
        /// </summary>
        IGameCondition Hp { get; }
        ///// <summary>
        ///// 体能,Condition Point
        ///// </summary>
        //IGameCondition Tp { get; } 
        /// <summary>
        /// 内力,Force Point
        /// </summary>
        IGameCondition Mp { get; }
        bool IsExhausted { get; }
        void Clone(ICombatStatus status);
    }

    public static class CombatStatusExtension
    {
        public static CombatStatus Clone(this ICombatStatus status)
        {
            return CombatStatus.Instance(
                hp: status.Hp.Value,
                maxHp: status.Hp.Max,
                fixHp: status.Hp.Fix,
                //tp: status.Tp.Value,
                //maxtp: status.Tp.Max,
                //fixtp: status.Tp.Fix,
                mp: status.Mp.Value,
                maxmp: status.Mp.Max,
                fixmp: status.Mp.Fix
            );
        }
    }
    public class CombatStatus : ICombatStatus
    {
        public static CombatStatus Instance(int hp, int mp) => Instance(hp, hp, hp, mp, mp, mp);
        public static CombatStatus Instance(int hp, int maxHp, int fixHp, int mp, int maxmp,
            int fixmp) => new (new ConValue(fixHp, maxHp, hp), new ConValue(fixmp, maxmp, mp));

        public IGameCondition Hp { get; }
        public IGameCondition Mp { get; }
        public bool IsExhausted => Hp.IsExhausted;
        public void Clone(ICombatStatus status)
        {
            
        }

        private CombatStatus(IGameCondition hp, IGameCondition mp)
        {
            Hp = hp;
            Mp = mp;
        }
        public override string ToString() => $"Hp{Hp},Mp{Mp}";
    }
}