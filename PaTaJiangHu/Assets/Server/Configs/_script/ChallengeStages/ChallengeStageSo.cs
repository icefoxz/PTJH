using System;
using _GameClient.Models;
using MyBox;
using Server.Configs.Adventures;
using Server.Configs.Battles;
using Server.Configs.Fields;
using Server.Configs.Items;
using UnityEngine;

namespace Server.Configs.ChallengeStages
{
    public interface IChallengeStage
    {
        IGameReward Reward{get;}
        bool IsBoss { get; }
        DiziCombatUnit GetNpc();
    }

    [CreateAssetMenu(fileName = "id_奖励件名", menuName = "挑战关卡/关卡")]
    internal class ChallengeStageSo : AutoAtNamingObject
    {
        private bool GetItem()
        {
            if (So == null) So = this;
            return true;
        }
        [ConditionalField(true, nameof(GetItem))][ReadOnly][SerializeField] private ChallengeStageSo So;
        [SerializeField] private int 回合限制 = 20;
        [SerializeField] private ChallengeField[] 挑战;
        private int RoundLimit => 回合限制 = 20;

        public IChallengeStage[] Challenges => 挑战;

        public DiziBattle Challenge(int challengeIndex, Dizi dizi)
        {
            var challenge = Challenges[challengeIndex];
            var diziCombat = new DiziCombatUnit(0, dizi);
            var npcCombat = challenge.GetNpc();
            return DiziBattle.Start(diziCombat, npcCombat, RoundLimit);
        }

        [Serializable] private class ChallengeField : IChallengeStage
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

            public IGameReward Reward => 奖励;
            public bool IsBoss => Boss;
            public int Level => 等级;

            public string Faction => 所属势力;

            private CombatNpcSo Npc => _npc;
            public DiziCombatUnit GetNpc() => new DiziCombatUnit(1, Npc);
        }
    }
}
