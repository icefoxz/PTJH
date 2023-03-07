using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Server.Configs.Battles;
using Server.Configs.Items;
using Utls;

namespace Server.Controllers
{
    /// <summary>
    /// 数据控制器,处理游戏模型配置的数据转化
    /// </summary>
    public class DataController : IGameController
    {
        private Config.DataCfg Data => Game.Config.Data;

        public IMedicine GetMedicine(int id)
        {
            var item = GetFromData(id, Data.Medicines);
            return item;
        }
        public IWeapon GetWeapon(int id) => GetFromData(id, Data.Weapons).Instance();
        public IArmor GetArmor(int id) => GetFromData(id, Data.Armors).Instance();
        public IBook GetBook(int id) => GetFromData(id, Data.Books);
        public IAdvItem GetAdvProp(int id) => GetFromData(id, Data.AdvItems);
        private T GetFromData<T>(int id, ICollection<T> list) where T : IGameItem
        {
            if (id == 0)
                throw new NotImplementedException($"物件{typeof(T).Name},Id = {id}, 请确保id > 0");
            var items = list.Where(o => o.Id == id).ToArray();
            if (!items.Any())
                XDebug.Log($"找不到物件<{typeof(T).Name}>.id = {id}");
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