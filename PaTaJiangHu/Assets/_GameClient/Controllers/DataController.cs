using System;
using System.Collections.Generic;
using System.Linq;
using AOT._AOT.Core;
using AOT._AOT.Utls;
using GameClient.Modules.BattleM;
using GameClient.Modules.DiziM;
using GameClient.SoScripts;
using GameClient.SoScripts.Items;
using GameClient.System;

namespace GameClient.Controllers
{
    /// <summary>
    /// 数据控制器,处理游戏模型配置的数据转化
    /// </summary>
    public class DataController : IGameController
    {
        private DataConfigSo Data => Game.Config.Data;

        public IMedicine GetMedicine(int id)
        {
            var item = GetFromData(id, Data.Medicines);
            return item;
        }
        public IWeapon GetWeapon(int id) => GetFromData(id, Data.Weapons).Instance();
        public IArmor GetArmor(int id) => GetFromData(id, Data.Armors).Instance();
        public IShoes GetShoes(int id) => GetFromData(id, Data.Shoes).Instance();
        public IDecoration GetDecoration(int id) => GetFromData(id, Data.Decorations).Instance();
        public IBook GetBook(int id) => GetFromData(id, Data.Books);
        public IAdvItem GetAdvProp(int id) => GetFromData(id, Data.AdvItems);
        public IFunctionItem GetFunctionItem(int id) => GetFromData(id, Data.FunctionItems);
        private T GetFromData<T>(int id, ICollection<T> list) where T : IGameItem
        {
            if (id == 0)
                throw new NotImplementedException($"物件{typeof(T).Name},Id = {id}, 请确保id > 0");
            var items = list.Where(o => o.Id == id).ToArray();
            if (!items.Any())
                XDebug.Log($"找不到物件<{typeof(T).Name}>.id = {id}, 请确保物件已经注册在配置里了.");
            if (items.Length > 1)
            {
                foreach (var item in items) 
                    XDebug.LogWarning($"物件【{item.Name}】Id 重复! Id = {item.Id}");
            }
            return items[0];
        }

    }

    /// <summary>
    /// 叠加游戏物件类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public record Stacking<T> : IStacking<T> where T : IGameItem
    {
        public T Item { get; }
        public int Amount { get; }
        public Stacking(T item, int amount)
        {
            Item = item;
            Amount = amount;
        }
    }
}