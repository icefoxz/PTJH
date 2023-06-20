using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Server.Configs.Fields;
using UnityEngine;

namespace Server.Configs.Adventures
{
    public interface IAdvPackage
    {
        int Grade { get; }
        IStacking<IGameItem>[] AllItems { get; }
        IAdvResPackage Package { get; }
    }

    public interface IAdvResPackage
    {
        int Food { get; }
        int Wine { get; }
        int Herb { get; }
        int Pill { get; }
        int Silver { get; }
        int YuanBao { get; }
    }
    /// <summary>
    /// 主要的游戏奖励, 可以用于宝箱接口
    /// </summary>
    public interface IGameReward
    {
        IAdvPackage[] Packages { get; }
        IStacking<IGameItem>[] AllItems { get; }
    }

    public interface IRewardHandler
    {
        void SetReward(IGameReward reward);
    }

    [CreateAssetMenu(fileName = "id_奖励件名", menuName = "状态玩法/事件/奖励事件")]
    internal class RewardEventSo : AdvEventSoBase
    {
        [SerializeField] private string 事件名 = "奖励";
        [SerializeField] private AdvEventSoBase 下个事件;
        [SerializeField] private int 弟子经验;
        [SerializeField] private RewardField 奖励;
        private RewardField GameReward => 奖励;

        private int Exp => 弟子经验;
        //[SerializeField] private int _id;
        public override string Name => 事件名;
        //public override int Id => _id;
        public override void EventInvoke(IAdvEventArg arg)
        {
            var messages = GenerateRewardMessages(arg.DiziName);
            var adjustment = arg.Adjustment.Set(IAdjustment.Types.Exp, Exp, false);
            messages.Add(adjustment);
            var eventMsgs = messages.ToArray();
            OnLogsTrigger?.Invoke(eventMsgs);
            arg.RewardHandler.SetReward(Reward);
            InvokeAdjustmentEvent(new[] { adjustment });
            InvokeRewardEvent(Reward);
            OnNextEvent?.Invoke(Next);
        }

        private List<string> GenerateRewardMessages(string diziName)
        {
            var messages = new List<string>(GameReward.AllItemFields.Select(m => GenerateLogFromItem(m, diziName)));
            if (Exp > 0) messages.Insert(0, $"{diziName}获得经验【{Exp}】");
            return messages;
        }
        private string GenerateLogFromItem(GameItemField item, string diziName) => $"{diziName}获得【{item.Name}】x{item.Amount}。";

        public override event Action<IAdvEvent> OnNextEvent;
        public override IAdvEvent[] AllEvents => new[] { Next };
        public override AdvTypes AdvType => AdvTypes.Reward;
        public override event Action<string[]> OnLogsTrigger;
        private IAdvEvent Next => 下个事件;
        public IGameReward Reward => GameReward;
    }
}