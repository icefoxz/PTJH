using System;
using AOT._AOT.Core;
using AOT._AOT.Utls;
using GameClient.Models;
using GameClient.SoScripts.Adventures;
using GameClient.SoScripts.Factions;
using GameClient.System;

namespace GameClient.Controllers
{
    /// <summary>
    /// 门派控制器
    /// </summary>
    public class FactionController : IGameController
    {
        private Faction Faction => Game.World.Faction;

        public void ConsumeResourceByStep(string diziGuid, IAdjustment.Types con)
        {
            var dizi = Faction.GetDizi(diziGuid);
            int stock;
            int consume;
            var resourceType = con switch
            {
                IAdjustment.Types.Food => ConsumeResources.Food,
                IAdjustment.Types.Emotion => ConsumeResources.Wine,
                IAdjustment.Types.Injury => ConsumeResources.Herb,
                IAdjustment.Types.Inner => ConsumeResources.Pill,
                _ => throw new ArgumentOutOfRangeException(nameof(con), con, $"{con}不可使用此方式!")
            };
            consume = dizi.Capable.GetConsume(resourceType);
            stock = Faction.GetResource(resourceType);

            if (consume > stock)
            {
                XDebug.Log($"门派资源: {resourceType}不足! {dizi.Name} 消耗量={consume}, 当前库存={stock}!");
                Game.MessagingManager.SendParams(EventString.Win_PopUp, dizi.Guid, "门派资源不足以消费!", stock, consume);
                return;
            }

            Faction.AddConsumeResource(resourceType, -consume);
            dizi.ConAdd(con, 1);
        }

        public void OpenAdvPackage(int index)
        {
            var package = Faction.Packages[index];
            Faction.RemovePackages(package);
            var res = package.Package;
            foreach (var item in package.AllItems) 
                Faction.AddGameItem(item);
            Faction.AddYuanBao(res.YuanBao);
            Faction.AddSilver(res.Silver);
            Faction.AddConsumeResource(ConsumeResources.Food,res.Food);
            Faction.AddConsumeResource(ConsumeResources.Wine, res.Wine);
            Faction.AddConsumeResource(ConsumeResources.Herb, res.Herb);
            Faction.AddConsumeResource(ConsumeResources.Pill, res.Pill);
        }
    }
}