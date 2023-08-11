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

        public Faction()
        {
            Level = 1;
            DiziMap = new Dictionary<string, Dizi>();
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
            SendEvent(EventString.Faction_ActionLing_Update, ActionLing, 100, 0, 0);
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
        private Inventory _inventory = new(Game.Config.FactionCfg.InventoryLimit);

        public IReadOnlyList<IEquipment> Equipments => _inventory.GetAllEquipments();
        public IReadOnlyList<IItemPackage> Packages => _inventory.GetAllPackages();
        public IReadOnlyList<IFunctionItem> FuncItems => _inventory.GetAllFuncItems();
        public IReadOnlyList<IBook> Books => _inventory.GetAllBooks();
        public IReadOnlyList<IGameItem> TempItems => _inventory.GetAllTempItems();

        public IReadOnlyList<IWeapon> GetWeapons() => Equipments.Where(i => i.EquipKind == EquipKinds.Weapon).Cast<IWeapon>().ToArray();
        public IReadOnlyList<IArmor> GetArmors() => Equipments.Where(i => i.EquipKind == EquipKinds.Armor).Cast<IArmor>().ToArray();
        public IReadOnlyList<IShoes> GetShoes() => Equipments.Where(i => i.EquipKind == EquipKinds.Shoes).Cast<IShoes>().ToArray();
        public IReadOnlyList<IDecoration> GetDecorations() => Equipments.Where(i => i.EquipKind == EquipKinds.Decoration).Cast<IDecoration>().ToArray();
        public IReadOnlyList<IAdvItem> GetAdventureItems() => FuncItems.Where(f => f.FunctionType == FunctionItemType.AdvItem).Cast<IAdvItem>().ToArray();
        public IReadOnlyList<IMedicine> GetAllMedicines() => FuncItems.Where(f => f.FunctionType == FunctionItemType.Medicine).Cast<IMedicine>().ToArray();
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

        // 给门派加物件
        internal void AddGameItem(IGameItem gi, bool newInstance = true)
        {
            var instance = gi;
            if (newInstance)
            {
                instance = GetInstance(gi);
            }
            _inventory.AddItem(gi, out var isPutToTemp);
            switch (instance.Type)
            {
                case ItemType.Equipment:
                    Log($"添加装备【 {instance.Id}.{instance.Name}】");
                    SendEvent(EventString.Faction_Equipment_Update);
                    break;
                case ItemType.Book:
                    Log($"添加书籍: {instance.Id}.{instance.Name}");
                    SendEvent(EventString.Faction_Book_Update);
                    break;
                case ItemType.FunctionItem:
                    Log($"添加功能道具: {instance.Id} . {instance.Name}");
                    SendEvent(EventString.Faction_FunctionItem_Update);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (isPutToTemp) SendEvent(EventString.Faction_TempItem_Update);

            IGameItem GetInstance(IGameItem gameItem)
            {
                var data = Game.Controllers.Get<DataController>();
                switch (gameItem.Type)
                {
                    case ItemType.Equipment:
                    {
                        if (gameItem is not IEquipment equipment)
                            throw new NullReferenceException($"物件{gameItem.Id}.{gameItem.Name} 未继承<IEquipment>");
                        IEquipment e = equipment.EquipKind switch
                        {
                            EquipKinds.Weapon => data.GetWeapon(gameItem.Id),
                            EquipKinds.Armor => data.GetArmor(gameItem.Id),
                            EquipKinds.Shoes => data.GetShoes(gameItem.Id),
                            EquipKinds.Decoration => data.GetDecoration(gameItem.Id),
                            _ => throw new ArgumentOutOfRangeException()
                        };
                        return e;
                    }
                    case ItemType.Book:
                    {
                        var book = data.GetBook(gameItem.Id);
                        return book;
                    }
                    case ItemType.FunctionItem:
                    {
                        if (gameItem is not IFunctionItem funcItem)
                            throw new NullReferenceException($"物件{gameItem.Id}.{gameItem.Name} 未继承<IFunctionItem>");
                        var item = funcItem.FunctionType switch
                        {
                            FunctionItemType.AdvItem => data.GetAdvItem(gameItem.Id),
                            FunctionItemType.Comprehend => data.GetComprehendItem(gameItem.Id),
                            FunctionItemType.Medicine => data.GetMedicine(gameItem.Id),
                            FunctionItemType.StoryProps => data.GetStoryItem(gameItem.Id),
                            _ => throw new ArgumentOutOfRangeException()
                        };
                        return item;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// 给门派移除物件
        /// </summary>
        internal void RemoveGameItem(IGameItem gi)
        {
            _inventory.RemoveItem(gi, out var isTempItemReplace);
            Log($"移除【{gi.Name}】");
            switch (gi.Type)
            {
                case ItemType.Equipment:
                    SendEvent(EventString.Faction_Equipment_Update);
                    break;
                case ItemType.Book:
                    SendEvent(EventString.Faction_Book_Update);
                    break;
                case ItemType.FunctionItem:
                    SendEvent(EventString.Faction_FunctionItem_Update);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (isTempItemReplace) SendEvent(EventString.Faction_TempItem_Update);
        }

        internal void AddPackages(ICollection<IItemPackage> packages)
        {
            if (packages == null) LogError("package = null!");
            _inventory.AddRange(packages);
            SendEvent(EventString.Faction_Package_Update, string.Empty);
            Log($"添加包裹x{packages.Count}");
        }
        
        internal void RemovePackages(IItemPackage package)
        {
            if (package == null) LogError("package = null!");
            _inventory.RemoveItem(package);
            SendEvent(EventString.Faction_Package_Update, string.Empty);
            Log($"移除包裹: 品级【{package.Grade}】");
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
}