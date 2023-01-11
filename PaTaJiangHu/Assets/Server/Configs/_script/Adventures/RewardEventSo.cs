using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Data;
using MyBox;
using Server.Configs.Items;
using UnityEngine;

namespace Server.Configs.Adventures
{
    public interface IGameReward
    {
        IGameItem[] Weapons { get; }
        IGameItem[] Armor { get; }
        IGameItem[] Medicines { get; }
        IGameItem[] Scrolls { get; }
        IGameItem[] StoryProps { get; }
        IGameItem[] FunctionProps { get; }
        IGameItem[] AllItems { get; }
    }

    [CreateAssetMenu(fileName = "id_奖励件名", menuName = "事件/奖励事件")]
    internal class RewardEventSo : AdvEventSoBase
    {
        [SerializeField] private string 事件名 = "奖励";
        [SerializeField] private AdvEventSoBase 下个事件;
        [SerializeField] private RewardField 奖励;
        [SerializeField] private int 弟子经验;
        private RewardField GameReward => 奖励;

        private int Exp => 弟子经验;
        //[SerializeField] private int _id;
        public override string Name => 事件名;
        //public override int Id => _id;
        public override void EventInvoke(IAdvEventArg arg)
        {
            OnLogsTrigger?.Invoke(GenerateRewardMessages(arg.DiziName));
            OnNextEvent?.Invoke(Next);
        }

        private string[] GenerateRewardMessages(string diziName)
        {
            var messages = new List<string>(GameReward.AllItemFields.Select(m => GenerateLogFromItem(m, diziName)));
            messages.Insert(0, $"{diziName}获得经验【{Exp}】");
            return messages.ToArray();
        }
        private string GenerateLogFromItem(GameItem item, string diziName) => $"{diziName}获得【{item.Name}】x{item.Amount}。";

        public override event Action<IAdvEvent> OnNextEvent;
        public override IAdvEvent[] AllEvents => new[] { Next };
        public override AdvTypes AdvType => AdvTypes.Reward;
        public override event Action<string[]> OnLogsTrigger;
        private IAdvEvent Next => 下个事件;
        public IGameReward Reward => GameReward;

        [Serializable] private class RewardField : IGameReward
        {
            [ConditionalField(true, nameof(UpdateAllList))] [SerializeField] private string justUpdate;
            [SerializeField] private WeaponItem[] 武器;
            [SerializeField] private ArmorItem[] 防具;
            [SerializeField] private MedicineItem[] 丹药;
            [SerializeField] private BookItem[] 秘籍;
            [SerializeField] private StoryPropItem[] 故事道具;
            [SerializeField] private FunctionPropItem[] 功能道具;

            public IGameItem[] Weapons => 武器;
            public IGameItem[] Armor => 防具;
            public IGameItem[] Medicines => 丹药;
            public IGameItem[] Scrolls => 秘籍;
            public IGameItem[] StoryProps => 故事道具;
            public IGameItem[] FunctionProps => 功能道具;

            public IGameItem[] AllItems => Weapons.Concat(Armor)
                .Concat(Medicines)
                .Concat(Scrolls)
                .Concat(StoryProps)
                .Concat(FunctionProps)
                .ToArray();

            public GameItem[] AllItemFields => 武器.Cast<GameItem>()
                .Concat(防具).Concat(丹药).Concat(秘籍).Concat(故事道具)
                .Concat(功能道具).ToArray();

            private bool UpdateAllList()
            {
                UpdateList(武器, GameItem.Kinds.Weapon);
                UpdateList(防具, GameItem.Kinds.Armor);
                UpdateList(丹药, GameItem.Kinds.Medicine);
                UpdateList(秘籍, GameItem.Kinds.Book);
                UpdateList(故事道具, GameItem.Kinds.StoryProp);
                UpdateList(功能道具, GameItem.Kinds.FunctionProp);
                return false;
            }

            private void UpdateList(GameItem[] gi, GameItem.Kinds kind) => gi?.ForEach(g => g?.UpdateList(kind));
        }
        [Serializable] private class WeaponItem : GameItem
        {
            [ConditionalField(true, nameof(TryUseSo))][SerializeField] private WeaponFieldSo 武器;
            protected override bool IsSupportSo(out ScriptableObject so)
            {
                so = null;
                if (Kind != Kinds.Weapon) return false;
                so = 武器;
                return true;
            }
        }
        [Serializable] private class ArmorItem : GameItem
        {
            [ConditionalField(true, nameof(TryUseSo))][SerializeField] private ArmorFieldSo 装备;
            protected override bool IsSupportSo(out ScriptableObject so)
            {
                so = null;
                if (Kind != Kinds.Armor) return false;
                so = 装备;
                return true;
            }
        }
        [Serializable] private class MedicineItem : GameItem
        {
            protected override bool IsSupportSo(out ScriptableObject so)
            {
                so = null;
                return true;
            }
        }
        [Serializable] private class BookItem : GameItem
        {
            [ConditionalField(true, nameof(TryUseSo))][SerializeField] private BookSoBase 残卷;
            protected override bool IsSupportSo(out ScriptableObject so)
            {
                so = null;
                if (Kind != Kinds.Book) return false;
                so = 残卷;
                return true;
            }
        }
        [Serializable] private class StoryPropItem : GameItem
        {
            protected override bool IsSupportSo(out ScriptableObject so)
            {
                so = null;
                return true;
            }
        }
        [Serializable] private class FunctionPropItem : GameItem
        {
            protected override bool IsSupportSo(out ScriptableObject so)
            {
                so = null;
                return true;
            }
        }

        [Serializable] private abstract class GameItem : IGameItem
        {
            private const string m_Weapon = "武器";
            private const string m_Armor = "防具";
            private const string m_Medicine= "丹药";
            private const string m_Book= "秘籍";
            private const string m_StoryProp= "故事道具";
            private const string m_FunctionProp= "功能道具";
            public enum Kinds
            {
                [InspectorName(m_Weapon)] Weapon,
                [InspectorName(m_Armor)] Armor,
                [InspectorName(m_Medicine)] Medicine,
                [InspectorName(m_Book)] Book,
                [InspectorName(m_StoryProp)] StoryProp,
                [InspectorName(m_FunctionProp)] FunctionProp
            }

            [HideInInspector][SerializeField] private string _name;
            [HideInInspector][SerializeField] private Kinds _kind;
            //[ConditionalField(true, nameof(IsSupportSo))]
            [ConditionalField(true, nameof(CheckSupport))] [SerializeField] private bool 引用;
            [ConditionalField(true, nameof(TryUseSo), true)] [SerializeField] private int _id;
            
            [SerializeField] private int 数量 = 1;
            public int Id => _id;
            public string Name => _name;
            public int Amount => 数量;

            public ItemType Type => Kind switch
            {
                Kinds.Weapon => ItemType.Equipment,
                Kinds.Armor => ItemType.Equipment,
                Kinds.Medicine => ItemType.Medicine,
                Kinds.Book => ItemType.Book,
                Kinds.StoryProp => ItemType.StoryProps,
                Kinds.FunctionProp => ItemType.StoryProps,
                _ => throw new ArgumentOutOfRangeException()
            };
            public Kinds Kind => _kind;

            private bool UseSo() => 引用;

            public void UpdateList(Kinds kind)
            {
                _kind = kind;
                TryUseSo();
            }
            
            protected bool TryUseSo()
            {
                ScriptableObject so = null;
                var supported = IsSupportSo(out so);
                if (!UseSo())
                {
                    UpdateName(null);
                    return false;
                }
                
                UpdateName(so);
                return supported;
            }

            private bool CheckSupport() => IsSupportSo(out _);
            protected abstract bool IsSupportSo(out ScriptableObject so);
            private void UpdateName(ScriptableObject so)
            {
                var title = "未命名";
                var id = Id;
                if (so)
                {
                    var d = so.As<IDataElement>();
                    title = d.Name;
                    id = d.Id;
                }
                _name = $"{id}.{title}({GetKindName(_kind)}):{Amount}";
            }
            public static string GetKindName(Kinds kind)
            {
                return kind switch
                {
                    Kinds.Weapon => m_Weapon,
                    Kinds.Armor => m_Armor,
                    Kinds.Medicine => m_Medicine,
                    Kinds.Book => m_Book,
                    Kinds.StoryProp => m_StoryProp,
                    Kinds.FunctionProp => m_FunctionProp,
                    _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
                };
            }
        }
    }
}