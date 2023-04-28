﻿using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Models;
using Server.Configs.Adventures;
using Server.Configs.Battles;
using Server.Configs.Factions;
using Server.Configs.Items;
using Server.Controllers;
using Utls;

namespace _GameClient.Models
{
    public interface IFaction
    {
        int Silver { get; }
        int YuanBao { get; }
        int ActionLing { get; }
        int ActionLingMax { get; }
        IReadOnlyList<Dizi> DiziList { get; }
        IReadOnlyList<IWeapon> Weapons { get; }
        IReadOnlyList<IArmor> Armors { get; }
        Dizi GetDizi(string guid);
    }

    /// <summary>
    /// 门派模型
    /// </summary>
    public class Faction : ModelBase, IFaction
    {
        protected override string LogPrefix { get; } = "门派";
        private List<IBook> _books = new List<IBook>();
        private List<IWeapon> _weapons= new List<IWeapon>();
        private List<IArmor> _armors = new List<IArmor>();
        private List<IAdvPackage> _packages = new List<IAdvPackage>();
        private List<IGameItem> _advItems = new List<IGameItem>();
        private List<Dizi> _diziList = new List<Dizi>();
        private Dictionary<IFunctionItem,int> _funcItems = new Dictionary<IFunctionItem, int>();
        private Dictionary<IMedicine,int> Medicines { get; } = new Dictionary<IMedicine,int>();

        public int Silver { get; private set; }
        public int YuanBao { get; private set; }
        public int Food { get; private set; }
        public int Wine { get; private set; }
        public int Pill { get; private set; }
        public int Herb { get; private set; }

        public int ActionLing { get; private set; }
        public int ActionLingMax { get; }
        public int MaxDizi => DiziMap.Count;
        /// <summary>
        /// key = dizi.Guid, value = dizi
        /// </summary>
        private Dictionary<string,Dizi> DiziMap { get; }
        public IReadOnlyList<IWeapon> Weapons => _weapons;
        public IReadOnlyList<IArmor> Armors => _armors;
        public IReadOnlyList<Dizi> DiziList => _diziList;
        public IReadOnlyList<IAdvPackage> Packages => _packages;
        public IReadOnlyList<IBook> Books => _books;
        public IReadOnlyDictionary<IFunctionItem, int> FuncItems => _funcItems;
        public IReadOnlyList<IGameItem> AdvProps => _advItems;

        public IStacking<IGameItem>[] GetAllSupportedAdvItems() => Medicines
            .Where(m => m.Key.Kind == MedicineKinds.StaminaDrug)
            .Select(m => new Stacking<IGameItem>(m.Key, m.Value))
            .Concat(AdvProps.Select(p => new Stacking<IGameItem>(p, 1))).ToArray();

        public (IMedicine med, int amount)[] GetAllMedicines() => Medicines.Select(m => (m.Key, m.Value)).ToArray();
        public IBook GetBook(int bookId) => Books.FirstOrDefault(b => b.Id == bookId);
        public IStacking<IComprehendItem>[] GetAllComprehendItems(int supportedLevel) => FuncItems
            .Where(f => f.Key.FunctionType == FunctionItemType.Comprehend)
            .Cast<KeyValuePair<IComprehendItem, int>>()
            .Where(f => f.Key.IsLevelAvailable(supportedLevel))
            .Select(f => new Stacking<IComprehendItem>(f.Key, f.Value)).ToArray();

        internal Faction(int silver, int yuanBao, int actionLing,int actionLingMax, List<Dizi> diziMap,
            int food = 0, int wine = 0, int pill = 0, int herb = 0)
        {
            DiziMap = diziMap.ToDictionary(d => d.Guid.ToString(), d => d);
            Silver = silver;
            YuanBao = yuanBao;
            ActionLing = actionLing;
            ActionLingMax = actionLingMax;
            Food = food;
            Wine = wine;
            Pill = pill;
            Herb = herb;
        }

        internal void AddDizi(Dizi dizi)
        {
            DiziMap.Add(dizi.Guid, dizi);
            _diziList.Add(dizi);
            Log($"添加弟子{dizi.Name}");
            SendEvent(EventString.Faction_DiziAdd, dizi.Guid);
            SendEvent(EventString.Faction_DiziListUpdate, string.Empty);
        }

        internal void RemoveDizi(Dizi dizi)
        {
            Log($"移除弟子{dizi.Name}");
            DiziMap.Remove(dizi.Guid);
            _diziList.Remove(dizi);
        }

        internal void AddSilver(int silver)
        {
            if (silver == 0) return;
            var last = Silver;
            Silver += silver;
            Log($"银两【{last}】增加了{silver},总:【{Silver}】");
            SendEvent(EventString.Faction_SilverUpdate, Silver);
        }

        internal void AddYuanBao(int yuanBao)
        {
            if (yuanBao == 0) return;
            var last = YuanBao;
            YuanBao += yuanBao;
            Log($"元宝【{last}】增加了{yuanBao},总:【{YuanBao}】");
            SendEvent(EventString.Faction_YuanBaoUpdate, YuanBao);
        }

        internal void AddLing(int ling)
        {
            if (ling == 0) return;
            var last = ActionLing;
            ActionLing += ling;
            ActionLing = Math.Clamp(ActionLing, 0, ActionLingMax);
            Log($"行动令【{last}】增加了{ling},总:【{ActionLing}】");
            SendEvent(EventString.Faction_Params_ActionLingUpdate, ActionLing, 100, 0, 0);
        }

        internal void AddWeapon(IWeapon weapon)
        {
            if(weapon == null) LogError("Weapon = null!");
            _weapons.Add(weapon);
            Log($"添加武器【{weapon.Name}】");
        }
        internal void RemoveWeapon(IWeapon weapon)
        {
            if(weapon == null) LogError("Weapon = null!");
            _weapons.Remove(weapon);
            Log($"移除武器【{weapon.Name}】");
        }

        internal void AddArmor(IArmor armor)
        {
            if(armor == null) LogError("armor = null!");
            _armors.Add(armor);
            Log($"添加防具【{armor.Name}】");
        }

        internal void RemoveArmor(IArmor armor)
        {
            if(armor == null) LogError("armor = null!");
            _armors.Remove(armor);
            Log($"移除防具【{armor.Name}】");
        }

        #region Dictionary Items
        internal void AddMedicine(IMedicine med, int amount)
        {
            if(med == null) LogError("med = null!");
            DictionaryAdd(Medicines, med, amount);
            if(amount > 0) Log($"添加药品【{med.Name}】x{amount}, 总: {Medicines[med]}");
        }

        internal void RemoveMedicine(IMedicine med,int amount)
        {
            if (med == null) LogError("med = null!");
            DictionaryRemove(Medicines, med, amount);
            if(amount > 0) Log($"移除药品【{med.Name}】");
        }

        internal void AddFunctionItem(IFunctionItem item, int amount)
        {
            if (item == null) LogError("functionItem = null!");
            DictionaryAdd(_funcItems, item, amount);
            SendEvent(EventString.Faction_FunctionItemUpdate, string.Empty);
            Log($"添加功能道具: {item.Id}.{item.Name}");
        }

        internal void RemoveFunctionItem(IFunctionItem item, int amount)
        {
            if (item == null) LogError("item = null!");
            DictionaryRemove(_funcItems, item, amount);
            SendEvent(EventString.Faction_FunctionItemUpdate, string.Empty);
            Log($"移除功能道具: {item.Id}.{item.Name}");
        }

        private void DictionaryAdd<T>(Dictionary<T, int> dict, T key, int value) where T : IGameItem
        {
            if (value == 0) return;
            dict.TryAdd(key, 0);
            dict[key] += value;
        }

        private void DictionaryRemove<T>(Dictionary<T, int> dict, T key, int value) where T : IGameItem
        {
            if (value == 0) return;
            if (!dict.ContainsKey(key))
                LogError($"找不到{key.Name},Id = {key.Id}");
            if (dict[key] < value) LogError($"{key.Name}:{dict[key]} < {value}! ");
            dict[key] -= value;
        }
        #endregion

        internal void AddConsumeResource(ConsumeResources resource, int value)
        {
            if (value == 0) return;
            var lastValue = 0;
            switch (resource)
            {
                case ConsumeResources.Food:
                    lastValue = Food;
                    Food += value; 
                    SendEvent(EventString.Faction_FoodUpdate, Food);
                    break;
                case ConsumeResources.Wine:
                    lastValue = Wine;
                    Wine += value;
                    SendEvent(EventString.Faction_WineUpdate, Wine);
                    break;
                case ConsumeResources.Pill:
                    lastValue = Pill;
                    Pill += value;
                    SendEvent(EventString.Faction_PillUpdate, Pill);
                    break;
                case ConsumeResources.Herb:
                    lastValue = Herb;
                    Herb += value;
                    SendEvent(EventString.Faction_HerbUpdate, Herb);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(resource), resource, null);
            }
            Log($"增加资源{resource}:{lastValue} => {value} : {lastValue + value}");
        }

        public Dizi GetDizi(string guid)
        {
            if(DiziMap.TryGetValue(guid, out var dizi)) return dizi;
            LogWarning($"找不到弟子 = {guid}");
            return null;
        }

        /// <summary>
        /// 给门派加物件
        /// </summary>
        /// <param name="gi"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal void AddGameItem(IStacking<IGameItem> gi)
        {
            var data = Game.Controllers.Get<DataController>();
            switch (gi.Item.Type)
            {
                case ItemType.Medicine:
                    AddMedicine(data.GetMedicine(gi.Item.Id),gi.Amount);
                    break;
                case ItemType.Equipment:
                    var equipment = gi.Item as IEquipment;
                    if (equipment == null) XDebug.LogError($"物件{gi.Item.Id}.{gi.Item.Name} 未继承<IEquipment>");
                    for (var i = 0; i < gi.Amount; i++)
                    {
                        switch (equipment.EquipKind)
                        {
                            case EquipKinds.Weapon:
                                var weapon = data.GetWeapon(gi.Item.Id);
                                AddWeapon(weapon);
                                break;
                            case EquipKinds.Armor:
                                var armor = data.GetArmor(gi.Item.Id);
                                AddArmor(armor);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    break;
                case ItemType.Book:
                    AddBook(data.GetBook(gi.Item.Id));
                    break;
                case ItemType.AdvProps:
                    AddAdvItem(data.GetAdvProp(gi.Item.Id));
                    break;
                case ItemType.FunctionItem:
                    AddFunctionItem(data.GetFunctionItem(gi.Item.Id), gi.Amount);
                    break;
                case ItemType.StoryProps:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 给门派加物件
        /// </summary>
        /// <param name="gi"></param>
        /// <param name="amount"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal void RemoveGameItem(IGameItem gi, int amount)
        {
            var data = Game.Controllers.Get<DataController>();
            switch (gi.Type)
            {
                case ItemType.Medicine:
                    RemoveMedicine(data.GetMedicine(gi.Id), amount);
                    break;
                case ItemType.Equipment:
                    var equipment = gi as IEquipment;
                    if (equipment == null) XDebug.LogError($"物件{gi.Id}.{gi.Name} 未继承<IEquipment>");
                    for (var i = 0; i < amount; i++)
                    {
                        switch (equipment.EquipKind)
                        {
                            case EquipKinds.Weapon:
                                var weapon = data.GetWeapon(gi.Id);
                                RemoveWeapon(weapon);
                                break;
                            case EquipKinds.Armor:
                                var armor = data.GetArmor(gi.Id);
                                RemoveArmor(armor);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    break;
                case ItemType.Book:
                    RemoveBook(data.GetBook(gi.Id));
                    break;
                case ItemType.AdvProps:
                    RemoveAdvItem(data.GetAdvProp(gi.Id));
                    break;
                case ItemType.FunctionItem:
                    RemoveFunctionItem(data.GetFunctionItem(gi.Id), amount);
                    break;
                case ItemType.StoryProps:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        internal void AddAdvItem(IGameItem item)
        {
            if (item == null) LogError("gameItem = null!");
            XDebug.LogError($"添加历练道具方法异常!: {item.Id}.{item.Name}");
            //AddGameItem(new Stacking<IGameItem>(item, 1));
            SendEvent(EventString.Faction_AdvItemsUpdate, string.Empty);
            Log($"添加历练道具: {item.Id}.{item.Name}");
        }
        internal void AddPackages(ICollection<IAdvPackage> packages)
        {
            if (packages == null) LogError("package = null!");
            _packages.AddRange(packages);
            SendEvent(EventString.Faction_AdvPackageUpdate, string.Empty);
            Log($"添加包裹x{packages.Count}");
        }

        internal void AddBook(IBook book)
        {
            if (book == null) LogError("book = null!");
            _books.Add(book);
            Log($"添加书籍: {book.Id}.{book.Name}");
            SendEvent(EventString.Faction_BookUpdate, string.Empty);
        }

        internal void RemoveAdvItem(IGameItem item)
        {
            if (item == null) LogError("item = null!");
            XDebug.LogError($"移除历练道具方法异常!: {item.Id}.{item.Name}");
            //RemoveGameItem(item, 1);//移除道具将直接执行gameItem移除
            SendEvent(EventString.Faction_AdvItemsUpdate, string.Empty);
            Log($"移除历练道具: {item.Id}.{item.Name}");
        }
        internal void RemovePackages(IAdvPackage package)
        {
            if (package == null) LogError("package = null!");
            _packages.Remove(package);
            SendEvent(EventString.Faction_AdvPackageUpdate, string.Empty);
            Log($"移除包裹: 品级【{package.Grade}】");
        }
        internal void RemoveBook(IBook book)
        {
            if (book == null) LogError("book = null!");
            _books.Remove(book);
            SendEvent(EventString.Faction_BookUpdate, string.Empty);
            Log($"移除书籍: {book.Id}.{book.Name}");
        }

        public int GetResource(ConsumeResources resourceType) =>
            resourceType switch
            {
                ConsumeResources.Food => Food,
                ConsumeResources.Wine => Wine,
                ConsumeResources.Herb => Herb,
                ConsumeResources.Pill => Pill,
                _ => throw new ArgumentOutOfRangeException(nameof(resourceType), resourceType, null)
            };

    }
}