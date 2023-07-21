using System;
using System.Collections.Generic;
using System.Linq;
using AOT.Core;
using AOT.Utls;
using GameClient.Controllers;
using GameClient.Modules.BattleM;
using GameClient.Modules.DiziM;
using GameClient.SoScripts;
using GameClient.SoScripts.Factions;
using GameClient.SoScripts.Items;
using GameClient.System;

namespace GameClient.Models
{
    public interface IFactionCommand
    {
    }


    /// <summary>
    /// 门派模型
    /// </summary>
    public partial class Faction : ModelBase, IFactionCommand
    {
        protected override string LogPrefix { get; } = "门派";
        private List<Dizi> _diziList = new();

        public int Level { get; private set; }
        public int ActionLing { get; private set; }
        public int ActionLingMax { get; }
        public int MaxDizi => DiziMap.Count;
        /// <summary>
        /// key = dizi.Guid, value = dizi
        /// </summary>
        private Dictionary<string,Dizi> DiziMap { get; }
        public IReadOnlyList<Dizi> DiziList => _diziList;

        private FactionConfigSo Config => Game.Config.FactionCfg;

        public Faction(int silver, int yuanBao, int actionLing,int actionLingMax, List<Dizi> diziMap,
            int food = 0, int wine = 0, int pill = 0, int herb = 0)
        {
            Level = 1;
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

        internal void Upgrade(int step = 1)
        {
            Level += step;
            SendEvent(EventString.Faction_Level_Update);
        }

        internal void SetLevel(int level)
        {
            Level = level;
            SendEvent(EventString.Faction_Level_Update);
        }

        internal bool TryAddDizi(Dizi dizi)
        {
            var limit = Config.GetDiziLimit(Level);
            if (DiziMap.Count >= limit)
            {
                Log($"弟子数量已达上限{limit}");
                SendEvent(EventString.Info_DiziAdd_LimitReached);
                return false;
            }
            DiziMap.Add(dizi.Guid, dizi);
            _diziList.Add(dizi);
            dizi.StartIdle(SysTime.UnixNow);
            Log($"添加弟子{dizi.Name}");
            SendEvent(EventString.Faction_DiziAdd, dizi.Guid);
            SendEvent(EventString.Faction_DiziListUpdate, string.Empty);
            return true;
        }
        internal void RemoveDizi(Dizi dizi)
        {
            Log($"移除弟子{dizi.Name}");
            DiziMap.Remove(dizi.Guid);
            _diziList.Remove(dizi);
            SendEvent(EventString.Faction_DiziListUpdate, string.Empty);
        }
        public Dizi GetDizi(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            if(DiziMap.TryGetValue(guid, out var dizi)) return dizi;
            LogWarning($"找不到弟子 = {guid}");
            return null;
        }
    }

    /// <summary>
    /// 门派资源的方法集
    /// </summary>
    public partial class Faction
    {
        public int Silver { get; private set; }
        public int YuanBao { get; private set; }
        public int Food { get; private set; }
        public int Wine { get; private set; }
        public int Pill { get; private set; }
        public int Herb { get; private set; }

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
        public void AddSilver(int silver)
        {
            if (silver == 0) return;
            var last = Silver;
            Silver += silver;
            Log($"银两【{last}】增加了{silver},总:【{Silver}】");
            SendEvent(EventString.Faction_SilverUpdate, Silver);
        }

        public void AddYuanBao(int yuanBao)
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

    /// <summary>
    /// 门派获取物品的方法集
    /// </summary>
    public partial class Faction
    {
        private Inventory _inventory = new();
        public IReadOnlyList<IWeapon> Weapons => _inventory.GetAllWeapons();
        public IReadOnlyList<IArmor> Armors => _inventory.GetAllArmors();
        public IReadOnlyList<IShoes> Shoes => _inventory.GetAllShoes();
        public IReadOnlyList<IDecoration> Decorations => _inventory.GetAllDecorations();
        public IReadOnlyList<IAdvPackage> Packages => _inventory.GetAllPackages();
        public IReadOnlyList<IBook> Books => _inventory.GetAllBooks();
        public IReadOnlyList<IFunctionItem> FuncItems => _inventory.GetAllFuncItems();
        public IReadOnlyList<IMedicine> GetAllMedicines() => _inventory.GetAllMedicines();
        public IReadOnlyList<IGameItem> GetAllSupportedAdvItems() => _inventory.GetAllSupportedAdvItems();
        public IBook GetBook(int bookId) => Books.FirstOrDefault(b => b.Id == bookId);
        public IComprehendItem[] GetAllComprehendItems(int supportedLevel)
        {
            var items = FuncItems
                .Where(f => f.FunctionType == FunctionItemType.Comprehend)
                .Cast<IComprehendItem>()
                .Where(f => f.IsLevelAvailable(supportedLevel))
                .ToArray();
            return items;
        }
        public IBook[] GetBooksForSkill(SkillType type) => _inventory.GetBooksForSkill(type);
        /// <summary>
        /// 给门派加物件
        /// </summary>
        /// <param name="gi"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal void AddGameItem(IGameItem gi)
        {
            var data = Game.Controllers.Get<DataController>();
            switch (gi.Type)
            {
                case ItemType.Medicine:
                    AddMedicine(data.GetMedicine(gi.Id));
                    break;
                case ItemType.Equipment:
                    var equipment = gi as IEquipment;
                    if (equipment == null) XDebug.LogError($"物件{gi.Id}.{gi.Name} 未继承<IEquipment>");
                    switch (equipment.EquipKind)
                    {
                        case EquipKinds.Weapon:
                            var weapon = data.GetWeapon(gi.Id);
                            AddWeapon(weapon);
                            break;
                        case EquipKinds.Armor:
                            var armor = data.GetArmor(gi.Id);
                            AddArmor(armor);
                            break;
                        case EquipKinds.Shoes:
                            var shoes = data.GetShoes(gi.Id);
                            AddShoes(shoes);
                            break;
                        case EquipKinds.Decoration:
                            var decoration = data.GetDecoration(gi.Id);
                            AddDecoration(decoration);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                case ItemType.Book:
                    AddBook(data.GetBook(gi.Id));
                    break;
                case ItemType.AdvProps:
                    AddAdvItem(data.GetAdvProp(gi.Id));
                    break;
                case ItemType.FunctionItem:
                    AddFunctionItem(data.GetFunctionItem(gi.Id));
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
                            case EquipKinds.Shoes:
                            case EquipKinds.Decoration:
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
                    RemoveFunctionItem(data.GetFunctionItem(gi.Id));
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
            _inventory.AddRange(packages);
            SendEvent(EventString.Faction_AdvPackageUpdate, string.Empty);
            Log($"添加包裹x{packages.Count}");
        }

        internal void AddBook(IBook book)
        {
            if (book == null) LogError("book = null!");
            _inventory.AddItem(book);
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
            _inventory.RemoveItem(package);
            SendEvent(EventString.Faction_AdvPackageUpdate, string.Empty);
            Log($"移除包裹: 品级【{package.Grade}】");
        }
        internal void RemoveBook(IBook book)
        {
            if (book == null) LogError("book = null!");
            _inventory.RemoveItem(book);
            SendEvent(EventString.Faction_BookUpdate, string.Empty);
            Log($"移除书籍: {book.Id}.{book.Name}");
        }

        internal void AddWeapon(IWeapon weapon)
        {
            if(weapon == null) LogError("Weapon = null!");
            _inventory.AddItem(weapon);
            Log($"添加武器【{weapon.Name}】");
        }
        internal void RemoveWeapon(IWeapon weapon)
        {
            if(weapon == null) LogError("Weapon = null!");
            _inventory.RemoveItem(weapon);
            Log($"移除武器【{weapon.Name}】");
        }
        internal void AddArmor(IArmor armor)
        {
            if(armor == null) LogError("armor = null!");
            _inventory.AddItem(armor);
            Log($"添加防具【{armor.Name}】");
        }
        internal void RemoveArmor(IArmor armor)
        {
            if(armor == null) LogError("armor = null!");
            _inventory.RemoveItem(armor);
            Log($"移除防具【{armor.Name}】");
        }
        internal void AddShoes(IShoes shoes)
        {
            if(shoes == null) LogError("shoes = null!");
            _inventory.AddItem(shoes);
            Log($"添加鞋子【{shoes.Name}】");
        }
        internal void RemoveShoes(IShoes shoes)
        {
            if(shoes == null) LogError("shoes = null!");
            _inventory.RemoveItem(shoes);
            Log($"移除鞋子【{shoes.Name}】");
        }
        internal void AddDecoration(IDecoration decoration)
        {
            if(decoration == null) LogError("decoration = null!");
            _inventory.AddItem(decoration);
            Log($"添加饰品【{decoration.Name}】");
        }
        internal void RemoveDecoration(IDecoration decoration)
        {
            if(decoration == null) LogError("decoration = null!");
            _inventory.RemoveItem(decoration);
            Log($"移除饰品【{decoration.Name}】");
        }

        internal void AddMedicine(IMedicine med)
        {
            if(med == null) LogError("med = null!");
            _inventory.AddItem(med);
            Log($"添加药品【{med.Name}】");
        }

        internal void RemoveMedicine(IMedicine med,int amount)
        {
            if (med == null) LogError("med = null!");
            _inventory.AddItem(med);
            if(amount > 0) Log($"移除药品【{med.Name}】");
        }

        internal void AddFunctionItem(IFunctionItem item)
        {
            if (item == null) LogError("functionItem = null!");
            _inventory.AddItem(item);
            SendEvent(EventString.Faction_FunctionItemUpdate, string.Empty);
            Log($"添加功能道具: {item.Id}.{item.Name}");
        }

        internal void RemoveFunctionItem(IFunctionItem item)
        {
            if (item == null) LogError("item = null!");
            _inventory.AddItem(item);
            SendEvent(EventString.Faction_FunctionItemUpdate, string.Empty);
            Log($"移除功能道具: {item.Id}.{item.Name}");
        }
    }

    /// <summary>
    /// 门派挑战相关方法集
    /// </summary>
    public partial class Faction
    {
        internal ChallengeStage Challenge { get; set; } = new ChallengeStage();
        public bool IsChallenging => Challenge is { IsFinish: false, Stage: not null };
        public int ChallengeStageProgress => Challenge?.Progress ?? -1;
        public int ChallengeLevel => Challenge.Level;
        public ICollection<IGameChest> ChallengeChests => Challenge?.Chests ?? Array.Empty<IGameChest>();
        public IChallengeStage GetChallengeStage() => Challenge?.Stage;
        public class ChallengeStage
        {
            private List<IGameChest> _chests = new List<IGameChest>();
            public IChallengeStage Stage { get; private set; }
            public int Progress { get; private set; }
            public int Level { get; private set; }
            public int PassCount { get; set; }
            public int AbandonCount { get; set; }
            public ICollection<IGameChest> Chests => _chests;
            public bool IsFinish => Progress == Stage?.StageCount;

            internal void SetProgress(int progress) => Progress = progress;
            internal void AddChest(IGameChest chest) => Chests.Add(chest);
            internal void RemoveChest(IGameChest chest) => Chests.Remove(chest);
            internal void RemoveStage()
            {
                Stage = null;
                Progress = 0;
            }
            internal void SetStage(IChallengeStage stage)
            {
                Stage = stage;
                Progress = 0;
            }
            internal void SetLevel(int level) => Level = level;
            internal void SetAbandonCount(int count) => AbandonCount = count;
            internal void SetPassCount(int count) => PassCount = count;

            public void LevelDown()
            {
                ResetLevelProgress();
                Level = Math.Max(Level, Level - 1);
            }
            public void LevelUp()
            {
                ResetLevelProgress();
                Level++;
            }

            private void ResetLevelProgress()
            {
                SetAbandonCount(0);
                SetPassCount(0);
            }
        }

        internal void SetChallenge(IChallengeStage stage)
        {
            Challenge.SetStage(stage);
            SendEvent(EventString.Faction_Challenge_Update);
        }

        internal void RemoveChallenge()
        {
            Challenge.RemoveStage();
            SendEvent(EventString.Faction_Challenge_Update);
        }

        internal void AddChest(IGameChest chest)
        {
            Challenge.AddChest(chest);
            SendEvent(EventString.Faction_Challenge_Update);
        }

        internal void RemoveChest(IGameChest chest)
        {
            Challenge.RemoveChest(chest);
            SendEvent(EventString.Faction_Challenge_Update);
        }

        internal void LevelDown()
        {
            Challenge.LevelDown();
            SendEvent(EventString.Faction_Challenge_Update);
        }

        internal void LevelUp()
        {
            Challenge.LevelUp();
            SendEvent(EventString.Faction_Challenge_Update);
        }

        internal void NextChallengeProgress()
        {
            Challenge.SetProgress(Challenge.Progress + 1);
            SendEvent(EventString.Faction_Challenge_Update);
        }

        internal void SetAbandonCount(int count)
        {
            Challenge.SetAbandonCount(count);
            SendEvent(EventString.Faction_Challenge_Update);
        }

        internal void SetPassCount(int count)
        {
            Challenge.SetPassCount(count);
            SendEvent(EventString.Faction_Challenge_Update);
        }
    }

    public class Inventory
    {
        private ItemList<IWeapon> Weapons { get; } = new ItemList<IWeapon>();
        private ItemList<IArmor> Armors { get; } = new ItemList<IArmor>();
        private ItemList<IShoes> Shoes { get; } = new ItemList<IShoes>();
        private ItemList<IDecoration> Decorations { get; } = new ItemList<IDecoration>();
        private ItemList<IAdvPackage> Packages { get; } = new ItemList<IAdvPackage>();
        private ItemList<IBook> Books { get; } = new ItemList<IBook>();
        private ItemList<IGameItem> AdvItems { get; } = new ItemList<IGameItem>();
        private ItemList<IMedicine> Medicines { get; } = new ItemList<IMedicine>();
        private ItemList<IFunctionItem> FuncItems { get; } = new ItemList<IFunctionItem>();

        public void AddItem<T>(T item) where T : IGameItem
        {
            switch (item)
            {
                case IWeapon weapon:
                    Weapons.Add(weapon);
                    break;
                case IArmor armor:
                    Armors.Add(armor);
                    break;
                case IShoes shoes:
                    Shoes.Add(shoes);
                    break;
                case IDecoration decoration:
                    Decorations.Add(decoration);
                    break;
                case IBook book:
                    Books.Add(book);
                    break;
                case IFunctionItem functionItem:
                    FuncItems.Add(functionItem);
                    break;
                case IMedicine medicine:
                    Medicines.Add(medicine);
                    break;
                case IGameItem advItem:
                    AdvItems.Add(advItem);
                    break;
                default:
                    throw new Exception($"Unsupported item type: {item.GetType().Name}");
            }
        }
        public void AddItem(IAdvPackage item) => Packages.Add(item);
        public void AddRange(IEnumerable<IAdvPackage> packages) => Packages.AddRange(packages);
        public bool RemoveItem<T>(T item) where T : IGameItem
        {
            switch (item)
            {
                case IWeapon weapon:
                    return Weapons.Remove(weapon);
                case IArmor armor:
                    return Armors.Remove(armor);
                case IShoes shoes:
                    return Shoes.Remove(shoes);
                case IDecoration decoration:
                    return Decorations.Remove(decoration);
                case IBook book:
                    return Books.Remove(book);
                case IFunctionItem functionItem:
                    return FuncItems.Remove(functionItem);
                case IMedicine medicine:
                    return Medicines.Remove(medicine);
                case IGameItem advItem:
                    return AdvItems.Remove(advItem);
                default:
                    throw new Exception($"Unsupported item type: {item.GetType().Name}");
            }
        }

        public bool RemoveItem(IAdvPackage item) => Packages.Remove(item);
        public IReadOnlyList<IWeapon> GetAllWeapons() => Weapons.GetAllItems().ToList();
        public IReadOnlyList<IArmor> GetAllArmors() => Armors.GetAllItems().ToList();
        public IReadOnlyList<IShoes> GetAllShoes() => Shoes.GetAllItems().ToList();
        public IReadOnlyList<IDecoration> GetAllDecorations() => Decorations.GetAllItems().ToList();
        public IReadOnlyList<IAdvPackage> GetAllPackages() => Packages.GetAllItems().ToList();
        public IReadOnlyList<IBook> GetAllBooks() => Books.GetAllItems().ToList();
        public IReadOnlyList<IFunctionItem> GetAllFuncItems() => FuncItems.GetAllItems().ToList();
        public IReadOnlyList<IMedicine> GetAllMedicines() => Medicines.GetAllItems().ToList();
        public IReadOnlyList<IGameItem> GetAllAdvItems() => AdvItems.GetAllItems().ToList();
        public IReadOnlyList<IGameItem> GetAllSupportedAdvItems()
        {
            return Medicines.Where(m => m.Kind == MedicineKinds.StaminaDrug)
                //.Concat(AdvProps.Select(p => new Stacking<IGameItem>(p, 1)))
                .ToArray();
        }
        public IBook[] GetBooksForSkill(SkillType type)=> Books.Where(x => x.GetSkill().SkillType == type).ToArray();
    }
}