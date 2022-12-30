using System.Collections.Generic;
using System.Linq;
using Utls;

namespace _GameClient.Models
{
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
            Game.MessagingManager.Send(EventString.Faction_DiziAdd, new DiziInfo(dizi));
            var list = DiziList.Select(d => new DiziInfo(d)).ToList();
            Game.MessagingManager.Send(EventString.Faction_DiziListUpdate, list);
        }

        public void RemoveDizi(Dizi dizi) => _diziList.Remove(dizi);

        public void AddSilver(int silver)
        {
            Silver += silver;
            Game.MessagingManager.Send(EventString.Faction_SilverUpdate, Silver);
        }

        public void AddYuanBao(int yuanBao)
        {
            YuanBao += yuanBao;
            Game.MessagingManager.Send(EventString.Faction_YuanBaoUpdate, YuanBao);
        }

        public void AddLing(int ling)
        {
            ActionLing += ling;
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
    }
    public class DiziInfo
    {
        public string Name { get; set; }

        public DiziInfo() { }
        public DiziInfo(Dizi d)
        {
            Name = d.Name;
        }
    }
}