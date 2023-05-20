using System;
using Models;
using MyBox;
using Server.Configs.Adventures;
using Server.Configs.Battles;
using Server.Configs.Fields;
using Server.Configs.Items;
using UnityEngine;
using UnityEngine.Serialization;

namespace Server.Configs.ChallengeStages
{
    public interface ISingleStageNpc : IStageNpc
    {
        string Faction { get; }
        IGameReward Reward { get; }
    }
    
    public interface IStageNpc
    {
        bool IsBoss { get; }
        string NpcName { get; }
        int Level { get; }
        DiziCombatUnit GetNpc();
        IDiziReward DiziReward { get; }
    }
    //对单个弟子的奖励
    public interface IDiziReward
    {
        int Exp { get; }
    }

    [CreateAssetMenu(fileName = "id_关卡", menuName = "关卡/关卡")]
    internal class GameStageSo : AutoAtNamingObject
    {
        [SerializeField] private int 回合限制 = 20;

        [FormerlySerializedAs("挑战")] [SerializeField]
        private StageNpc[] 关卡;

        private int RoundLimit => 回合限制;

        public ISingleStageNpc[] Npcs => 关卡;

        public DiziBattle InstanceBattle(int challengeIndex, Dizi dizi)
        {
            var challenge = Npcs[challengeIndex];
            var diziCombat = new DiziCombatUnit(0, dizi);
            var npcCombat = challenge.GetNpc();
            return DiziBattle.Instance(new[] { diziCombat, npcCombat }, RoundLimit);
        }

        [Serializable]
        internal class StageNpc : ISingleStageNpc
        {
            #region ChangeName

            private bool ChangeElementName()
            {
                _name = _npc != null ? (Boss ? "Boss: " : string.Empty) + _npc.Name : "未设置NPC!";
                return true;
            }

            #endregion

            [ConditionalField(true, nameof(ChangeElementName))] [SerializeField] [ReadOnly]
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
            public DiziCombatUnit GetNpc() => new(teamId: 1, npc: Npc);
        }
    }
}
