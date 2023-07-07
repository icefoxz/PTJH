using System;
using GameClient.Args;
using GameClient.Modules.BattleM;
using GameClient.Modules.DiziM;
using GameClient.SoScripts.Battles;
using GameClient.SoScripts.Fields;
using GameClient.SoScripts.Items;
using MyBox;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameClient.SoScripts.ChallengeStages
{
    [CreateAssetMenu(fileName = "id_关卡", menuName = "关卡玩法/成就/关卡")]
    internal class GameStageSo : AutoAtNamingObject
    {
        [SerializeField] private int 回合限制 = 20;

        [FormerlySerializedAs("挑战")] [SerializeField]
        private CombatNpc[] 关卡;

        public int RoundLimit => 回合限制;

        public ISingleCombatNpc[] Npcs => 关卡;

        public ISingleCombatNpc GetNpc(int challengeIndex) => Npcs[challengeIndex];

        [Serializable]
        internal class CombatNpc : ISingleCombatNpc
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
            public int Hp => _npc.Hp;
            public int Mp => _npc.Mp;
            public int Strength => _npc.Strength;
            public int Agility => _npc.Agility;
            public IDiziEquipment Equipment => _npc.Equipment;
            public ICombatSet GetCombatSet() => _npc.GetCombatSet();
            public ISkillMap<ISkillInfo> ForceSkillInfo => _npc.GetForceSkillinfo();
            public ISkillMap<ICombatSkillInfo> CombatSkillInfo => _npc.GetCombatSkillInfo();
            public ISkillMap<ISkillInfo> DodgeSkillInfo => _npc.GetDodgeSkillInfo();
            public bool IsBoss => Boss;
            public string Name => _npc.Name;
            public int Level => 等级;
            public Sprite Icon => _npc.Icon;
            public string Faction => 所属势力;

            private CombatNpcSo Npc => _npc;
            public DiziCombatUnit GetDiziCombat() => new(teamId: 1, npc: Npc);
        }
    }
}