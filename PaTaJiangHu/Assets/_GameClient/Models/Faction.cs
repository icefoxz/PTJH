﻿using System;
using System.Collections.Generic;
using System.Linq;
using BattleM;
using Core;
using Server.Configs.Items;
using Utls;

namespace _GameClient.Models
{
    public interface IFaction
    {
        int Silver { get; }
        int YuanBao { get; }
        int ActionLing { get; }
        IReadOnlyList<IWeapon> Weapons { get; }
        IReadOnlyList<IArmor> Armors { get; }
        ICollection<Dizi> DiziList { get; }
        Dizi GetDizi(string guid);
    }

    /// <summary>
    /// 门派模型
    /// </summary>
    public class Faction : ModelBase, IFaction
    {
        protected override string LogPrefix { get; } = "门派";

        private List<IWeapon> _weapons= new List<IWeapon>();
        private List<IArmor> _armors = new List<IArmor>();
        private Dictionary<IMedicine,int> Medicines { get; } = new Dictionary<IMedicine,int>();
        public int Silver { get; private set; }
        public int YuanBao { get; private set; }
        public int ActionLing { get; private set; }
        /// <summary>
        /// key = dizi.Guid, value = dizi
        /// </summary>
        private Dictionary<string,Dizi> DiziMap { get; }
        public IReadOnlyList<IWeapon> Weapons => _weapons;
        public IReadOnlyList<IArmor> Armors => _armors;
        public ICollection<Dizi> DiziList => DiziMap.Values;

        public (IMedicine med, int amount)[] GetAllMedicines() => Medicines.Select(m => (m.Key, m.Value)).ToArray();

        internal Faction(int silver, int yuanBao, int actionLing, List<Dizi> diziMap)
        {
            DiziMap = diziMap.ToDictionary(d => d.Guid.ToString(), d => d);
            Silver = silver;
            YuanBao = yuanBao;
            ActionLing = actionLing;
        }

        internal void AddDizi(Dizi dizi)
        {
            DiziMap.Add(dizi.Guid, dizi);
            Log($"添加弟子{dizi.Name}");
            SendEvent(EventString.Faction_DiziAdd, dizi.Guid);
            SendEvent(EventString.Faction_DiziListUpdate, string.Empty);
        }

        internal void RemoveDizi(Dizi dizi)
        {
            Log($"移除弟子{dizi.Name}");
            DiziMap.Remove(dizi.Guid);
        }

        internal void AddSilver(int silver)
        {
            var last = Silver;
            Silver += silver;
            Log($"银两【{last}】增加了{silver},总:【{Silver}】");
            SendEvent(EventString.Faction_SilverUpdate, Silver);
        }

        internal void AddYuanBao(int yuanBao)
        {
            var last = YuanBao;
            YuanBao += yuanBao;
            Log($"元宝【{last}】增加了{yuanBao},总:【{YuanBao}】");
            SendEvent(EventString.Faction_YuanBaoUpdate, YuanBao);
        }

        internal void AddLing(int ling)
        {
            var last = ActionLing;
            ActionLing += ling;
            Log($"行动令【{last}】增加了{ling},总:【{ActionLing}】");
            SendEvent(EventString.Faction_Params_ActionLingUpdate, ActionLing, 100, 0, 0);
        }

        public class Dto
        {
            public int Silver { get; set; }
            public int YuanBao { get; set; }
            public int ActionLing { get; set; }
            public int ActionLingMax { get; set; }

            public Dto() { }

            public Dto(Faction f)
            {
                Silver = f.Silver;
                YuanBao = f.YuanBao;
                ActionLing = f.ActionLing;
                ActionLingMax = 100;
            }
            public Dto(int silver, int yuanBao, int actionLing, int actionLingMax)
            {
                Silver = silver;
                YuanBao = yuanBao;
                ActionLing = actionLing;
                ActionLingMax = actionLingMax;
            }
        }

        internal void AddWeapon(IWeapon weapon)
        {
            _weapons.Add(weapon);
            Log($"添加武器【{weapon.Name}】");
        }
        internal void RemoveWeapon(IWeapon weapon)
        {
            _weapons.Remove(weapon);
            Log($"移除武器【{weapon.Name}】");
        }

        internal void AddArmor(IArmor armor)
        {
            _armors.Add(armor);
            Log($"添加防具【{armor.Name}】");
        }

        internal void RemoveArmor(IArmor armor)
        {
            _armors.Remove(armor);
            Log($"移除防具【{armor.Name}】");
        }

        internal void AddMedicine(IMedicine med, int amount)
        {
            if (!Medicines.ContainsKey(med))
                Medicines.Add(med, 0);
            Medicines[med] += amount;
            Log($"添加药品【{med.Name}】x{amount}, 总: {Medicines[med]}");
        }

        internal void RemoveMedicine(IMedicine med,int amount)
        {
            if (!Medicines.ContainsKey(med))
                LogError($"找不到{med.Name},Id = {med.Id}");
            if (Medicines[med] < amount) LogError($"{med.Name}:{Medicines[med]} < {amount}! ");
            Medicines[med] -= amount;
            Log($"移除药品【{med.Name}】");
        }

        public Dizi GetDizi(string guid)
        {
            if(DiziMap.TryGetValue(guid, out var dizi)) return dizi;
            LogWarning($"找不到弟子 = {guid}");
            return null;
        }
    }
}