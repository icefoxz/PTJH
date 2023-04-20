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
        private void OnRewardAction() => SendEvent(EventString.Dizi_Activity_Reward, Guid);
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
            SendEvent(EventString.Dizi_Params_StateUpdate, Guid);
            SendEvent(EventString.Dizi_Adv_Start, Guid);
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
            SendEvent(EventString.Dizi_Params_StateUpdate, Guid);
            SendEvent(EventString.Dizi_Adv_Recall, Guid);
        }
        internal void AdventureFinalize()
        {
            State.FinalizeAdventure();
            Log("历练结束!");
            SendEvent(EventString.Dizi_Params_StateUpdate, Guid);
            SendEvent(EventString.Dizi_Adv_Finalize, Guid);
        }

        public void AdventureTerminate(long terminateTime, int lastMile)
        {
            Log("历练中断!");
            State.Terminate(terminateTime, lastMile);
            SendEvent(EventString.Dizi_Adv_Terminate, Guid);
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
            SendEvent(EventString.Dizi_Params_StateUpdate, Guid);
            SendEvent(EventString.Dizi_Idle_Start, Guid);
        }

        internal void StopIdle()
        {
            State.StopIdleState();
            SendEvent(EventString.Dizi_Params_StateUpdate, Guid);
            SendEvent(EventString.Dizi_Idle_Stop, Guid);
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
            SendEvent(EventString.Dizi_Params_StateUpdate, Guid);
            SendEvent(EventString.Dizi_Lost_Start, Guid);
        }

        internal void RestoreFromLost()
        {
            State.StartIdle(SysTime.UnixNow);
            SendEvent(EventString.Dizi_Params_StateUpdate, Guid);
            SendEvent(EventString.Dizi_Lost_End, Guid);
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