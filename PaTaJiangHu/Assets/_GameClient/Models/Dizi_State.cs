using System;
using System.Collections.Generic;
using System.Linq;
using AOT.Core;
using GameClient.SoScripts.Items;
using GameClient.System;

namespace GameClient.Models
{
    //弟子模型,处理状态
    public partial class Dizi
    {
        private GameWorld.DiziState WorldState => Game.World.State;

        public IEnumerable<IGameItem> Items => Activity == DiziActivities.Adventure
            ? WorldState.Adventure.GetActivity(Guid).Logs.SelectMany(a =>
                a.Reward.AllItems.Concat(a.Reward.Packages.SelectMany(p => p.AllItems)))
            : Array.Empty<IGameItem>();

        /// <summary>
        /// 状态信息, 提供当前状态的描述,与时长
        /// </summary>
        public DiziActivities Activity => State?.Activity ?? DiziActivities.None;
        public IDiziState State { get; private set; }
        public void SetState(IDiziState state) => State = state;
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

        internal AdvItemModel(IAdvItem item)
        {
            Kind = item.FunctionType switch
            {
                FunctionItemType.Medicine => Kinds.Medicine,
                FunctionItemType.AdvItem => Kinds.Horse,
                FunctionItemType.StoryProps => Kinds.StoryProp,
                _ => throw new ArgumentOutOfRangeException($"物品{item.Type}不支持! ")
            };
            Item = item;
        }
    }
}