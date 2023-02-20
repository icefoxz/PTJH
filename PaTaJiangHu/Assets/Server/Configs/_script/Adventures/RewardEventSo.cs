﻿using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Data;
using MyBox;
using Server.Configs.Items;
using UnityEngine;

namespace Server.Configs.Adventures
{
    public interface IAdvPackage
    {
        int Grade { get; }
        IStacking<IGameItem>[] AllItems { get; }
    }
    public interface IGameReward
    {
        IAdvPackage[] Packages { get; }
        IStacking<IGameItem>[] AllItems { get; }
    }

    public interface IRewardHandler
    {
        void SetReward(IGameReward reward);
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
            arg.Handler.SetReward(Reward);
            arg.Adjustment.Set(IAdjustment.Types.Exp, Exp, false);
            OnNextEvent?.Invoke(Next);
        }

        private string[] GenerateRewardMessages(string diziName)
        {
            var messages = new List<string>(GameReward.AllItemFields.Select(m => GenerateLogFromItem(m, diziName)));
            if (Exp > 0) messages.Insert(0, $"{diziName}获得经验【{Exp}】");
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
            [SerializeField] private AdvPackage[] 奖励包;
            [SerializeField] private WeaponItem[] 武器;
            [SerializeField] private ArmorItem[] 防具;
            [SerializeField] private MedicineItem[] 丹药;
            [SerializeField] private BookItem[] 秘籍;
            [SerializeField] private StoryPropItem[] 故事道具;
            [SerializeField] private FunctionPropItem[] 功能道具;

            public IAdvPackage[] Packages => 奖励包;
            public IStacking<IGameItem>[] Weapons => 武器;
            public IStacking<IGameItem>[] Armor => 防具;
            public IStacking<IGameItem>[] Medicines => 丹药;
            public IStacking<IGameItem>[] Book => 秘籍;
            public IStacking<IGameItem>[] StoryProps => 故事道具;
            public IStacking<IGameItem>[] FunctionProps => 功能道具;

            public IStacking<IGameItem>[] AllItems => Weapons.Concat(Armor)
                .Concat(Medicines)
                .Concat(Book)
                .Concat(StoryProps)     
                .Concat(FunctionProps)
                .ToArray();

            public GameItem[] AllItemFields => 武器.Cast<GameItem>()
                .Concat(防具).Concat(丹药).Concat(秘籍).Concat(故事道具)
                .Concat(功能道具).ToArray();
        }
        [Serializable] private class AdvPackage : IAdvPackage
        {
            private bool ChangeElementName()
            {
                var weaponText = GetText("武器", Weapons?.Sum(w => w.Amount) ?? 0);
                var armorText = GetText("防具", Armor?.Sum(w => w.Amount) ?? 0);
                var medicineText = GetText("丹药", Medicines?.Sum(w => w.Amount) ?? 0);
                var bookText = GetText("秘籍", Book?.Sum(w => w.Amount) ?? 0);
                var list = new List<string> { weaponText, armorText, medicineText, bookText };
                _name = string.Join(',', list.Where(s => !string.IsNullOrWhiteSpace(s)));
                return true;

                string GetText(string title,int count) => count > 0 ? $"{title}x{count}" : string.Empty;
            }

            [ConditionalField(true,nameof(ChangeElementName))][SerializeField][ReadOnly]private string _name;

            [SerializeField] private int 品级;
            [SerializeField] private WeaponItem[] 武器;
            [SerializeField] private ArmorItem[] 防具;
            [SerializeField] private MedicineItem[] 丹药;
            [SerializeField] private BookItem[] 秘籍;
            public IStacking<IGameItem>[] Weapons => 武器;
            public IStacking<IGameItem>[] Armor => 防具;
            public IStacking<IGameItem>[] Medicines => 丹药;
            public IStacking<IGameItem>[] Book => 秘籍;

            public int Grade => 品级;
            public IStacking<IGameItem>[] AllItems => Weapons
                .Concat(Armor)
                .Concat(Medicines)
                .Concat(Book)
                .ToArray();
        }
        [Serializable] private class WeaponItem : GameItem, IEquipment
        {
            [ConditionalField(true, nameof(TryUseSo))][SerializeField] private WeaponFieldSo 武器;
            
            public override Kinds Kind => Kinds.Weapon;
            protected override ScriptableObject So => 武器;
            protected override IGameItem Gi => 武器;
            protected override bool IsSupportSo => true;
            public EquipKinds EquipKind => EquipKinds.Weapon;
        }
        [Serializable] private class ArmorItem : GameItem, IEquipment
        {
            [ConditionalField(true, nameof(TryUseSo))][SerializeField] private ArmorFieldSo 防具;
            public override Kinds Kind => Kinds.Armor;
            protected override ScriptableObject So => 防具;
            protected override IGameItem Gi => 防具;
            protected override bool IsSupportSo => true;
            public EquipKinds EquipKind => EquipKinds.Armor;
        }
        [Serializable] private class MedicineItem : GameItem
        {
            [ConditionalField(true, nameof(TryUseSo))][SerializeField] private MedicineFieldSo 丹药;
            public override Kinds Kind => Kinds.Medicine;
            protected override ScriptableObject So => 丹药;
            protected override IGameItem Gi => 丹药;
            protected override bool IsSupportSo => true;
        }
        [Serializable] private class BookItem : GameItem
        {
            [ConditionalField(true, nameof(TryUseSo))][SerializeField] private BookSoBase 残卷;
            public override Kinds Kind => Kinds.Book;
            protected override ScriptableObject So => 残卷;
            protected override IGameItem Gi => 残卷;
            protected override bool IsSupportSo => true;
        }
        [Serializable] private class StoryPropItem : GameItem
        {
            public override Kinds Kind => Kinds.StoryProp;
            protected override ScriptableObject So { get; }
            protected override IGameItem Gi { get; }
            protected override bool IsSupportSo => false;
        }
        [Serializable] private class FunctionPropItem : GameItem
        {
            public override Kinds Kind => Kinds.FunctionProp;
            protected override ScriptableObject So { get; }
            protected override IGameItem Gi { get; }
            protected override bool IsSupportSo => false;
        }

        [Serializable] private abstract class GameItem : IStacking<IGameItem>,IGameItem
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
            //[ConditionalField(true, nameof(IsSupportSo))]
            [ConditionalField(true, nameof(CheckSupport))] [SerializeField] private bool 引用;
            [ConditionalField(true, nameof(TryUseSo), true)] [SerializeField] private int _id;
            
            [SerializeField] private int 数量 = 1;
            public int Price => Gi.Price;
            public int Id => UseSo() ? Gi?.Id ?? 0 : _id;
            public string Name => Gi.Name;
            public string About => Gi.About;

            public IGameItem Item => this;
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
            public abstract Kinds Kind { get; }
            protected abstract ScriptableObject So { get; }
            protected abstract IGameItem Gi { get; }
            private bool UseSo() => 引用;
            protected abstract bool IsSupportSo { get; }

            protected bool TryUseSo()
            {
                ScriptableObject so = So;
                var supported = IsSupportSo;
                if (!UseSo())
                {
                    UpdateName(null);
                    return false;
                }
                
                UpdateName(so);
                return supported;
            }

            private bool CheckSupport() => IsSupportSo;

            private void UpdateName(ScriptableObject so)
            {
                var title = "未命名";
                var id = Id;
                if (so)
                {
                    var d = (IDataElement)so;
                    title = d.Name;
                    id = d.Id;
                }
                _name = $"{id}.{title}({GetKindName(Kind)}):{Amount}";
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