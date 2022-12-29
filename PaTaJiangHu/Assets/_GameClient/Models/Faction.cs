using System.Collections.Generic;
using System.Linq;
using Server.Controllers.Adventures;
using Utls;
using static Server.Controllers.Factions.RecruitController;

namespace _GameClient.Models
{
    public interface IDiziInfo
    {
        string Name { get; }
        int Strength { get; }
        int Agility { get; }
        int Hp { get; }
        int MaxHp { get; }
        int Mp { get; }
        int MaxMp { get; }
        int Level { get; }
        /// <summary>
        /// 品级
        /// </summary>
        int Grade { get; }
        /// <summary>
        /// 轻功格
        /// </summary>
        int DodgeSlot { get; }
        /// <summary>
        /// 武功格
        /// </summary>
        int CombatSlot { get; }
        /// <summary>
        /// 背包格
        /// </summary>
        int BagSlot { get; }
        /// <summary>
        /// 当前体力
        /// </summary>
        int Stamina { get; set; }
        /// <summary>
        /// 最大体力
        /// </summary>
        int StaminaMax { get; set; }
        /// <summary>
        /// 上次体力更新
        /// </summary>
        long StaminaUpdate { get; set; }
    }

    /// <summary>
    /// 门派模型
    /// </summary>
    public class Faction
    {
        private List<Dizi> _diziList;
        public int Silver { get; private set; }
        public int YuanBao { get; private set; }
        public int ActionLing { get; private set; }

        public IReadOnlyList<Dizi> DiziList => _diziList;

        public Faction(int silver, int yuanBao, int actionLing, List<Dizi> diziList)
        {
            _diziList = diziList;
            Silver = silver;
            YuanBao = yuanBao;
            ActionLing = actionLing;
        }

        public void AddDizi(Dizi dizi)
        {
            _diziList.Add(dizi);
            Game.MessagingManager.Invoke(EventString.Faction_DiziAdd, new DiziInfo(dizi));
            var list = DiziList.Select(d => new DiziInfo(d)).ToList();
            Game.MessagingManager.Invoke(EventString.Faction_DiziListUpdate, ObjectBag.Serialize(list));
        }

        public void RemoveDizi(Dizi dizi) => _diziList.Remove(dizi);

        public void AddSilver(int silver) => Silver += silver;
        public void AddYuanBao(int yuanBao) => YuanBao += yuanBao;
        public void AddLing(int ling)=> ActionLing += ling;

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
    }
}