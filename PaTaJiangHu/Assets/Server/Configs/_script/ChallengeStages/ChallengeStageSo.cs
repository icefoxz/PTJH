using System;
using _GameClient.Models;
using MyBox;
using Server.Configs.Adventures;
using Server.Configs.Battles;
using Server.Configs.Fields;
using Server.Configs.Items;
using UnityEngine;
using Dizi = Models.Dizi;

namespace Server.Configs.ChallengeStages
{
    public interface IChallengeStageNpc
    {
        IGameReward Reward{get;}
        IDiziReward DiziReward { get; }
        bool IsBoss { get; }
        string NpcName { get; }
        string Faction { get; }
        int Level { get; }
        DiziCombatUnit GetNpc();
    }

    public interface IDiziReward
    {
        int Exp { get; }
    }

    [CreateAssetMenu(fileName = "id_奖励件名", menuName = "挑战关卡/关卡")]
    internal class ChallengeStageSo : AutoAtNamingObject
    {
        [SerializeField] private int 回合限制 = 20;
        [SerializeField] private ChallengeField[] 挑战;
        private int RoundLimit => 回合限制;

        public IChallengeStageNpc[] Npcs => 挑战;

        public DiziBattle Instance(int challengeIndex, Dizi dizi)
        {
            var challenge = Npcs[challengeIndex];
            var diziCombat = new DiziCombatUnit(0, dizi);
            var npcCombat = challenge.GetNpc();
            return DiziBattle.Instance(new[] { diziCombat, npcCombat }, RoundLimit);
        }

        [Serializable] private class ChallengeField : IChallengeStageNpc
        {
            #region ChangeName
            private bool ChangeElementName()
            {
                _name = _npc != null ? (Boss ? "Boss: " : string.Empty) + _npc.Name : "未设置NPC!";
                return true;
            }
            #endregion
            [ConditionalField(true, nameof(ChangeElementName))]
            [SerializeField]
            [ReadOnly]
            private string _name;
            [SerializeField] private CombatNpcSo _npc;
            [SerializeField] private bool Boss;
            [SerializeField] private int 等级 = 1;
            [SerializeField] private string 所属势力;
            [SerializeField] private RewardField 奖励;
            [SerializeField] private DiziRewardField 弟子奖励;

            public IGameReward Reward => 奖励;
            public IDiziReward DiziReward => 弟子奖励;

            public bool IsBoss => Boss;
            public string NpcName => _npc.Name;
            public int Level => 等级;
            public string Faction => 所属势力;

            private CombatNpcSo Npc => _npc;
            public DiziCombatUnit GetNpc() => new DiziCombatUnit(1, Npc);

            [Serializable] private class DiziRewardField : IDiziReward
            {
                [SerializeField] private int 经验;

                public int Exp => 经验;
            }
        }
    }
}
