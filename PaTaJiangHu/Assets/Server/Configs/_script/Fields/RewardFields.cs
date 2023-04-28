using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Data;
using MyBox;
using Server.Configs.Adventures;
using Server.Configs.Battles;
using Server.Configs.Items;
using Server.Controllers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Server.Configs.Fields
{
    [Serializable]
    internal class RewardField : IGameReward
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

        public GameItemField[] AllItemFields => 武器.Cast<GameItemField>()
            .Concat(防具).Concat(丹药).Concat(秘籍).Concat(故事道具)
            .Concat(功能道具).ToArray();
    }

    [Serializable]
    internal class AdvPackage : IAdvPackage, IAdvResPackage
    {
        private enum PackageResources
        {
            [InspectorName("银两")] Silver,
            [InspectorName("食物")] Food,
            [InspectorName("酒水")] Wine,
            [InspectorName("药草")] Herb,
            [InspectorName("丹药")] Pill,
            [InspectorName("元宝")] YuanBao,
        }

        private bool ChangeElementName()
        {
            var weaponText = GetText("武器", Weapons?.Sum(w => w.Amount) ?? 0);
            var armorText = GetText("防具", Armor?.Sum(w => w.Amount) ?? 0);
            var medicineText = GetText("丹药", Medicines?.Sum(w => w.Amount) ?? 0);
            var bookText = GetText("秘籍", Book?.Sum(w => w.Amount) ?? 0);
            var list = new List<string> { weaponText, armorText, medicineText, bookText };
            _name = string.Join(',', list.Where(s => !string.IsNullOrWhiteSpace(s)));
            return true;

            string GetText(string title, int count) => count > 0 ? $"{title}x{count}" : string.Empty;
        }

        [ConditionalField(true, nameof(ChangeElementName))] [SerializeField] 
        [ReadOnly] private string _name;

        [SerializeField] private int 品级;
        [SerializeField] private ConsumeResourceField[] 资源;
        [SerializeField] private WeaponItem[] 武器;
        [SerializeField] private ArmorItem[] 防具;
        [SerializeField] private MedicineItem[] 丹药;
        [SerializeField] private BookItem[] 秘籍;
        private ConsumeResourceField[] Resources => 资源;
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

        public IAdvResPackage Package => this;
        public int Food => Resources.SingleOrDefault(r => r.Resource == PackageResources.Food)?.Value ?? 0;
        public int Wine => Resources.SingleOrDefault(r => r.Resource == PackageResources.Wine)?.Value ?? 0;
        public int Herb => Resources.SingleOrDefault(r => r.Resource == PackageResources.Herb)?.Value ?? 0;
        public int Pill => Resources.SingleOrDefault(r => r.Resource == PackageResources.Pill)?.Value ?? 0;
        public int Silver => Resources.SingleOrDefault(r => r.Resource == PackageResources.Silver)?.Value ?? 0;
        public int YuanBao => Resources.SingleOrDefault(r => r.Resource == PackageResources.YuanBao)?.Value ?? 0;

        [Serializable]
        private class ConsumeResourceField
        {
            private bool ChangeConsumeResourceName()
            {
                if (Value <= 0)
                {
                    _name = "未设置资源!";
                    return true;
                }

                _name = $"{GetResourceName(Resource)}x{Value}";
                return true;
            }

            private string GetResourceName(PackageResources resource)
            {
                return resource switch
                {
                    PackageResources.Food => "食物",
                    PackageResources.Wine => "酒水",
                    PackageResources.Herb => "药草",
                    PackageResources.Pill => "丹药",
                    PackageResources.Silver => "银两",
                    PackageResources.YuanBao => "元宝",
                    _ => throw new ArgumentOutOfRangeException(nameof(resource), resource, null)
                };
            }

            [ConditionalField(true, nameof(ChangeConsumeResourceName))]
            [SerializeField]
            [ReadOnly]
            private string _name;

            [SerializeField] private PackageResources 资源;
            [SerializeField] private int 数量;
            public PackageResources Resource => 资源;
            public int Value => 数量 < 0 ? throw new InvalidOperationException($"资源数量 = {数量}!") : 数量;
        }
    }

    [Serializable]
    internal class WeaponItem : GameItemField, IWeapon
    {
        [ConditionalField(true, nameof(TryUseSo))]
        [SerializeField]
        private WeaponFieldSo 武器;

        public override ItemType Type => ItemType.Equipment;
        public override Sprite Icon => W.Icon;
        protected override ScriptableObject So => 武器;
        protected override IGameItem Gi => 武器;
        private WeaponFieldSo W => 武器;
        public EquipKinds EquipKind => W.EquipKind;
        public DiziGrades Grade => W.Grade;
        public WeaponArmed Armed => W.Armed;
        public int Damage => W.Damage;
    }

    [Serializable]
    internal class ArmorItem : GameItemField, IArmor
    {
        [ConditionalField(true, nameof(TryUseSo))]
        [SerializeField]
        private ArmorFieldSo 防具;

        public override ItemType Type => ItemType.Equipment;
        public override Sprite Icon => Gi.Icon;
        protected override ScriptableObject So => 防具;
        protected override IGameItem Gi => 防具;
        private ArmorFieldSo A => 防具;
        public EquipKinds EquipKind => A.EquipKind;
        public DiziGrades Grade => A.Grade;
        public int AddHp => A.AddHp;
    }

    [Serializable]
    internal class MedicineItem : GameItemField
    {
        [ConditionalField(true, nameof(TryUseSo))]
        [SerializeField]
        private MedicineFieldSo 丹药;

        public override ItemType Type => ItemType.Medicine;
        public override Sprite Icon => Gi.Icon;
        protected override ScriptableObject So => 丹药;
        protected override IGameItem Gi => 丹药;
    }

    [Serializable]
    internal class BookItem : GameItemField
    {
        [ConditionalField(true, nameof(TryUseSo))]
        [FormerlySerializedAs("残卷")][SerializeField]
        private BookSoBase 书籍;

        public override ItemType Type => ItemType.Book;
        public override Sprite Icon => Gi.Icon;
        protected override ScriptableObject So => 书籍;
        protected override IGameItem Gi => 书籍;
    }

    [Serializable]
    internal class StoryPropItem : GameItemField
    {
        public override ItemType Type => ItemType.StoryProps;
        public override Sprite Icon => Gi.Icon;
        protected override ScriptableObject So { get; }
        protected override IGameItem Gi { get; }
    }

    [Serializable]
    internal class FunctionPropItem : GameItemField
    {
        public override ItemType Type => ItemType.FunctionItem;
        public override Sprite Icon => Gi.Icon;
        protected override ScriptableObject So { get; }
        protected override IGameItem Gi { get; }
    }

    [Serializable]
    internal abstract class GameItemField : IStacking<IGameItem>, IGameItem
    {
        [HideInInspector][SerializeField] private string _name;

        [SerializeField] private int 数量 = 1;
        public int Id => Gi?.Id ?? 0;
        public string Name => Gi.Name;
        public string About => Gi.About;

        public IGameItem Item => this;
        public int Amount => 数量;

        public abstract ItemType Type { get; }
        public abstract Sprite Icon { get; }
        protected abstract ScriptableObject So { get; }
        protected abstract IGameItem Gi { get; }

        protected bool TryUseSo()
        {
            UpdateName(So);
            return true;
        }

        private void UpdateName(ScriptableObject so)
        {
            var title = "未命名";
            var id = Id;
            if (so)
            {
                var d = (IDataElement)so;
                if(Gi!=null)
                {
                    title = Gi.Name;
                    id = Gi.Id;
                }
            }
            _name = $"{id}.{title}:{Amount}";
        }
    }
}