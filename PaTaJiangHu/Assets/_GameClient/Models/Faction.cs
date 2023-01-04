using System.Collections.Generic;
using System.Linq;
using BattleM;
using Utls;

namespace _GameClient.Models
{
    /// <summary>
    /// 门派模型
    /// </summary>
    public class Faction
    {
        private Dictionary<string, Dizi> _diziMap;
        private List<IWeapon> _weapons= new List<IWeapon>();
        private List<IArmor> _armors = new List<IArmor>();
        public int Silver { get; private set; }
        public int YuanBao { get; private set; }
        public int ActionLing { get; private set; }
        /// <summary>
        /// key = dizi.Guid, value = dizi
        /// </summary>
        public IReadOnlyDictionary<string,Dizi> DiziMap => _diziMap;
        public IReadOnlyList<IWeapon> Weapons => _weapons;
        public IReadOnlyList<IArmor> Armors => _armors;

        internal Faction(int silver, int yuanBao, int actionLing, List<Dizi> diziMap)
        {
            _diziMap = diziMap.ToDictionary(d => d.Guid.ToString(), d => d);
            Silver = silver;
            YuanBao = yuanBao;
            ActionLing = actionLing;
        }

        internal void AddDizi(Dizi dizi)
        {
            _diziMap.Add(dizi.Guid, dizi);
            Log($"添加弟子{dizi.Name}");
            Game.MessagingManager.Send(EventString.Faction_DiziAdd, dizi);
            Game.MessagingManager.Send(EventString.Faction_DiziListUpdate, string.Empty);
        }

        internal void RemoveDizi(Dizi dizi)
        {
            Log($"移除弟子{dizi.Name}");
            _diziMap.Remove(dizi.Guid.ToString());
        }

        internal void AddSilver(int silver)
        {
            var last = Silver;
            Silver += silver;
            Log($"银两【{last}】增加了{silver},总:【{Silver}】");
            Game.MessagingManager.Send(EventString.Faction_SilverUpdate, Silver);
        }

        internal void AddYuanBao(int yuanBao)
        {
            var last = YuanBao;
            YuanBao += yuanBao;
            Log($"元宝【{last}】增加了{yuanBao},总:【{YuanBao}】");
            Game.MessagingManager.Send(EventString.Faction_YuanBaoUpdate, YuanBao);
        }

        internal void AddLing(int ling)
        {
            var last = ActionLing;
            ActionLing += ling;
            Log($"行动令【{last}】增加了{ling},总:【{ActionLing}】");
            Game.MessagingManager.SendParams(EventString.Faction_Params_ActionLingUpdate, ActionLing, 100, 0, 0);
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

        private void Log(string message) => XDebug.Log($"门派: {message}");

    }
}