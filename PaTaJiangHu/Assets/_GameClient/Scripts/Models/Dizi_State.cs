using _GameClient.Models.States;
using _GameClient.Models;
using Core;
using Server.Configs.Adventures;
using Server.Controllers;
using System.Collections.Generic;
using System;
using Utls;

namespace Models
{
    //弟子模型,处理状态
    public partial class Dizi
    {
        /// <summary>
        /// 状态信息, 提供当前状态的描述,与时长
        /// </summary>
        public DiziStateHandler State { get; }

        //下列事件将在构造函数中注册
        #region StateHandler
        private void OnRewardAction() => EventUpdate(EventString.Dizi_Activity_Reward);
        private void OnAdjustAction(string adjust) => SendEvent(EventString.Dizi_Activity_Adjust, Guid, adjust);
        private void OnMessageAction(string message) => SendEvent(EventString.Dizi_Activity_Message, Guid, message);
        #endregion
    }

    //弟子模型,处理历练事件
    public partial class Dizi
    {
        public IEnumerable<IStacking<IGameItem>> Items => State.Adventure?.GetItems() ?? Array.Empty<IStacking<IGameItem>>();
        //public AutoAdventure Adventure => State.Adventure;
        internal void AdventureStart(IAutoAdvMap map, long startTime, int messageSecs, bool isProduction)
        {
            State.StartAdventure(map, startTime, messageSecs, isProduction);
            Log("开始历练.");
            EventUpdate(EventString.Dizi_Params_StateUpdate);
            EventUpdate(EventString.Dizi_Adv_Start);
        }

        internal void AdventureStoryLogging(DiziActivityLog story)
        {
            if (State.Adventure.State == AutoAdventure.States.End)
                throw new NotImplementedException();
            State.RegAutoAdvStory(story);
        }
        internal void AdventureRecall(long now, int lastMile, long reachingTime)
        {
            State.RecallFromAdventure(now, lastMile, reachingTime);
            Log($"停止历练, 里数: {lastMile}, 将{TimeSpan.FromMilliseconds(reachingTime - now).TotalSeconds}秒后到达宗门!");
            EventUpdate(EventString.Dizi_Params_StateUpdate);
            EventUpdate(EventString.Dizi_Adv_Recall);
        }
        internal void AdventureFinalize()
        {
            State.FinalizeAdventure();
            Log("历练结束!");
            EventUpdate(EventString.Dizi_Params_StateUpdate);
            EventUpdate(EventString.Dizi_Adv_Finalize);
        }

        public void AdventureTerminate(long terminateTime, int lastMile)
        {
            Log("历练中断!");
            State.Terminate(terminateTime, lastMile);
            EventUpdate(EventString.Dizi_Adv_Terminate);
        }
    }

    //弟子模型, 处理闲置事件
    public partial class Dizi
    {
        //闲置状态
        //public IdleState Idle => State.Idle;

        internal void StartIdle(long startTime)
        {
            State.StartIdle(startTime);
            EventUpdate(EventString.Dizi_Params_StateUpdate);
            EventUpdate(EventString.Dizi_Idle_Start);
        }

        internal void StopIdle()
        {
            State.StopIdleState();
            EventUpdate(EventString.Dizi_Params_StateUpdate);
            EventUpdate(EventString.Dizi_Idle_Stop);
        }

        internal void RegIdleStory(DiziActivityLog log)
        {
            State.RegIdleStory(log);
        }
    }

    //处理失踪事件
    public partial class Dizi
    {
        //public LostState LostState => State.LostState;

        internal void StartLostState(long startTime, DiziActivityLog lastActivityLog)
        {
            State.StartLost(this, startTime, lastActivityLog);
            EventUpdate(EventString.Dizi_Params_StateUpdate);
            EventUpdate(EventString.Dizi_Lost_Start);
        }

        internal void RestoreFromLost()
        {
            State.StartIdle(SysTime.UnixNow);
            EventUpdate(EventString.Dizi_Params_StateUpdate);
            EventUpdate(EventString.Dizi_Lost_End);
        }
    }

    /// <summary>
    /// 历练道具模型
    /// </summary>
    public class AdvItemModel : ModelBase
    {
        public enum Kinds
        {
            Medicine,
            StoryProp,
            Horse
        }
        protected override string LogPrefix => "历练道具";
        public IGameItem Item { get; private set; }
        public Kinds Kind { get; private set; }

        internal AdvItemModel(IGameItem item)
        {
            Kind = item.Type switch
            {
                ItemType.Medicine => Kinds.Medicine,
                ItemType.StoryProps => Kinds.StoryProp,
                ItemType.AdvProps => Kinds.Horse,
                _ => throw new ArgumentOutOfRangeException($"物品{item.Type}不支持! ")
            };
            Item = item;
        }
    }
}