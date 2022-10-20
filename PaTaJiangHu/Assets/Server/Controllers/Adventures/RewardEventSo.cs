using System;
using Data;
using MyBox;
using So;
using UnityEditor;
using UnityEngine;

namespace Server.Controllers.Adventures
{
    public interface IGameItem
    {
        int Id { get; }
        int Amount { get; }
    }
    public interface IGameReward
    {
        IGameItem[] Weapons { get; }
        IGameItem[] Armor { get; }
        IGameItem[] Medicines { get; }
        IGameItem[] Scrolls { get; }
        IGameItem[] StoryProps { get; }
        IGameItem[] FunctionProps { get; }
    }

    [CreateAssetMenu(fileName = "id_奖励件名", menuName = "副本/奖励事件")]
    internal class RewardEventSo : AdvEventSoBase
    {
        [SerializeField] private AdvEventSoBase 下个事件;
        [SerializeField] private RewardField 奖励;
        
        //[SerializeField] private int _id;
        //public override int Id => _id;
        public override IAdvEvent[] PossibleEvents => new[] { Next };
        public override AdvTypes AdvType => AdvTypes.Reward;
        private IAdvEvent Next => 下个事件;
        public IGameReward Reward => 奖励;
        protected override Action<IAdvEvent> OnResultCallback { get; set; }
        public void NextEvent() => OnResultCallback?.Invoke(Next);

        [Serializable] private class RewardField : IGameReward
        {
            [ConditionalField(true, nameof(UpdateAllList))] [SerializeField] private string justUpdate;
            [SerializeField] private GameItem[] 武器;
            [SerializeField] private GameItem[] 防具;
            [SerializeField] private GameItem[] 丹药;
            [SerializeField] private GameItem[] 秘籍;
            [SerializeField] private GameItem[] 故事道具;
            [SerializeField] private GameItem[] 功能道具;

            public IGameItem[] Weapons => 武器;
            public IGameItem[] Armor => 防具;
            public IGameItem[] Medicines => 丹药;
            public IGameItem[] Scrolls => 秘籍;
            public IGameItem[] StoryProps => 故事道具;
            public IGameItem[] FunctionProps => 功能道具;

            private bool UpdateAllList()
            {
                UpdateList(武器, GameItem.Kinds.Weapon);
                UpdateList(防具, GameItem.Kinds.Armor);
                UpdateList(丹药, GameItem.Kinds.Medicine);
                UpdateList(秘籍, GameItem.Kinds.Scroll);
                UpdateList(故事道具, GameItem.Kinds.StoryProp);
                UpdateList(功能道具, GameItem.Kinds.FunctionProp);
                return false;
            }

            private void UpdateList(GameItem[] gi, GameItem.Kinds kind) => gi.ForEach(g => g.UpdateList(kind));
        }
        [Serializable] private class GameItem : IGameItem
        {
            private const string m_Weapon = "武器";
            private const string m_Armor = "防具";
            private const string m_Medicine= "丹药";
            private const string m_Scroll= "秘籍";
            private const string m_StoryProp= "故事道具";
            private const string m_FunctionProp= "功能道具";
            public enum Kinds
            {
                [InspectorName(m_Weapon)] Weapon,
                [InspectorName(m_Armor)] Armor,
                [InspectorName(m_Medicine)] Medicine,
                [InspectorName(m_Scroll)] Scroll,
                [InspectorName(m_StoryProp)] StoryProp,
                [InspectorName(m_FunctionProp)] FunctionProp
            }

            [HideInInspector][SerializeField] private string _name;
            [HideInInspector][SerializeField] private Kinds _kind;
            //[ConditionalField(true, nameof(IsSupportSo))]
            [ConditionalField(true, nameof(CheckSupport))] [SerializeField] private bool 引用;
            [ConditionalField(true, nameof(TryUseSo), true)] [SerializeField] private int _id;
            [ConditionalField(true, nameof(TryUseSo))] [SerializeField] private ScrollSoBase 残卷;
            [SerializeField] private int 数量 = 1;
            public int Id => _id;
            public int Amount => 数量;
            private Kinds Kind => _kind;

            private bool UseSo() => 引用;

            public void UpdateList(Kinds kind)
            {
                _kind = kind;
                TryUseSo();
            }

            private bool TryUseSo()
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
            private bool IsSupportSo(out ScriptableObject so)
            {
                so = null;
                switch (Kind)
                {
                    case Kinds.Weapon:
                        break;
                    case Kinds.Armor:
                        break;
                    case Kinds.Medicine:
                        break;
                    case Kinds.Scroll:
                        so = 残卷;
                        return true;
                    case Kinds.StoryProp:
                        break;
                    case Kinds.FunctionProp:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return false;
            }
            private void UpdateName(ScriptableObject so)
            {
                var title = string.Empty;
                var id = Id;
                if (so)
                {
                    var d = so.As<IDataElement>();
                    title = d.Name;
                    id = d.Id;
                }
                _name = $"{id}.{title}({GetKindName(_kind)}):{Amount}";
            }
            private string GetKindName(Kinds kind)
            {
                return kind switch
                {
                    Kinds.Weapon => m_Weapon,
                    Kinds.Armor => m_Armor,
                    Kinds.Medicine => m_Medicine,
                    Kinds.Scroll => m_Scroll,
                    Kinds.StoryProp => m_StoryProp,
                    Kinds.FunctionProp => m_FunctionProp,
                    _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
                };
            }
        }
    }
}