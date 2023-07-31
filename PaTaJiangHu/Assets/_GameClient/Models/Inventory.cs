using System;
using System.Collections.Generic;
using System.Linq;
using AOT.Core;
using GameClient.Modules.BattleM;
using GameClient.Modules.DiziM;

namespace GameClient.Models
{
    public class Inventory
    {
        private ItemList<IItemPackage> Packages { get; } = new ItemList<IItemPackage>();
        private ItemList<IEquipment> Equipments { get; } = new ItemList<IEquipment>();
        private ItemList<IBook> Books { get; } = new ItemList<IBook>();
        private ItemList<IFunctionItem> FuncItems { get; } = new ItemList<IFunctionItem>();
        private ItemList<IGameItem> TempItems { get; } = new ItemList<IGameItem>();
        private Dictionary<ItemType,int> InventoryLimit { get; } 

        public Inventory(Dictionary<ItemType, int> inventoryLimit)
        {
            InventoryLimit = inventoryLimit;
        }

        public void AddItem<T>(T item,out bool isPutToTemp) where T : IGameItem
        {
            if (!InventoryLimit.TryGetValue(item.Type, out var limit))
                throw new KeyNotFoundException($"找不到{item.Name}({item.Type})类型的数量限制!");
            isPutToTemp = limit > 0 && GetItemCount(item) >= limit;
            if (isPutToTemp)
            {
                TempItems.Add(item);
                return;
            }

            switch (item)
            {
                case IEquipment equipment:
                    Equipments.Add(equipment);
                    break;
                case IBook book:
                    Books.Add(book);
                    break;
                case IFunctionItem functionItem:
                    FuncItems.Add(functionItem);
                    break;
                default:
                    throw new Exception($"Unsupported item type: {item.GetType().Name}");
            }

            int GetItemCount(IGameItem gi)
            {
                return gi.Type switch
                {
                    ItemType.Equipment => Equipments.Count,
                    ItemType.Book => Books.Count,
                    ItemType.FunctionItem => FuncItems.Count,
                    _ => throw new Exception($"Unsupported item type: {gi.Type}")
                };
            }
        }
        public void AddItem(IItemPackage item) => Packages.Add(item);
        public void AddRange(IEnumerable<IItemPackage> packages) => Packages.AddRange(packages);

        public void RemoveItem<T>(T item, out bool isTempItemReplace) where T : IGameItem
        {
            switch (item.Type)
            {
                case ItemType.Equipment:
                    Equipments.Remove((IEquipment)item);
                    break;
                case ItemType.Book:
                    Books.Remove((IBook)item);
                    break;
                case ItemType.FunctionItem:
                    FuncItems.Remove((IFunctionItem)item);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var temp = TempItems.FirstOrDefault(i => i.Type == item.Type);
            isTempItemReplace = temp != null;
            if (!isTempItemReplace) return;
            // 如果还有同类物件，用第一个暂存物品存入物品栏
            AddItem(temp, out var isPutToTemp);
            if (isPutToTemp)
                throw new NotImplementedException($"暂存物件{temp?.Name}({item.Type})提取了, 但又放回暂存栏里.");
            TempItems.Remove(temp);
        }

        public bool RemoveItem(IItemPackage item) => Packages.Remove(item);
        public IReadOnlyList<IItemPackage> GetAllPackages() => Packages;
        public IReadOnlyList<IBook> GetAllBooks() => Books;
        public IReadOnlyList<IFunctionItem> GetAllFuncItems() => FuncItems;
        public IReadOnlyList<IGameItem> GetAllTempItems() => TempItems;
        public IBook[] GetBooksForSkill(SkillType type)=> Books.Where(x => x.GetSkill().SkillType == type).ToArray();
        public IReadOnlyList<IEquipment> GetAllEquipments() => Equipments;

        public void RemoveTemp(IGameItem item) => TempItems.Remove(item);
    }
}