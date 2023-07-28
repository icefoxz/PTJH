using System;
using System.Collections.Generic;
using System.Linq;
using AOT.Core;
using AOT.Core.Data;
using AOT.Core.Dizi;
using GameClient.Modules.BattleM;
using GameClient.Modules.DiziM;
using GameClient.SoScripts.Items;
using MyBox;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameClient.SoScripts.Fields
{
    [Serializable]
    internal class DiziRewardField : IDiziReward
    {
        [SerializeField] private int 经验;

        public int Exp => 经验;
    }
    [Serializable]
    internal class RewardField : IGameReward
    {
        [SerializeField] private ItemPackage[] 奖励包;
        [SerializeField] private WeaponItem[] 武器;
        [SerializeField] private ArmorItem[] 防具;
        [SerializeField] private ShoesItem[] 鞋子;
        [SerializeField] private DecorationItem[] 挂件;
        [SerializeField] private MedicineItem[] 丹药;
        [SerializeField] private BookItem[] 秘籍;
        [SerializeField] private StoryPropItem[] 故事道具;
        [SerializeField] private FunctionAdvItem[] 功能道具;

        public IItemPackage[] Packages => 奖励包;
        public IGameItem[] Weapons => 武器;
        public IGameItem[] Armor => 防具;
        public IGameItem[] Medicines => 丹药;
        public IGameItem[] Book => 秘籍;
        public IGameItem[] StoryProps => 故事道具;
        public IGameItem[] FunctionProps => 功能道具;
        public IGameItem[] Shoes => 鞋子;
        public IGameItem[] Decoration => 挂件;

        public IGameItem[] AllItems => Weapons
            .Concat(Armor)
            .Concat(Shoes)
            .Concat(Decoration)
            .Concat(Medicines)
            .Concat(Book)
            .Concat(StoryProps)
            .Concat(FunctionProps)
            .ToArray();

        public GameItemField[] AllItemFields => 武器.Cast<GameItemField>()
            .Concat(鞋子).Concat(挂件).Concat(防具)
            .Concat(丹药).Concat(秘籍)
            .Concat(故事道具)
            .Concat(功能道具).ToArray();
    }

    [Serializable]
    internal class ItemPackage : IItemPackage, IResPackage
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
            var weaponText = GetText("武器", Weapons?.Length ?? 0);
            var armorText = GetText("防具", Armor?.Length ?? 0);
            var medicineText = GetText("丹药", Medicines?.Length ?? 0);
            var bookText = GetText("秘籍", Book?.Length ?? 0);
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
        public IGameItem[] Weapons => 武器;
        public IGameItem[] Armor => 防具;
        public IGameItem[] Medicines => 丹药;
        public IGameItem[] Book => 秘籍;

        public int Grade => 品级;

        public IGameItem[] AllItems => Weapons
            .Concat(Armor)
            .Concat(Medicines)
            .Concat(Book)
            .ToArray();

        public IResPackage Package => this;
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
        public ColorGrade Grade => W.Grade;
        public WeaponArmed Armed => W.Armed;
        public int Quality => W.Quality;
        public float GetAddOn(DiziProps prop)=> W.GetAddOn(prop);
        public ICombatSet GetCombatSet() => W.GetCombatSet();

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
        public ColorGrade Grade => A.Grade;
        public int Quality => A.Quality;
        public float GetAddOn(DiziProps prop)=> A.GetAddOn(prop);
        public ICombatSet GetCombatSet() => A.GetCombatSet();
    }
    
    [Serializable]
    internal class ShoesItem : GameItemField, IShoes
    {
        [ConditionalField(true, nameof(TryUseSo))]
        [SerializeField]
        private ShoesFieldSo 鞋子;

        public override ItemType Type => ItemType.Equipment;
        public override Sprite Icon => Gi.Icon;
        protected override ScriptableObject So => 鞋子;
        protected override IGameItem Gi => 鞋子;
        private ShoesFieldSo A => 鞋子;
        public EquipKinds EquipKind => A.EquipKind;
        public ColorGrade Grade => A.Grade;
        public int Quality => A.Quality;
        public float GetAddOn(DiziProps prop)=> A.GetAddOn(prop);
        public ICombatSet GetCombatSet() => A.GetCombatSet();
    }

    [Serializable] internal class DecorationItem : GameItemField, IShoes
    {
        [ConditionalField(true, nameof(TryUseSo))]
        [SerializeField]
        private DecorationFieldSo 挂件;

        public override ItemType Type => ItemType.Equipment;
        public override Sprite Icon => Gi.Icon;
        protected override ScriptableObject So => 挂件;
        protected override IGameItem Gi => 挂件;
        private DecorationFieldSo A => 挂件;
        public EquipKinds EquipKind => A.EquipKind;
        public ColorGrade Grade => A.Grade;
        public int Quality => A.Quality;
        public float GetAddOn(DiziProps prop)=> A.GetAddOn(prop);
        public ICombatSet GetCombatSet() => A.GetCombatSet();
    }

    [Serializable]
    internal class MedicineItem : FunctionItem
    {
        [ConditionalField(true, nameof(TryUseSo))] [SerializeField] private MedicineFieldSo 丹药;
        public override FunctionItemType FunctionType => FunctionItemType.Medicine;
        protected override ScriptableObject So => 丹药;
        protected override IGameItem Gi => 丹药;
    }

    [Serializable] internal class BookItem : GameItemField
    {
        [ConditionalField(true, nameof(TryUseSo))]
        [FormerlySerializedAs("残卷")][SerializeField]
        private BookSoBase 书籍;

        public override ItemType Type => ItemType.Book;
        public override Sprite Icon => Gi.Icon;
        protected override ScriptableObject So => 书籍;
        protected override IGameItem Gi => 书籍;
    }

    [Serializable] internal class StoryPropItem : FunctionItem
    {
        protected override ScriptableObject So { get; }
        protected override IGameItem Gi { get; }
        public override FunctionItemType FunctionType => FunctionItemType.StoryProps;
    }

    [Serializable]
    internal class FunctionAdvItem : FunctionItem
    {
        public override ItemType Type => ItemType.FunctionItem;
        public override Sprite Icon => Gi.Icon;
        protected override ScriptableObject So { get; }
        protected override IGameItem Gi { get; }
        public override FunctionItemType FunctionType { get; } = FunctionItemType.AdvItem;
    }
    internal abstract class FunctionItem : GameItemField, IFunctionItem
    {
        public override ItemType Type => ItemType.FunctionItem;
        public override Sprite Icon => Gi.Icon;
        public abstract FunctionItemType FunctionType { get; }
    }

    [Serializable]
    internal abstract class GameItemField : IGameItem
    {
        [HideInInspector][SerializeField] private string _name;

        public int Id => Gi?.Id ?? 0;
        public string Name => Gi.Name;
        public string About => Gi.About;

        public IGameItem Item => this;
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
            _name = $"{id}.{title}";
        }
    }
}