using System.Net.Sockets;
using AOT.Core;
using AOT.Core.Dizi;
using GameClient.Models;
using GameClient.Modules.Adventure;
using GameClient.SoScripts;
using GameClient.System;

namespace GameClient.Controllers
{
    public class DiziLostController : IGameController
    {
        private GameWorld.DiziState State => Game.World.State;
        private Lost_ActivityManager Lost => State.Lost;
        private Faction Faction => Game.World.Faction;
        private Config.Recruit RecruitCfg => Game.Config.RecruitCfg;

        public void LostStart(string guid, DiziActivityLog log)
        {
            var dizi = State.RemoveStateless(guid);
            Lost.ActivityStart(dizi, log);
        }

        private void SendEvent(string eventName, params object[] args) => Game.MessagingManager.SendParams(eventName, args);
        private bool FactionTryTrade(int silver, int yuanBao)
        {
            if (Faction.Silver < silver)
            {
                SendEvent(EventString.Info_Trade_Failed_Silver, silver);
                return false;
            }

            if (Faction.YuanBao < yuanBao)
            {
                SendEvent(EventString.Info_Trade_Failed_YuanBao, silver);
                return false;
            }

            Faction.AddYuanBao(-yuanBao);
            Faction.AddSilver(-silver);
            return true;
        }
        public void RestoreDizi(string guid)
        {
            var dizi = Lost.GetDizi(guid);
            //var restoreCost = RecruitCfg.GradeCfg.GetRestoreCost((ColorGrade)dizi.Capable.Grade);
            //if (FactionTryTrade(0, -restoreCost))
            {
                Lost.RestoreDizi(guid);
            }
            State.AddStateless(dizi);
        }
    }
}